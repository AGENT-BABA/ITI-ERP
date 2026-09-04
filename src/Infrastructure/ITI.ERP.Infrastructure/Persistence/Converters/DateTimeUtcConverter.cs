using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ITI.ERP.Infrastructure.Persistence.Converters;

public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcConverter()
        : base(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v,
            v => v)
    {
    }
}

public class NullableDateTimeUtcConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableDateTimeUtcConverter()
        : base(
            v => v.HasValue && v.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                : v,
            v => v)
    {
    }
}
