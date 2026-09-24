namespace AppleZone.Models.Account
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
        public List<string> CurrentRoles { get; set; } = new List<string>();
    }

    public class RoleCheckbox
    {
        public string RoleName { get; set; } = null!;
        public bool IsSelected { get; set; }
    }

    public class EditUserRolesViewModel
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public List<RoleCheckbox> Roles { get; set; } = new List<RoleCheckbox>();
    }
}
