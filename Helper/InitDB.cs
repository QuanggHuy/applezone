using AppleZone.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace AppleZone.Helper
{
    public class InitDB
    {
        public static async void Execute(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MyUser>>();
                await CreateRoles(roleManager);
                await CreateAdmin(userManager);
            }
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            Console.WriteLine("****: Creating roles...");
            string[] roleNames = { "Admin", "Employee", "Customer" };
            IdentityResult roleResult;
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            Console.WriteLine("****: Roles already for use");
        }

        private static async Task CreateAdmin(UserManager<MyUser> userManager)
        {
            Console.WriteLine("****: Creating the admin user...");
            string email = "admin@gmail.com";
            var admin = new MyUser
            {
                Name = "Administrator",
                DOB = DateTime.Now,
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            string pwd = "1";

            var _user = await userManager.FindByEmailAsync(admin.Email);
            if (_user == null)
            {
                var createAdmin = await userManager.CreateAsync(admin, pwd);
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                Console.WriteLine("****: Result: " + createAdmin.ToString());
            }
        }
    }
}
