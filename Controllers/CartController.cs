using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using AppleZone.Helper;
using AppleZone.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AppleZone.Controllers
{
    public class CartController : Controller
    {
        private readonly AppleZoneContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ILogger<CartController> logger, AppleZoneContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Order(Dictionary<int, int>? quantities)
        {
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();

            // Áp dụng đúng số lượng đang hiển thị trên màn hình lúc bấm "Đặt hàng",
            // không phụ thuộc user có bấm "Lưu" trước đó hay không.
            if (quantities != null)
            {
                foreach (var kv in quantities)
                {
                    cart.Update(kv.Key, kv.Value);
                }
                HttpContext.Session.Set<Cart>("cart", cart);
            }

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var mstate = new ModelStateDictionary();

            if (AppleZone.Models.Order.Checkout(_context, mstate, customerId, cart, out int orderId))
            {
                HttpContext.Session.Remove("cart");
                ViewData["OrderId"] = orderId;
                return View(cart);
            }

            ViewData["ErrorMessage"] = string.Join("; ", mstate.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(cart);
        }

        public async Task<IActionResult> Add(int id)
        {
            Product? p = _context.Products.Include(p => p.Category).First(p => p.Id == id);

            Item item = new Item
            {
                Id = p.Id,
                Category = p.Category.Name,
                Name = p.Name,
                Discount = p.Discount,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Quantity = 1
            };
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            cart.Add(item);
            HttpContext.Session.Set<Cart>("cart", cart);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Remove(int id)
        {
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            cart.Remove(id);
            HttpContext.Session.Set<Cart>("cart", cart);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Empty()
        {
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            cart.Empty();
            HttpContext.Session.Set<Cart>("cart", cart);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(int id, int quantity)
        {
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            cart.Update(id, quantity);
            HttpContext.Session.Set<Cart>("cart", cart);
            return RedirectToAction("Index");
        }

    }
}
