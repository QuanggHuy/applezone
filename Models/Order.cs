using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using AppleZone.Data;
using AppleZone.Helper;

namespace AppleZone.Models;

public static class OrderStatus
{
    public const string Pending = "Pending";
    public const string Shipping = "Shipping";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";

    // Thứ tự hợp lệ khi chuyển trạng thái (không cho nhảy cóc)
    public static readonly Dictionary<string, string> NextStatus = new()
    {
        { Pending, Shipping },
        { Shipping, Delivered },
    };
}

public partial class Order
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string? Status { get; set; }

    public string? CustomerId { get; set; }

    public string? EmployeeId { get; set; }

    public virtual AspNetUser? Customer { get; set; }

    public virtual AspNetUser? Employee { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public static List<Order> ReadByCustomer(AppleZoneContext db, string customerId)
    {
        return db.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.Date)
            .ToList();
    }

    public static Order? ReadDetail(AppleZoneContext db, int id, string customerId)
    {
        return db.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefault(o => o.Id == id && o.CustomerId == customerId);
    }

    // Danh sách TOÀN BỘ đơn hàng, không lọc theo CustomerId — dùng cho nhân viên/admin xử lý đơn.
    public static List<Order> ReadAll(AppleZoneContext db)
    {
        return db.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .OrderByDescending(o => o.Date)
            .ToList();
    }

    // Chi tiết 1 đơn bất kỳ (không giới hạn CustomerId) — dùng cho nhân viên/admin.
    public static Order? ReadDetailForStaff(AppleZoneContext db, int id)
    {
        return db.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefault(o => o.Id == id);
    }

    public static bool Checkout(AppleZoneContext db, ModelStateDictionary mstate, string? customerId, Cart cart, out int orderId)
    {
        orderId = 0;

        if (cart.List.Count == 0)
        {
            mstate.AddModelError(string.Empty, "Giỏ hàng đang trống");
            return false;
        }

        using var tran = db.Database.BeginTransaction();
        try
        {
            foreach (var item in cart.List.Values)
            {
                // Khóa dòng ngay tại đây (giống SELECT FOR UPDATE bên Postgres),
                // tránh 2 request cùng trừ tồn kho song song gây bán vượt tồn kho (race condition).
                var product = db.Products
                    .FromSqlInterpolated($"SELECT * FROM Product WITH (UPDLOCK, ROWLOCK) WHERE Id = {item.Id}")
                    .FirstOrDefault();

                if (product == null)
                {
                    mstate.AddModelError(string.Empty, $"Sản phẩm '{item.Name}' không còn tồn tại");
                    tran.Rollback();
                    return false;
                }

                if (product.StockQuantity < item.Quantity)
                {
                    mstate.AddModelError(string.Empty, $"Sản phẩm '{item.Name}' chỉ còn {product.StockQuantity} trong kho (bạn đặt {item.Quantity})");
                    tran.Rollback();
                    return false;
                }

                product.StockQuantity -= item.Quantity;
            }

            var order = new Order
            {
                Date = DateTime.Now,
                Status = OrderStatus.Pending,
                CustomerId = customerId,
            };
            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in cart.List.Values)
            {
                db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.Id,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Discount = item.Discount,
                });
            }
            db.SaveChanges();

            tran.Commit();
            orderId = order.Id;
            return true;
        }
        catch (Exception)
        {
            tran.Rollback();
            mstate.AddModelError(string.Empty, "Đặt hàng thất bại, vui lòng thử lại");
            return false;
        }
    }

    public static bool Cancel(AppleZoneContext db, ModelStateDictionary mstate, int orderId, string? customerId, bool isStaff)
    {
        var order = db.Orders.Include(o => o.OrderDetails).FirstOrDefault(o => o.Id == orderId);
        if (order == null)
        {
            mstate.AddModelError(string.Empty, "Đơn hàng không tồn tại");
            return false;
        }

        if (!isStaff)
        {
            if (order.CustomerId != customerId)
            {
                mstate.AddModelError(string.Empty, "Đơn hàng không tồn tại"); // không lộ đơn hàng của người khác (IDOR)
                return false;
            }
            if (order.Status != OrderStatus.Pending)
            {
                mstate.AddModelError(string.Empty, "Đơn hàng đang được xử lý, không thể tự hủy");
                return false;
            }
        }
        else if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
        {
            mstate.AddModelError(string.Empty, $"Không thể hủy đơn đang ở trạng thái '{order.Status}'");
            return false;
        }

        using var tran = db.Database.BeginTransaction();
        try
        {
            // Hoàn lại tồn kho — đối xứng ngược lại với Checkout(), cũng dùng khóa
            // (UPDLOCK, ROWLOCK) để tránh 2 giao dịch cùng sửa tồn kho 1 sản phẩm cùng lúc.
            foreach (var detail in order.OrderDetails)
            {
                var product = db.Products
                    .FromSqlInterpolated($"SELECT * FROM Product WITH (UPDLOCK, ROWLOCK) WHERE Id = {detail.ProductId}")
                    .FirstOrDefault();
                if (product != null)
                {
                    product.StockQuantity += detail.Quantity;
                }
            }

            order.Status = OrderStatus.Cancelled;
            db.SaveChanges();
            tran.Commit();
            return true;
        }
        catch (Exception)
        {
            tran.Rollback();
            mstate.AddModelError(string.Empty, "Hủy đơn thất bại, vui lòng thử lại");
            return false;
        }
    }

    public static bool UpdateStatus(AppleZoneContext db, ModelStateDictionary mstate, int orderId, string newStatus)
    {
        var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
        {
            mstate.AddModelError(string.Empty, "Đơn hàng không tồn tại");
            return false;
        }

        if (!OrderStatus.NextStatus.TryGetValue(order.Status ?? "", out var expectedNext) || expectedNext != newStatus)
        {
            mstate.AddModelError(string.Empty, $"Không thể chuyển từ '{order.Status}' sang '{newStatus}'");
            return false;
        }

        order.Status = newStatus;
        db.SaveChanges();
        return true;
    }
}
