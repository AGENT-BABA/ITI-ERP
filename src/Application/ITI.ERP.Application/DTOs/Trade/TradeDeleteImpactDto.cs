namespace ITI.ERP.Application.DTOs.Trade;

public class TradeDeleteImpactDto
{
    public string TradeName { get; set; } = string.Empty;
    public string TradeCode { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int ProtectedStudents { get; set; }
    public int EligibleStudents { get; set; }
    public bool CanDelete { get; set; }
    public string? BlockReason { get; set; }
}
