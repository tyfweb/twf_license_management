namespace TechWayFit.Licensing.WebUI.Models.Authentication
{
    public class User
    {
        public string Name { get; set; }
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
