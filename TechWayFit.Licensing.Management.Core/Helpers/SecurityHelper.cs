using System;
using System.Security.Cryptography;
using System.Text;

namespace TechWayFit.Licensing.Management.Core.Helpers;

public static class SecurityHelper
{
    /// <summary>
    /// Generates a secure random token
    /// </summary>
    /// <returns>A secure random token as a string</returns>
    public static string GenerateSecureToken()
    {
        var tokenData = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(tokenData);
        return Convert.ToBase64String(tokenData);
    }
    
    public static (string hash, string salt) HashPassword(string password)
    {
        // Generate salt
        var saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        var salt = Convert.ToBase64String(saltBytes);

        // Hash password with salt
        var hash = HashPasswordWithSalt(password, salt);

        return (hash, salt);
    }

    public static string HashPasswordWithSalt(string password, string salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string hash, string salt)
    {
        var computedHash = HashPasswordWithSalt(password, salt);
        return computedHash == hash;
    }

    public static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        
        var password = new StringBuilder();
        
        // Ensure at least one of each type
        password.Append(chars[random.Next(0, 25)]); // Uppercase
        password.Append(chars[random.Next(25, 50)]); // Lowercase
        password.Append(chars[random.Next(50, 59)]); // Number
        password.Append(chars[random.Next(59, chars.Length)]); // Special
        
        // Fill remaining positions
        for (int i = 4; i < 12; i++)
        {
            password.Append(chars[random.Next(chars.Length)]);
        }
        
        // Shuffle the password
        var shuffled = password.ToString().ToCharArray();
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        
        return new string(shuffled);
    }
}
