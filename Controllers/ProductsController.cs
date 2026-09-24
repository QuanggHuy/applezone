using AppleZone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppleZone.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB

        private readonly AppleZoneContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(AppleZoneContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public IActionResult Index()
        {
            var list = Product.Read(_db);
            return View(list);
        }

        public IActionResult Details(int id)
        {
            var item = _db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        public IActionResult Create()
        {
            var vm = new ProductFormViewModel
            {
                CategoryList = GetCategoryList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductFormViewModel vm)
        {
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                if (!TrySaveImage(vm.ImageFile, out var savedPath, out var error))
                {
                    ModelState.AddModelError(nameof(vm.ImageFile), error!);
                    vm.CategoryList = GetCategoryList(vm.Product.CategoryId);
                    return View(vm);
                }
                vm.Product.ImageUrl = savedPath;
            }

            if (ModelState.IsValid)
            {
                if (Product.Create(_db, ModelState, vm.Product))
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            vm.CategoryList = GetCategoryList(vm.Product.CategoryId);
            return View(vm);
        }

        public IActionResult Edit(int id)
        {
            var item = _db.Products.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            var vm = new ProductFormViewModel
            {
                Product = item,
                CategoryList = GetCategoryList(item.CategoryId)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductFormViewModel vm)
        {
            // Nếu Admin không chọn file mới, vm.Product.ImageUrl giữ nguyên giá trị cũ
            // (đến từ hidden field trong Edit.cshtml) — không bị xóa mất ảnh đang có.
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                if (!TrySaveImage(vm.ImageFile, out var savedPath, out var error))
                {
                    ModelState.AddModelError(nameof(vm.ImageFile), error!);
                    vm.CategoryList = GetCategoryList(vm.Product.CategoryId);
                    return View(vm);
                }
                vm.Product.ImageUrl = savedPath;
            }

            if (ModelState.IsValid)
            {
                if (Product.Update(_db, ModelState, vm.Product))
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            vm.CategoryList = GetCategoryList(vm.Product.CategoryId);
            return View(vm);
        }

        public IActionResult Delete(int id)
        {
            var item = _db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!Product.Destroy(_db, ModelState, id))
            {
                var item = _db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
                ViewData["ErrorMessage"] = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return View("Delete", item);
            }
            return RedirectToAction(nameof(Index));
        }

        private List<SelectListItem> GetCategoryList(int? selectedId = null)
        {
            return _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == selectedId
                }).ToList();
        }

        // Lưu file ảnh upload vào wwwroot/images/products, trả về đường dẫn tương đối để lưu vào Product.ImageUrl.
        // Đặt tên file bằng GUID (không dùng tên gốc từ người dùng) để tránh path traversal/ghi đè file khác.
        private bool TrySaveImage(IFormFile file, out string? relativePath, out string? error)
        {
            relativePath = null;
            error = null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
            {
                error = "Chỉ chấp nhận ảnh định dạng: " + string.Join(", ", AllowedImageExtensions);
                return false;
            }

            if (file.Length > MaxImageSizeBytes)
            {
                error = "Ảnh không được lớn hơn 5MB";
                return false;
            }

            var folder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            relativePath = $"/images/products/{fileName}";
            return true;
        }
    }
}
