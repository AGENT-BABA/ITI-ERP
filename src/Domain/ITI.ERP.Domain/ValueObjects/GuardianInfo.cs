namespace ITI.ERP.Domain.ValueObjects;

public record GuardianInfo
{
    public string Name { get; }
    public PhoneNumber? Phone { get; }
    public string Relation { get; }

    private GuardianInfo(string name, PhoneNumber? phone, string relation)
    {
        Name = name;
        Phone = phone;
        Relation = relation;
    }

    public static GuardianInfo Create(string name, string? phone, string relation)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Guardian name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(relation))
            throw new ArgumentException("Guardian relation cannot be empty.", nameof(relation));

        return new GuardianInfo(
            name.Trim(),
            PhoneNumber.TryCreate(phone),
            relation.Trim());
    }

    public static GuardianInfo? TryCreate(string? name, string? phone, string? relation)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(relation))
            return null;

        return new GuardianInfo(
            name.Trim(),
            PhoneNumber.TryCreate(phone),
            relation.Trim());
    }
}
