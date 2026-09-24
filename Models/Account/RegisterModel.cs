using System.ComponentModel.DataAnnotations;
using AppleZone.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AppleZone.Models.Account
{
    public class RegisterModel
    {
        [Required]
        [Display(Name = "Full name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public static async Task<MyUser> Create(UserManager<MyUser> userManager, ModelStateDictionary mstate, RegisterModel m)
        {
            var user = new MyUser
            {
                UserName = m.Email,
                Email = m.Email,
                Name = m.Name,
                DOB = m.DOB,
                Address = m.Address
            };

            var result = await userManager.CreateAsync(user, m.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    mstate.AddModelError(string.Empty, error.Description);
                }
                return null;
            }

            await userManager.AddToRoleAsync(user, "Customer");
            return user;
        }
    }
}
