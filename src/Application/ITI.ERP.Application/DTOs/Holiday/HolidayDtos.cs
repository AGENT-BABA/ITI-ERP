namespace ITI.ERP.Application.DTOs.Holiday;

public class HolidayDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string? Name { get; set; }
    public string? SessionYear { get; set; }
    public Guid AcademicSessionId { get; set; }
}

public class CreateHolidayRequest
{
    public DateTime Date { get; set; }
    public string? Name { get; set; }
}
