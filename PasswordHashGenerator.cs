using System;
using System.Security.Cryptography;
using System.Text;

public class PasswordHashGenerator
{
    public static void Main()
    {
        // Generate correct hashes for the passwords using the same method as SecurityHelper
        var passwords = new[]
        {
            ("admin", "Admin@123", "admin_salt_2024"),
            ("manager", "Manager@123", "manager_salt_2024"),
            ("user", "User@123", "user_salt_2024"),
            ("john.doe", "Demo@123", "john_salt_2024"),
            ("jane.smith", "Demo@123", "jane_salt_2024"),
            ("mike.wilson", "Demo@123", "mike_salt_2024")
        };

        foreach (var (username, password, salt) in passwords)
        {
            var hash = HashPasswordWithSalt(password, salt);
            Console.WriteLine($"-- {username} - Password: {password}");
            Console.WriteLine($"Hash: '{hash}'");
            Console.WriteLine($"Salt: '{salt}'");
            Console.WriteLine();
        }
    }

    public static string HashPasswordWithSalt(string password, string salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
