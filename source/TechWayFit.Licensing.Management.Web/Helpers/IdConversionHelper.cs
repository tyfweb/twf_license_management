using System;

namespace TechWayFit.Licensing.Management.Web.Helpers
{
    /// <summary>
    /// Helper class for converting between string and Guid identifiers
    /// Used to maintain string-based ViewModels while working with Guid-based services
    /// </summary>
    public static class IdConversionHelper
    {
        /// <summary>
        /// Converts a string ID to Guid. Returns Guid.Empty if conversion fails.
        /// </summary>
        /// <param name="stringId">The string representation of the ID</param>
        /// <returns>Guid representation or Guid.Empty if invalid</returns>
        public static Guid ToGuid(this string? stringId)
        {
            if (string.IsNullOrWhiteSpace(stringId))
                return Guid.Empty;

            return Guid.TryParse(stringId, out var result) ? result : Guid.Empty;
        }

        /// <summary>
        /// Converts a string ID to nullable Guid. Returns null if conversion fails.
        /// </summary>
        /// <param name="stringId">The string representation of the ID</param>
        /// <returns>Nullable Guid representation or null if invalid</returns>
        public static Guid? ToNullableGuid(this string? stringId)
        {
            if (string.IsNullOrWhiteSpace(stringId))
                return null;

            return Guid.TryParse(stringId, out var result) ? result : null;
        }

        /// <summary>
        /// Converts a Guid ID to string. Returns empty string if Guid is empty.
        /// </summary>
        /// <param name="guidId">The Guid to convert</param>
        /// <returns>String representation or empty string if Guid is empty</returns>
        public static string ConvertToString(this Guid guidId)
        {
            return guidId == Guid.Empty ? string.Empty : guidId.ToString();
        }

        /// <summary>
        /// Converts a nullable Guid ID to string. Returns empty string if null or empty.
        /// </summary>
        /// <param name="guidId">The nullable Guid to convert</param>
        /// <returns>String representation or empty string if null or empty</returns>
        public static string ConvertToString(this Guid? guidId)
        {
            return guidId?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Checks if a string ID is valid (can be parsed as Guid and is not empty)
        /// </summary>
        /// <param name="stringId">The string ID to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidId(this string? stringId)
        {
            if (string.IsNullOrWhiteSpace(stringId))
                return false;

            return Guid.TryParse(stringId, out var result) && result != Guid.Empty;
        }

        /// <summary>
        /// Checks if a Guid ID is valid (not empty)
        /// </summary>
        /// <param name="guidId">The Guid ID to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidId(this Guid guidId)
        {
            return guidId != Guid.Empty;
        }

        /// <summary>
        /// Checks if a nullable Guid ID is valid (not null and not empty)
        /// </summary>
        /// <param name="guidId">The nullable Guid ID to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidId(this Guid? guidId)
        {
            return guidId.HasValue && guidId.Value != Guid.Empty;
        }

        /// <summary>
        /// Converts a Guid to a shortened display string (first 8 characters)
        /// </summary>
        /// <param name="guidId">The Guid to convert</param>
        /// <returns>Shortened string representation</returns>
        public static string ToShortString(this Guid guidId)
        {
            return guidId == Guid.Empty ? string.Empty : guidId.ToString("N")[..8];
        }

        /// <summary>
        /// Converts a nullable Guid to a shortened display string (first 8 characters)
        /// </summary>
        /// <param name="guidId">The nullable Guid to convert</param>
        /// <returns>Shortened string representation</returns>
        public static string ToShortString(this Guid? guidId)
        {
            return guidId?.ToString("N")[..8] ?? string.Empty;
        }

        /// <summary>
        /// Creates a new Guid and returns its string representation
        /// </summary>
        /// <returns>New Guid as string</returns>
        public static string NewId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
