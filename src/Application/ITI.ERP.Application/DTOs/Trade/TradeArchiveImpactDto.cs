namespace ITI.ERP.Application.DTOs.Trade;

public class TradeArchiveImpactDto
{
    public string TradeName { get; set; } = string.Empty;
    public string TradeCode { get; set; } = string.Empty;
    public int Batches { get; set; }
    public int Students { get; set; }
    public int AttendanceRecords { get; set; }
    public int MonthlyPracticals { get; set; }
    public int YearlyPracticals { get; set; }
    public int UserRoles { get; set; }
}
