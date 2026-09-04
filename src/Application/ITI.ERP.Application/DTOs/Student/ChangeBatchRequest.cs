namespace ITI.ERP.Application.DTOs.Student;

public class ChangeBatchRequest
{
    public Guid NewBatchId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
