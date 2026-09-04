using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class YearlyPracticalMark : TenantEntity
{
    public Guid YearlyPracticalId { get; set; }
    public Guid StudentId { get; set; }
    public decimal MarksObtained { get; set; }
    public string? Remarks { get; set; }
    public Guid MarkedBy { get; set; }

    public string? MonthlyManualEntries { get; set; }
    public decimal? AnnualTotal { get; set; }
    public string? AnnualRemark { get; set; }

    public YearlyPractical YearlyPractical { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public User MarkedByUser { get; set; } = null!;
}
