using System.Globalization;
using System.Text.RegularExpressions;

namespace ITI.ERP.Shared.Extensions;

public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }

    public static string ToSlug(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var slug = input.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }

    public static string Truncate(this string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (input.Length <= maxLength)
            return input;

        return input[..maxLength] + "...";
    }

    public static bool IsNullOrEmpty(this string? input)
    {
        return string.IsNullOrEmpty(input);
    }

    public static bool IsNotNullOrEmpty(this string? input)
    {
        return !string.IsNullOrEmpty(input);
    }
}
