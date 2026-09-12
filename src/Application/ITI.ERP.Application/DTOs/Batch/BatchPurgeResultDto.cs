namespace ITI.ERP.Application.DTOs.Batch;

public class BatchPurgeResultDto
{
    public string BatchName { get; set; } = string.Empty;
    public int StudentsDeleted { get; set; }
    public int MonthlyPracticalsDeleted { get; set; }
    public int MonthlyPracticalsSkipped { get; set; }
    public int YearlyPracticalsDeleted { get; set; }
    public int YearlyPracticalsSkipped { get; set; }
    public int UserRolesDeleted { get; set; }
    public List<string> SkipReasons { get; set; } = new();
}
