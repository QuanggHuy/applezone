using Microsoft.AspNetCore.Identity;

namespace AppleZone.Areas.Identity.Data
{
    public class MyUser : IdentityUser
    {
        [PersonalData]
        public string? Name { get; set; }
        [PersonalData]
        public DateTime DOB { get; set; }
        [PersonalData]
        public string? Address { get; set; }
    }
}
