namespace ITI.ERP.Application.DTOs.Trade;

public class CreateTradeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DurationInMonths { get; set; }
    public int TotalSeats { get; set; }
    public Guid? HeadUserId { get; set; }
}
