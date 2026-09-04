namespace ITI.ERP.Application.DTOs.Batch;

public class UpdateBatchRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public int? Capacity { get; set; }
    public bool? IsActive { get; set; }
}
