namespace ITI.ERP.Application.DTOs.Trade;

public class TradeDto
{
    public Guid Id { get; set; }
    public Guid? InstituteId { get; set; }
    public Guid? AcademicSessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DurationInMonths { get; set; }
    public int TotalSeats { get; set; }
    public Guid? HeadUserId { get; set; }
    public string? HeadUserName { get; set; }
    public int DraftStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
