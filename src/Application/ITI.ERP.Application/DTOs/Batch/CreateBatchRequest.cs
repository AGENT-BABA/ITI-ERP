namespace ITI.ERP.Application.DTOs.Batch;

public class CreateBatchRequest
{
    public Guid TradeId { get; set; }
    public Guid StartAcademicSessionId { get; set; }
    public DateTime StartDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? Capacity { get; set; }
}
