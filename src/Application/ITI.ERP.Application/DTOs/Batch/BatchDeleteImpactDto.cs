namespace ITI.ERP.Application.DTOs.Batch;

public class BatchDeleteImpactDto
{
    public string BatchName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int ProtectedStudents { get; set; }
    public int EligibleStudents { get; set; }
    public bool CanDelete { get; set; }
    public string? BlockReason { get; set; }
}
