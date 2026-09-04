namespace ITI.ERP.Application.DTOs.Practical;

public class MonthlyPracticalDto
{
    public Guid Id { get; set; }
    public Guid? InstituteId { get; set; }
    public Guid AcademicSessionId { get; set; }
    public Guid TradeId { get; set; }
    public Guid BatchId { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
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
    public int MarksEnteredCount { get; set; }
    public int TotalStudents { get; set; }
    public int PassMarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateMonthlyPracticalRequest
{
    public Guid TradeId { get; set; }
    public Guid? BatchId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AssessorName { get; set; }
    public string? LearningOutcome { get; set; }
    public string? ProfessionalSkillName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateMonthlyPracticalRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class PracticalMarkDto
{
    public Guid Id { get; set; }
    public Guid MonthlyPracticalId { get; set; }
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public int SafetyConsciousness { get; set; }
    public int WorkplaceHygiene { get; set; }
    public int AttendancePunctuality { get; set; }
    public int FollowInstructions { get; set; }
    public int ApplicationKnowledge { get; set; }
    public int SkillsToolsEquipment { get; set; }
    public int SpeedDoingWork { get; set; }
    public int QualityWorkmanship { get; set; }
    public int Viva { get; set; }
    public int TotalObtained { get; set; }
    public int TotalMarks { get; set; }
    public bool IsPassed { get; set; }
    public bool SignedByTrainee { get; set; }
    public string? Remarks { get; set; }
    public string? MarkedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SubmitPracticalMarksRequest
{
    public Guid MonthlyPracticalId { get; set; }
    public List<StudentPracticalMarkItem> Students { get; set; } = new();
}

public class StudentPracticalMarkItem
{
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
}

public class PracticalReportDto
{
    public Guid MonthlyPracticalId { get; set; }
    public string? PracticalName { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalMarks { get; set; }
    public int PassMarks { get; set; }
    public int TotalStudents { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public decimal AverageMarks { get; set; }
    public int HighestMarks { get; set; }
    public int LowestMarks { get; set; }
    public decimal PassPercentage { get; set; }
    public List<PracticalMarkDto> Marks { get; set; } = new();
}
