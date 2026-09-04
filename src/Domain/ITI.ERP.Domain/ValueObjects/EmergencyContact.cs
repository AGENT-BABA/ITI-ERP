namespace ITI.ERP.Domain.ValueObjects;

public record EmergencyContact
{
    public string Name { get; }
    public PhoneNumber Phone { get; }
    public string Relation { get; }

    private EmergencyContact(string name, PhoneNumber phone, string relation)
    {
        Name = name;
        Phone = phone;
        Relation = relation;
    }

    public static EmergencyContact Create(string name, string phone, string relation)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Emergency contact name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Emergency contact phone cannot be empty.", nameof(phone));
        if (string.IsNullOrWhiteSpace(relation))
            throw new ArgumentException("Emergency contact relation cannot be empty.", nameof(relation));

        return new EmergencyContact(name.Trim(), PhoneNumber.Create(phone), relation.Trim());
    }

    public static EmergencyContact? TryCreate(string? name, string? phone, string? relation)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(relation))
            return null;

        var phoneObj = PhoneNumber.TryCreate(phone);
        if (phoneObj is null)
            return null;

        return new EmergencyContact(name.Trim(), phoneObj, relation.Trim());
    }
}
