using AppleZone.Helper;
using AppleZone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Dynamic.Core;

namespace AppleZone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppleZoneContext _context;

        public HomeController(ILogger<HomeController> logger, AppleZoneContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int? pageIndex,
            int? categoryId, string? name, double? fromPrice, double? toPrice, double? fromDiscount, double? toDiscount,
            string? orderBy, string? orderType,
            string? op)
        {
            var products = (IQueryable<Product>)_context.Products.Include(p => p.Category);
            products = Filter(products, categoryId, name, fromPrice, toPrice, fromDiscount, toDiscount, op);
            products = Sort(products, orderBy, orderType, op);
            const int pageSize = 3;
            return View(await PaginatedList<Product>.CreateAsync(products, pageIndex ?? 1, pageSize));
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IQueryable<Product> Filter(IQueryable<Product> products, int? categoryId, string? name, double? fromPrice, double? toPrice, double? fromDiscount, double? toDiscount, string? op)
        {
            FilterInfo filterInfo = new FilterInfo();
            switch (op)
            {
                case "search-do":
                    filterInfo = new FilterInfo
                    {
                        CategoryId = categoryId,
                        Name = name,
                        FromPrice = fromPrice,
                        ToPrice = toPrice,
                        FromDiscount = fromDiscount,
                        ToDiscount = toDiscount,
                    };
                    break;
                case "search-clear":
                    break;
                default:
                    filterInfo = HttpContext.Session.Get<FilterInfo>("filterInfo") ?? filterInfo;
                    break;
            }
            HttpContext.Session.Set<FilterInfo>("filterInfo", filterInfo);
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", filterInfo.CategoryId);
            ViewBag.FilterInfo = filterInfo;
            if (filterInfo.CategoryId != null && filterInfo.CategoryId != -1)
            {
                products = products.Where(p => p.CategoryId == filterInfo.CategoryId);
            }
            if (!string.IsNullOrEmpty(filterInfo.Name))
            {
                products = products.Where(p => p.Name.Contains(filterInfo.Name));
            }
            if (filterInfo.FromPrice != null)
            {
                products = products.Where(p => p.Price >= filterInfo.FromPrice);
            }
            if (filterInfo.ToPrice != null)
            {
                products = products.Where(p => p.Price <= filterInfo.ToPrice);
            }
            if (filterInfo.FromDiscount != null)
            {
                products = products.Where(p => p.Discount >= filterInfo.FromDiscount);
            }
            if (filterInfo.ToDiscount != null)
            {
                products = products.Where(p => p.Discount <= filterInfo.ToDiscount);
            }
            return products;
        }
        private IQueryable<Product> Sort(IQueryable<Product> products, string? orderBy, string? orderType, string? op)
        {
            //Must run: Install-Package System.Linq.Dynamic.Core
            SortInfo sortInfo = new SortInfo { OrderBy = "Id", OrderType = "ASC" };
            switch (op)
            {
                case "sort-do":
                    sortInfo = new SortInfo { OrderBy = orderBy, OrderType = orderType };
                    break;
                case "sort-clear":
                    break;
                default:
                    sortInfo = HttpContext.Session.Get<SortInfo>("sortInfo") ?? sortInfo;
                    break;
            }
            HttpContext.Session.Set<SortInfo>("sortInfo", sortInfo);
            List<SelectListItem> fieldList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Id", Text = "Mã sản phẩm" },
                new SelectListItem { Value = "Name", Text = "Tên sản phẩm" },
                new SelectListItem { Value = "Discount", Text = "Giảm giá" },
                new SelectListItem { Value = "Price", Text = "Giá" },
                new SelectListItem { Value = "Category", Text = "Danh mục" },
            };
            ViewBag.FieldList = new SelectList(fieldList, "Value", "Text", sortInfo.OrderBy);
            ViewBag.SortInfo = sortInfo;
            // Category là navigation property (object), phải sort theo Category.Name thì mới đúng nghĩa
            string orderByField = sortInfo.OrderBy == "Category" ? "Category.Name" : sortInfo.OrderBy;
            return products.OrderBy($"{orderByField} {sortInfo.OrderType}");
        }

    }
}
