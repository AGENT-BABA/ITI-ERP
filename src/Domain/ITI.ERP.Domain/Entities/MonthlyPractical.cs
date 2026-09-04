using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class MonthlyPractical : SoftDeletableEntity
{
    public Guid AcademicSessionId { get; set; }
    public Guid TradeId { get; set; }
    public Guid BatchId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AssessorName { get; set; }
    public string? LearningOutcome { get; set; }
    public string? ProfessionalSkillName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public Guid? LockedBy { get; set; }

    public Institute Institute { get; set; } = null!;
    public AcademicSession AcademicSession { get; set; } = null!;
    public Trade Trade { get; set; } = null!;
    public Batch Batch { get; set; } = null!;
    public User? LockedByUser { get; set; }
}
