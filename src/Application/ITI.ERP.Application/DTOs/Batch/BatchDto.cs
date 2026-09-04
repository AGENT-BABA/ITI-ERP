using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.DTOs.Batch;

public class BatchDto
{
    public Guid Id { get; set; }
    public Guid InstituteId { get; set; }
    public Guid TradeId { get; set; }
    public Guid StartAcademicSessionId { get; set; }
    public DateTime StartDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? Capacity { get; set; }
    public bool IsActive { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public int? TradeDurationInMonths { get; set; }
    public string? StartSessionYear { get; set; }
    public int StudentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public BatchComputedStatus ComputedStatus { get; set; }
    public int? ComputedYearLevel { get; set; }
    public string ComputedYearLevelLabel { get; set; } = string.Empty;
}
