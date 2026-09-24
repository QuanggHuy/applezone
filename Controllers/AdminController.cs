using AppleZone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppleZone.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppleZoneContext _context;

        public AdminController(AppleZoneContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? year, int? month)
        {
            // Chọn Tháng mà chưa chọn Năm thì vô nghĩa (tháng của năm nào?) — bỏ qua bộ lọc Tháng, báo rõ cho user biết
            if (year == null && month != null)
            {
                month = null;
                ViewBag.WarningMessage = "Bạn cần chọn Năm trước khi chọn Tháng — đang hiển thị dữ liệu tất cả thời gian.";
            }

            var model = DashboardViewModel.Read(_context, year, month);
            return View(model);
        }
    }
}
