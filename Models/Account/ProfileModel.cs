using System.ComponentModel.DataAnnotations;
using AppleZone.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AppleZone.Models.Account
{
    public class ProfileModel
    {
        [Required]
        [Display(Name = "Full name")]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = null!;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        public static ProfileModel Read(MyUser user)
        {
            return new ProfileModel
            {
                Name = user.Name ?? "",
                DOB = user.DOB,
                Address = user.Address ?? "",
                Email = user.Email,
            };
        }

        public static async Task<bool> Update(UserManager<MyUser> userManager, ModelStateDictionary mstate, MyUser user, ProfileModel m)
        {
            user.Name = m.Name;
            user.DOB = m.DOB;
            user.Address = m.Address;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    mstate.AddModelError(string.Empty, error.Description);
                }
                return false;
            }

            return true;
        }
    }
}
