using System.ComponentModel.DataAnnotations;

namespace ITI.ERP.Domain.ValueObjects;

public record EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        var trimmed = email.Trim();
        if (trimmed.Length > 200)
            throw new ArgumentException("Email cannot exceed 200 characters.", nameof(email));

        var validator = new EmailAddressAttribute();
        if (!validator.IsValid(trimmed))
            throw new ArgumentException("Invalid email format.", nameof(email));

        return new EmailAddress(trimmed);
    }

    public static EmailAddress? TryCreate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var trimmed = email.Trim();
        if (trimmed.Length > 200)
            return null;

        var validator = new EmailAddressAttribute();
        if (!validator.IsValid(trimmed))
            return null;

        return new EmailAddress(trimmed);
    }

    public override string ToString() => Value;
}
