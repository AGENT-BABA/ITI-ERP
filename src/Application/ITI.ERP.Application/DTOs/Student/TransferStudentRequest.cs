namespace ITI.ERP.Application.DTOs.Student;

public class TransferStudentRequest
{
    public Guid NewTradeId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
