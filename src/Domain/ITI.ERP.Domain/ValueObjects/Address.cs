namespace ITI.ERP.Domain.ValueObjects;

public record Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PinCode { get; }

    private Address(string street, string city, string state, string pinCode)
    {
        Street = street;
        City = city;
        State = state;
        PinCode = pinCode;
    }

    public static Address Create(string street, string city, string state, string pinCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty.", nameof(state));
        if (!string.IsNullOrWhiteSpace(pinCode) && pinCode.Length != 6)
            throw new ArgumentException("PinCode must be exactly 6 digits.", nameof(pinCode));

        return new Address(street.Trim(), city.Trim(), state.Trim(), pinCode?.Trim() ?? string.Empty);
    }

    public static Address? TryCreate(string? street, string? city, string? state, string? pinCode)
    {
        if (string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state))
            return null;

        return new Address(street.Trim(), city.Trim(), state.Trim(), pinCode?.Trim() ?? string.Empty);
    }
}
