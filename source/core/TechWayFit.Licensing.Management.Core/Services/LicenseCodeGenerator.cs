using System.Security.Cryptography;
using System.Text;

namespace TechWayFit.Licensing.Management.Core.Services;

/// <summary>
/// Generates license codes in the format: XXXX-YYYY-ZZZZ-AAAA-BBBB
/// XXXX: First 2 chars of ProductId + First 2 chars of ConsumerId
/// YYYY: YY (Year) + YY (Month)
/// ZZZZ-AAAA-BBBB: Random alphanumeric segments
/// </summary>
public static class LicenseCodeGenerator
{
    // Character set excluding confusing characters (0/O, 1/I/l)
    private const string CharSet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
    private static readonly Random Random = new();
    
    /// <summary>
    /// Generates a license code based on product ID, consumer ID, and creation date
    /// </summary>
    /// <param name="productId">The product identifier</param>
    /// <param name="consumerId">The consumer identifier</param>
    /// <param name="createdDate">The license creation date</param>
    /// <returns>A formatted license code</returns>
    public static string GenerateLicenseCode(Guid productId, Guid consumerId, DateTime createdDate)
    {
        // XXXX: Product + Consumer prefixes (4 chars)
        var productPrefix = ExtractPrefix(productId, 2);
        var consumerPrefix = ExtractPrefix(consumerId, 2);
        var identifierSegment = $"{productPrefix}{consumerPrefix}";
        
        // YYYY: Year + Month (4 chars)
        var yearMonth = $"{createdDate.Year % 100:00}{createdDate.Month:00}";
        
        // ZZZZ-AAAA-BBBB: Random segments (12 chars total)
        var randomSegment1 = GenerateRandomSegment(4);
        var randomSegment2 = GenerateRandomSegment(4);
        var randomSegment3 = GenerateRandomSegment(4);
        
        return $"{identifierSegment}-{yearMonth}-{randomSegment1}-{randomSegment2}-{randomSegment3}";
    }
    
    /// <summary>
    /// Validates if a license code matches the expected format
    /// </summary>
    /// <param name="licenseCode">The license code to validate</param>
    /// <returns>True if the format is valid</returns>
    public static bool IsValidFormat(string licenseCode)
    {
        if (string.IsNullOrWhiteSpace(licenseCode))
            return false;
            
        // Check format: XXXX-YYYY-ZZZZ-AAAA-BBBB (24 chars with hyphens)
        if (licenseCode.Length != 24)
            return false;
            
        var segments = licenseCode.Split('-');
        if (segments.Length != 5)
            return false;
            
        // Each segment should be 4 characters
        if (segments.Any(segment => segment.Length != 4))
            return false;
            
        // All characters should be from our character set
        var allChars = string.Join("", segments);
        return allChars.All(c => CharSet.Contains(c));
    }
    
    /// <summary>
    /// Extracts a prefix from a GUID by taking the first N alphanumeric characters
    /// </summary>
    /// <param name="guid">The GUID to extract from</param>
    /// <param name="length">Number of characters to extract</param>
    /// <returns>Uppercase alphanumeric prefix</returns>
    private static string ExtractPrefix(Guid guid, int length)
    {
        var guidString = guid.ToString("N").ToUpperInvariant(); // Remove hyphens
        var result = new StringBuilder();
        
        foreach (char c in guidString)
        {
            if (result.Length >= length)
                break;
                
            // Convert to our character set
            if (char.IsLetterOrDigit(c))
            {
                var convertedChar = ConvertToCharSet(c);
                result.Append(convertedChar);
            }
        }
        
        // If we don't have enough characters, pad with random ones
        while (result.Length < length)
        {
            result.Append(CharSet[Random.Next(CharSet.Length)]);
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Converts a character to our safe character set
    /// </summary>
    /// <param name="c">Character to convert</param>
    /// <returns>Safe character from our character set</returns>
    private static char ConvertToCharSet(char c)
    {
        return c switch
        {
            '0' => '2',  // Convert confusing 0 to 2
            '1' => '3',  // Convert confusing 1 to 3
            'I' => 'J',  // Convert confusing I to J
            'O' => 'P',  // Convert confusing O to P
            _ when CharSet.Contains(c) => c,  // Keep if already in set
            _ => CharSet[Math.Abs(c.GetHashCode()) % CharSet.Length]  // Map others consistently
        };
    }
    
    /// <summary>
    /// Generates a random alphanumeric segment of specified length
    /// </summary>
    /// <param name="length">Length of the segment</param>
    /// <returns>Random alphanumeric string</returns>
    private static string GenerateRandomSegment(int length)
    {
        var result = new StringBuilder(length);
        
        for (int i = 0; i < length; i++)
        {
            result.Append(CharSet[Random.Next(CharSet.Length)]);
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Generates a cryptographically secure random segment
    /// </summary>
    /// <param name="length">Length of the segment</param>
    /// <returns>Cryptographically secure random string</returns>
    private static string GenerateSecureRandomSegment(int length)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        
        var result = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            result.Append(CharSet[bytes[i] % CharSet.Length]);
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Parses a license code and extracts its components
    /// </summary>
    /// <param name="licenseCode">The license code to parse</param>
    /// <returns>License code components</returns>
    public static LicenseCodeComponents? ParseLicenseCode(string licenseCode)
    {
        if (!IsValidFormat(licenseCode))
            return null;
            
        var segments = licenseCode.Split('-');
        
        // Parse date segment (YYYY)
        var dateSegment = segments[1];
        if (!int.TryParse(dateSegment.Substring(0, 2), out int year) ||
            !int.TryParse(dateSegment.Substring(2, 2), out int month))
        {
            return null;
        }
        
        // Reconstruct full year (assuming 2000s)
        var fullYear = 2000 + year;
        
        return new LicenseCodeComponents
        {
            ProductPrefix = segments[0].Substring(0, 2),
            ConsumerPrefix = segments[0].Substring(2, 2),
            Year = fullYear,
            Month = month,
            RandomSegment1 = segments[2],
            RandomSegment2 = segments[3],
            RandomSegment3 = segments[4],
            FullCode = licenseCode
        };
    }
}

/// <summary>
/// Components of a parsed license code
/// </summary>
public class LicenseCodeComponents
{
    public string ProductPrefix { get; set; } = string.Empty;
    public string ConsumerPrefix { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string RandomSegment1 { get; set; } = string.Empty;
    public string RandomSegment2 { get; set; } = string.Empty;
    public string RandomSegment3 { get; set; } = string.Empty;
    public string FullCode { get; set; } = string.Empty;
    
    public DateTime CreationDate => new DateTime(Year, Month, 1);
}
