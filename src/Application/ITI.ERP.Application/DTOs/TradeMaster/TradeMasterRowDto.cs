namespace ITI.ERP.Application.DTOs.TradeMaster;

public class TradeMasterRowDto
{
    public int RowNumber { get; set; }
    public string TradeCode { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int DurationMonths { get; set; }
}
