using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using AppleZone.Data;

namespace AppleZone.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(200)]
    [Display(Name = "Tên sản phẩm")]
    public string Name { get; set; } = null!;

    [StringLength(1000)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
    [Display(Name = "Giá")]
    public double Price { get; set; }

    [Range(0, 1, ErrorMessage = "Giảm giá phải trong khoảng 0 - 1 (ví dụ 0.1 = giảm 10%)")]
    [Display(Name = "Giảm giá")]
    public double Discount { get; set; }

    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    [Display(Name = "Thương hiệu")]
    public string? Brand { get; set; }

    [Display(Name = "Xuất xứ")]
    public string? Origin { get; set; }

    [Display(Name = "Bảo hành")]
    public string? Guarantee { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Tồn kho không được âm")]
    [Display(Name = "Tồn kho")]
    public int StockQuantity { get; set; }

    [Display(Name = "Ảnh sản phẩm")]
    public string? ImageUrl { get; set; }

    [ValidateNever]
    public virtual Category Category { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public static List<Product> Read(AppleZoneContext db)
    {
        return db.Products.Include(p => p.Category).OrderBy(p => p.Name).ToList();
    }

    public static bool Create(AppleZoneContext db, ModelStateDictionary mstate, Product m)
    {
        var categoryExists = db.Categories.Any(c => c.Id == m.CategoryId);
        if (!categoryExists)
        {
            mstate.AddModelError(nameof(Product.CategoryId), "Danh mục không tồn tại");
            return false;
        }

        db.Products.Add(m);
        db.SaveChanges();
        return true;
    }

    public static bool Update(AppleZoneContext db, ModelStateDictionary mstate, Product m)
    {
        var item = db.Products.FirstOrDefault(p => p.Id == m.Id);
        if (item == null)
        {
            mstate.AddModelError(string.Empty, "Sản phẩm cần sửa không tồn tại");
            return false;
        }

        var categoryExists = db.Categories.Any(c => c.Id == m.CategoryId);
        if (!categoryExists)
        {
            mstate.AddModelError(nameof(Product.CategoryId), "Danh mục không tồn tại");
            return false;
        }

        item.Name = m.Name;
        item.Description = m.Description;
        item.Price = m.Price;
        item.Discount = m.Discount;
        item.CategoryId = m.CategoryId;
        item.Brand = m.Brand;
        item.Origin = m.Origin;
        item.Guarantee = m.Guarantee;
        item.StockQuantity = m.StockQuantity;
        item.ImageUrl = m.ImageUrl;
        db.SaveChanges();
        return true;
    }

    public static bool Destroy(AppleZoneContext db, ModelStateDictionary mstate, int id)
    {
        var item = db.Products.FirstOrDefault(p => p.Id == id);
        if (item == null)
        {
            mstate.AddModelError(string.Empty, "Sản phẩm cần xóa không tồn tại");
            return false;
        }

        var conOrderDetail = db.OrderDetails.Any(od => od.ProductId == id);
        if (conOrderDetail)
        {
            mstate.AddModelError(string.Empty, "Không thể xóa vì sản phẩm đã có trong đơn hàng");
            return false;
        }

        db.Products.Remove(item);
        db.SaveChanges();
        return true;
    }
}
