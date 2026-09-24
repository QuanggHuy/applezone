using AppleZone.Areas.Identity.Data;
using AppleZone.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppleZone.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<MyUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<MyUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.OrderBy(u => u.Email).ToList();
            var list = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                list.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    Name = user.Name,
                    CurrentRoles = roles.ToList()
                });
            }

            return View(list);
        }

        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var allRoles = _roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var vm = new EditUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email!,
                Roles = allRoles.Select(r => new RoleCheckbox
                {
                    RoleName = r,
                    IsSelected = userRoles.Contains(r)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesViewModel vm)
        {
            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = vm.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

            // Không cho tự bỏ quyền Admin của chính mình — tránh tự khóa mình khỏi trang quản lý này
            var currentUserId = _userManager.GetUserId(User);
            if (vm.UserId == currentUserId && currentRoles.Contains("Admin") && !selectedRoles.Contains("Admin"))
            {
                ModelState.AddModelError(string.Empty, "Không thể tự bỏ quyền Admin của chính tài khoản đang đăng nhập.");
                vm.Email = user.Email!;
                return View(vm);
            }

            var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

            if (rolesToAdd.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }
            if (rolesToRemove.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
