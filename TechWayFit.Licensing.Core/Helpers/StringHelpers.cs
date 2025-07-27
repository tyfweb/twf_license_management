using System;

namespace TechWayFit.Licensing.Core.Helpers;

public static class StringHelpers
{
    public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct,Enum
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));

        if (!Enum.TryParse<TEnum>(value, true, out var result))
            throw new ArgumentException($"'{value}' is not a valid value for {typeof(TEnum).Name}", nameof(value));

        return result;
    }

    public static string ToUpperInvariant(this string value)
    {
        return value.ToUpperInvariant();
    }
}
