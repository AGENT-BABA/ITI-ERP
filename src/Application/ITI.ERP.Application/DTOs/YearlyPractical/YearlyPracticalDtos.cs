namespace ITI.ERP.Application.DTOs.YearlyPractical;

public class YearlyPracticalDto
{
    public Guid Id { get; set; }
    public Guid? InstituteId { get; set; }
    public Guid AcademicSessionId { get; set; }
    public Guid TradeId { get; set; }
    public Guid BatchId { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public int Year { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalMarks { get; set; }
    public int PassMarks { get; set; }
    public bool IsLocked { get; set; }
    public int MarksEnteredCount { get; set; }
    public int TotalStudents { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateYearlyPracticalRequest
{
    public Guid TradeId { get; set; }
    public Guid? BatchId { get; set; }
    public int Year { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateYearlyPracticalRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class YearlyPracticalMarkDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public decimal AnnualAverage { get; set; }
    public int TotalMarks { get; set; }
    public int PassMarks { get; set; }
    public bool IsPassed { get; set; }
    public List<MonthlyAverageResultDto> MonthlyAverages { get; set; } = new();
}

public class SubmitYearlyPracticalMarksRequest
{
    public Guid YearlyPracticalId { get; set; }
    public List<StudentYearlyPracticalMarkItem> Students { get; set; } = new();
}

public class StudentYearlyPracticalMarkItem
{
    public Guid StudentId { get; set; }
}

public class YearlyPracticalReportDto
{
    public Guid YearlyPracticalId { get; set; }
    public string? PracticalName { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public int Year { get; set; }
    public int TotalMarks { get; set; }
    public int PassMarks { get; set; }
    public int TotalStudents { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public decimal AverageMarks { get; set; }
    public decimal HighestMarks { get; set; }
    public decimal LowestMarks { get; set; }
    public decimal PassPercentage { get; set; }
    public List<YearlyPracticalMarkDto> Marks { get; set; } = new();
}

public class StudentYearlyPerformanceDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public string? TradeCode { get; set; }
    public List<MonthlyPracticalResultDto> MonthlyPracticals { get; set; } = new();
    public List<MonthlyAverageResultDto> MonthlyAverages { get; set; } = new();
    public YearlyPracticalResultDto? YearlyPractical { get; set; }
    public decimal MonthlyPracticalAverage { get; set; }
    public decimal OverallPracticalAverage { get; set; }
}

public class MonthlyPracticalResultDto
{
    public Guid MonthlyPracticalId { get; set; }
    public string? PracticalName { get; set; }
    public string? ProfessionalSkillName { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalObtained { get; set; }
    public int TotalMarks { get; set; }
    public int PassMarks { get; set; }
    public bool IsPassed { get; set; }
}

public class MonthlyAverageResultDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int PracticalCount { get; set; }
    public decimal AverageObtained { get; set; }
    public int TotalMarks { get; set; }
    public int PassMarks { get; set; }
    public bool IsPassed { get; set; }
}

public class YearlyPracticalResultDto
{
    public Guid YearlyPracticalId { get; set; }
    public string? PracticalName { get; set; }
    public decimal MarksObtained { get; set; }
    public int TotalMarks { get; set; }
    public int PassMarks { get; set; }
    public bool IsPassed { get; set; }
}

public class YearlyPracticalStudentListDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public List<MonthlyPrSummaryDto> MonthlyPRs { get; set; } = new();
    public decimal AnnualTotal { get; set; }
    public string? AnnualRemark { get; set; }
}

public class MonthlyPrSummaryDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal? PR { get; set; }
    public int? PracticalCount { get; set; }
}

public class YearlyPracticalStudentDetailDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public List<YearlyPracticalMonthRowDto> Months { get; set; } = new();
    public decimal AnnualTotal { get; set; }
    public string? AnnualRemark { get; set; }
    public bool IsLocked { get; set; }
}

public class YearlyPracticalMonthRowDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal? PR { get; set; }
    public int? PartA { get; set; }
    public int? PartB { get; set; }
    public int? VocationalScience { get; set; }
    public int? EngDrawing { get; set; }
    public decimal? Total { get; set; }
    public string? GISig { get; set; }
    public string? PVPSig { get; set; }
    public string? Remark { get; set; }
}

public class SaveYearlyEntriesRequest
{
    public int Month { get; set; }
    public int? PartA { get; set; }
    public int? PartB { get; set; }
    public int? VocationalScience { get; set; }
    public int? EngDrawing { get; set; }
    public string? GISig { get; set; }
    public string? PVPSig { get; set; }
    public string? Remark { get; set; }
}
