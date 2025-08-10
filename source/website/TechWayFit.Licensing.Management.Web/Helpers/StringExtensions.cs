public static class StringExtensions
{
    /// <summary>
    /// Converts a string to title case (first letter of each word capitalized)
    /// </summary>
    /// <param name="input">The input string</param>
    /// <returns>A title-cased version of the input string</returns>
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Select(word =>
            char.ToUpperInvariant(word[0]) + (word.Length > 1 ? word[1..].ToLowerInvariant() : "")));
    }
}