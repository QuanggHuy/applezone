using Microsoft.EntityFrameworkCore;

namespace AppleZone.Models;

public class TopProductItem
{
    public string Name { get; set; } = null!;
    public int TotalSold { get; set; }
}

public class DashboardViewModel
{
    public double TotalRevenue { get; set; }
    public int PendingCount { get; set; }
    public int ShippingCount { get; set; }
    public int DeliveredCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public List<TopProductItem> TopProducts { get; set; } = new();
    public List<Product> LowStockProducts { get; set; } = new();

    public int? SelectedYear { get; set; }
    public int? SelectedMonth { get; set; }
    public List<int> AvailableYears { get; set; } = new();

    private const int LowStockThreshold = 5;

    public static DashboardViewModel Read(AppleZoneContext db, int? year, int? month)
    {
        // Doanh thu tính trên tất cả đơn hàng TRỪ đơn đã hủy (Cancelled)
        var revenueOrderDetails = db.OrderDetails
            .Include(od => od.Order)
            .Where(od => od.Order.Status != OrderStatus.Cancelled);

        var ordersQuery = db.Orders.AsQueryable();

        if (year != null)
        {
            revenueOrderDetails = revenueOrderDetails.Where(od => od.Order.Date.Year == year);
            ordersQuery = ordersQuery.Where(o => o.Date.Year == year);
        }
        if (month != null)
        {
            revenueOrderDetails = revenueOrderDetails.Where(od => od.Order.Date.Month == month);
            ordersQuery = ordersQuery.Where(o => o.Date.Month == month);
        }

        var model = new DashboardViewModel
        {
            SelectedYear = year,
            SelectedMonth = month,
            AvailableYears = db.Orders.Select(o => o.Date.Year).Distinct().OrderByDescending(y => y).ToList(),

            TotalRevenue = revenueOrderDetails
                .Select(od => od.Quantity * od.Price * (1 - od.Discount))
                .Sum(),

            PendingCount = ordersQuery.Count(o => o.Status == OrderStatus.Pending),
            ShippingCount = ordersQuery.Count(o => o.Status == OrderStatus.Shipping),
            DeliveredCount = ordersQuery.Count(o => o.Status == OrderStatus.Delivered),
            CancelledCount = ordersQuery.Count(o => o.Status == OrderStatus.Cancelled),

            TotalCustomers = ordersQuery.Select(o => o.CustomerId).Distinct().Count(),
            TotalProducts = db.Products.Count(),
            TotalCategories = db.Categories.Count(),

            TopProducts = revenueOrderDetails
                .GroupBy(od => od.Product.Name)
                .Select(g => new TopProductItem { Name = g.Key, TotalSold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList(),

            // Tồn kho là trạng thái hiện tại, không lọc theo tháng
            LowStockProducts = db.Products
                .Where(p => p.StockQuantity <= LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToList(),
        };

        return model;
    }
}
