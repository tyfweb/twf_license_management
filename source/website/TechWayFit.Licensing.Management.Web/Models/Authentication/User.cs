namespace TechWayFit.Licensing.Management.Web.Models.Authentication
{
    public class User
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    public class AuthenticationSettings
    {
        public List<User> Users { get; set; } = new();
    }

    public class LoginViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
