namespace ITI.ERP.Infrastructure.Services;

public interface IDateTime
{
    DateTime Now { get; }
}

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}