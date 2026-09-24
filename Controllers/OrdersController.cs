using AppleZone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace AppleZone.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppleZoneContext _db;

        public OrdersController(AppleZoneContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var list = Order.ReadByCustomer(_db, customerId!);
            return View(list);
        }

        public IActionResult Details(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = Order.ReadDetail(_db, id, customerId!);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var mstate = new ModelStateDictionary();
            if (!Order.Cancel(_db, mstate, id, customerId, isStaff: false))
            {
                TempData["ErrorMessage"] = string.Join("; ", mstate.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
