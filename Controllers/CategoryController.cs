using AppleZone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppleZone.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly AppleZoneContext _db;

        public CategoryController(AppleZoneContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var list = Category.Read(_db);
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category model)
        {
            if (ModelState.IsValid)
            {
                if (Category.Create(_db, ModelState, model))
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var item = _db.Categories.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category model)
        {
            if (ModelState.IsValid)
            {
                if (Category.Update(_db, ModelState, model))
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var item = _db.Categories.Find(id);
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
            if (!Category.Destroy(_db, ModelState, id))
            {
                var item = _db.Categories.Find(id);
                ViewData["ErrorMessage"] = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return View("Delete", item);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}