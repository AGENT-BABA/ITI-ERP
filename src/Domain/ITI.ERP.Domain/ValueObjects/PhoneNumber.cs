using System.Text.RegularExpressions;

namespace ITI.ERP.Domain.ValueObjects;

public record PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phone));

        var cleaned = phone.Trim();
        if (!Regex.IsMatch(cleaned, @"^\d{10}$"))
            throw new ArgumentException("Phone number must be exactly 10 digits.", nameof(phone));

        return new PhoneNumber(cleaned);
    }

    public static PhoneNumber? TryCreate(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var cleaned = phone.Trim();
        if (!Regex.IsMatch(cleaned, @"^\d{10}$"))
            return null;

        return new PhoneNumber(cleaned);
    }

    public override string ToString() => Value;
}
