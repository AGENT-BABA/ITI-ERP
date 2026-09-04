using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class PracticalMark : TenantEntity
{
    public Guid MonthlyPracticalId { get; set; }
    public Guid StudentId { get; set; }
    public int SafetyConsciousness { get; set; }
    public int WorkplaceHygiene { get; set; }
    public int AttendancePunctuality { get; set; }
    public int FollowInstructions { get; set; }
    public int ApplicationKnowledge { get; set; }
    public int SkillsToolsEquipment { get; set; }
    public int SpeedDoingWork { get; set; }
    public int QualityWorkmanship { get; set; }
    public int Viva { get; set; }
    public bool SignedByTrainee { get; set; }
    public string? Remarks { get; set; }
    public Guid MarkedBy { get; set; }

    public MonthlyPractical MonthlyPractical { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public User MarkedByUser { get; set; } = null!;
}
