using System.Reflection;
using System.Text.Json;

namespace ITI.ERP.Application.Common.Helpers;

public static class AuditSanitizerHelper
{
    private static readonly HashSet<string> SensitivePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash", "Password", "PasswordHash", "CurrentPassword", "NewPassword",
        "AadharNumber", "GuardianPhone", "Phone", "TokenHash", "Token",
        "RefreshToken", "AccessToken", "Secret", "ApiKey"
    };

    public static object? Sanitize(object? values)
    {
        if (values is null) return null;

        if (values is JsonElement jsonElement)
            return SanitizeJsonElement(jsonElement);

        if (values is IDictionary<string, object?> dict)
        {
            var sanitized = new Dictionary<string, object?>();
            foreach (var kvp in dict)
            {
                if (SensitivePropertyNames.Contains(kvp.Key))
                    sanitized[kvp.Key] = "***REDACTED***";
                else
                    sanitized[kvp.Key] = kvp.Value;
            }
            return sanitized;
        }

        var properties = values.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var result = new Dictionary<string, object?>();
        foreach (var prop in properties)
        {
            if (SensitivePropertyNames.Contains(prop.Name))
                result[prop.Name] = "***REDACTED***";
            else
                result[prop.Name] = prop.GetValue(values);
        }
        return result;
    }

    private static object SanitizeJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    if (SensitivePropertyNames.Contains(prop.Name))
                        dict[prop.Name] = "***REDACTED***";
                    else
                        dict[prop.Name] = SanitizeJsonElement(prop.Value);
                }
                return dict;

            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in element.EnumerateArray())
                    list.Add(SanitizeJsonElement(item));
                return list;

            default:
                return element.ToString();
        }
    }
}