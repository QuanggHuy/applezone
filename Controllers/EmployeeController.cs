using AppleZone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AppleZone.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class EmployeeController : Controller
    {
        private readonly AppleZoneContext _db;

        public EmployeeController(AppleZoneContext db)
        {
            _db = db;
        }

        public IActionResult Orders()
        {
            var list = Order.ReadAll(_db);
            return View(list);
        }

        public IActionResult OrderDetail(int id)
        {
            var order = Order.ReadDetailForStaff(_db, id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string status)
        {
            var mstate = new ModelStateDictionary();
            if (!Order.UpdateStatus(_db, mstate, id, status))
            {
                TempData["ErrorMessage"] = string.Join("; ", mstate.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var mstate = new ModelStateDictionary();
            if (!Order.Cancel(_db, mstate, id, null, isStaff: true))
            {
                TempData["ErrorMessage"] = string.Join("; ", mstate.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }
            return RedirectToAction(nameof(OrderDetail), new { id });
        }
    }
}
