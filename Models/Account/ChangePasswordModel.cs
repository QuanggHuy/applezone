using System.ComponentModel.DataAnnotations;
using AppleZone.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AppleZone.Models.Account
{
    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        public static async Task<bool> Update(UserManager<MyUser> userManager, ModelStateDictionary mstate, MyUser user, ChangePasswordModel m)
        {
            var result = await userManager.ChangePasswordAsync(user, m.CurrentPassword, m.NewPassword);
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
