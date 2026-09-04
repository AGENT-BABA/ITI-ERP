namespace ITI.ERP.Application.DTOs.Trade;

public class UpdateTradeRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public int? DurationInMonths { get; set; }
    public int? TotalSeats { get; set; }
    public Guid? HeadUserId { get; set; }
}
