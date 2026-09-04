namespace ITI.ERP.Application.DTOs.Batch;

public class BatchArchiveImpactDto
{
    public string BatchName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public int Students { get; set; }
    public int AttendanceRecords { get; set; }
    public int MonthlyPracticals { get; set; }
    public int YearlyPracticals { get; set; }
    public int UserRoles { get; set; }
}
