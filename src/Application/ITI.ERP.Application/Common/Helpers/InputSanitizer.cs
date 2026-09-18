using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ITI.ERP.Application.Common.Helpers;

public static partial class InputSanitizer
{
    public static string TrimAndNormalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.Trim().Normalize(NormalizationForm.FormC);
    }

    public static string ForLog(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var trimmed = input.Trim();
        trimmed = Regex.Replace(trimmed, @"[\r\n\t]", " ");
        trimmed = WebUtility.HtmlEncode(trimmed);
        return trimmed.Length > 200 ? trimmed[..200] + "..." : trimmed;
    }

    public static string StripHtmlTags(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return Regex.Replace(input.Trim(), "<.*?>", string.Empty);
    }

    [GeneratedRegex(@"<.*?>")]
    private static partial Regex HtmlTagRegex();
}
