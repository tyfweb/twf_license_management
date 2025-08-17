using System.Security.Cryptography;
using System.Text;

namespace TechWayFit.Licensing.Management.Core.Utilities;

/// <summary>
/// Utility for generating and validating product keys in XXXX-XXXX-XXXX-XXXX format
/// </summary>
public static class ProductKeyGenerator
{
    private const string ALLOWED_CHARS = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789"; // Excludes O, 0 for clarity
    private const int GROUP_SIZE = 4;
    private const int GROUP_COUNT = 4;
    private const int TOTAL_LENGTH = GROUP_SIZE * GROUP_COUNT;

    /// <summary>
    /// Generates a new product key in XXXX-XXXX-XXXX-XXXX format
    /// </summary>
    /// <returns>Formatted product key</returns>
    public static string GenerateProductKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var keyBytes = new byte[TOTAL_LENGTH];
        rng.GetBytes(keyBytes);

        var keyBuilder = new StringBuilder();
        
        for (int i = 0; i < TOTAL_LENGTH; i++)
        {
            // Add separator after each group
            if (i > 0 && i % GROUP_SIZE == 0)
            {
                keyBuilder.Append('-');
            }
            
            // Get random character from allowed set
            var charIndex = keyBytes[i] % ALLOWED_CHARS.Length;
            keyBuilder.Append(ALLOWED_CHARS[charIndex]);
        }

        return keyBuilder.ToString();
    }

    /// <summary>
    /// Validates the format of a product key
    /// </summary>
    /// <param name="productKey">Product key to validate</param>
    /// <returns>True if format is valid</returns>
    public static bool ValidateProductKeyFormat(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
            return false;

        // Check length (16 chars + 3 separators = 19)
        if (productKey.Length != 19)
            return false;

        var parts = productKey.Split('-');
        
        // Should have exactly 4 parts
        if (parts.Length != GROUP_COUNT)
            return false;

        // Each part should be exactly 4 characters
        foreach (var part in parts)
        {
            if (part.Length != GROUP_SIZE)
                return false;

            // Each character should be in allowed set
            foreach (var c in part)
            {
                if (!ALLOWED_CHARS.Contains(c))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Normalizes a product key by removing spaces and converting to uppercase
    /// </summary>
    /// <param name="productKey">Product key to normalize</param>
    /// <returns>Normalized product key</returns>
    public static string NormalizeProductKey(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
            return string.Empty;

        return productKey.Replace(" ", "").Replace("\t", "").ToUpperInvariant();
    }

    /// <summary>
    /// Generates multiple unique product keys
    /// </summary>
    /// <param name="count">Number of keys to generate</param>
    /// <returns>List of unique product keys</returns>
    public static List<string> GenerateMultipleProductKeys(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than zero", nameof(count));

        var keys = new HashSet<string>();
        
        while (keys.Count < count)
        {
            var key = GenerateProductKey();
            keys.Add(key);
        }

        return keys.ToList();
    }

    /// <summary>
    /// Checks if a product key is in the correct format and normalizes it
    /// </summary>
    /// <param name="productKey">Input product key</param>
    /// <param name="normalizedKey">Normalized product key output</param>
    /// <returns>True if valid and normalized successfully</returns>
    public static bool TryNormalizeProductKey(string productKey, out string normalizedKey)
    {
        normalizedKey = string.Empty;

        if (string.IsNullOrWhiteSpace(productKey))
            return false;

        var normalized = NormalizeProductKey(productKey);
        
        if (!ValidateProductKeyFormat(normalized))
            return false;

        normalizedKey = normalized;
        return true;
    }
}
