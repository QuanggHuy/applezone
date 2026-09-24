using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AppleZone.Data;

namespace AppleZone.Models;

public partial class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [Display(Name = "Tên danh mục")]
    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public static List<Category> Read(AppleZoneContext db)
    {
        return db.Categories.OrderBy(c => c.Name).ToList();
    }

     public static bool Create(AppleZoneContext db, ModelStateDictionary mstate, Category m)
    {
        var trung = db.Categories.Any(c => c.Name.ToLower() == m.Name.ToLower());
        if (trung)
        {
            mstate.AddModelError(nameof(Category.Name), "Tên danh mục đã tồn tại");
            return false;
        }

        db.Categories.Add(m);
        db.SaveChanges();
        return true;
    }

    public static bool Update(AppleZoneContext db, ModelStateDictionary mstate, Category m)
    {
        var item = db.Categories.FirstOrDefault(c => c.Id == m.Id);
        if (item == null)
        {
            mstate.AddModelError(string.Empty, "Danh mục cần sửa không tồn tại");
            return false;
        }

        var trung = db.Categories.Any(c => c.Id != m.Id && c.Name.ToLower() == m.Name.ToLower());
        if (trung)
        {
            mstate.AddModelError(nameof(Category.Name), "Tên danh mục đã tồn tại");
            return false;
        }

        item.Name = m.Name;
        db.SaveChanges();
        return true;
    }

    public static bool Destroy(AppleZoneContext db, ModelStateDictionary mstate, int id)
    {
        var item = db.Categories.FirstOrDefault(c => c.Id == id);
        if (item == null)
        {
            mstate.AddModelError(string.Empty, "Danh mục cần xóa không tồn tại");
            return false;
        }

        var conSanPham = db.Products.Any(p => p.CategoryId == id);
        if (conSanPham)
        {
            mstate.AddModelError(string.Empty, "Không thể xóa vì danh mục đang có sản phẩm bên trong");
            return false;
        }

        db.Categories.Remove(item);
        db.SaveChanges();
        return true;
    }
}
