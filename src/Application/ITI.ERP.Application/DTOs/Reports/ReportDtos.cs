namespace ITI.ERP.Application.DTOs.Reports;

public class StudentAttendanceReportDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public string? TradeCode { get; set; }
    public string? TradeName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int CLDays { get; set; }
    public int ELDays { get; set; }
    public int MLDays { get; set; }
    public int HODays { get; set; }
    public decimal AttendancePercentage { get; set; }
    public int AttendanceThresholdPercentage { get; set; }
    public List<DailyAttendanceDto> DailyRecords { get; set; } = new();
}

public class DailyAttendanceDto
{
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class TradeAttendanceReportDto
{
    public Guid TradeId { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalWorkingDays { get; set; }
    public int AttendanceThresholdPercentage { get; set; }
    public List<StudentAttendanceSummaryDto> Students { get; set; } = new();
    public TradeAttendanceSummaryDto Summary { get; set; } = new();
}

public class StudentAttendanceSummaryDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int CLDays { get; set; }
    public int ELDays { get; set; }
    public int MLDays { get; set; }
    public int HODays { get; set; }
    public decimal AttendancePercentage { get; set; }
}

public class TradeAttendanceSummaryDto
{
    public int TotalStudents { get; set; }
    public decimal AverageAttendance { get; set; }
    public int HighestAttendanceDays { get; set; }
    public int LowestAttendanceDays { get; set; }
    public int StudentsAboveThreshold { get; set; }
    public int StudentsBelowThreshold { get; set; }
}

public class MonthlyPracticalReportDto
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
    public decimal HighestMarks { get; set; }
    public decimal LowestMarks { get; set; }
    public decimal PassPercentage { get; set; }
    public List<PracticalMarkReportDto> Marks { get; set; } = new();
}

public class PracticalMarkReportDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public decimal MarksObtained { get; set; }
    public bool IsPassed { get; set; }
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
    public List<PracticalMarkReportDto> Marks { get; set; } = new();
}

public class StudentPerformanceReportDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public string? TradeCode { get; set; }
    public string? TradeName { get; set; }
    public StudentAttendanceSummaryDto Attendance { get; set; } = new();
    public List<MonthlyPracticalResultDto> MonthlyPracticals { get; set; } = new();
    public YearlyPracticalResultDto? YearlyPractical { get; set; }
    public decimal OverallAttendancePercentage { get; set; }
    public decimal MonthlyPracticalAverage { get; set; }
    public decimal OverallPracticalAverage { get; set; }
}

public class MonthlyPracticalResultDto
{
    public string? PracticalName { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalObtained { get; set; }
    public int TotalMarks { get; set; }
    public int PassMarks { get; set; }
    public bool IsPassed { get; set; }
}

public class YearlyPracticalResultDto
{
    public string? PracticalName { get; set; }
    public decimal MarksObtained { get; set; }
    public int TotalMarks { get; set; }
    public int PassMarks { get; set; }
    public bool IsPassed { get; set; }
}

public class InstituteSummaryReportDto
{
    public string? InstituteName { get; set; }
    public string? GRNumber { get; set; }
    public string? SessionYear { get; set; }
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalTrades { get; set; }
    public int TotalUsers { get; set; }
    public decimal OverallAttendancePercentage { get; set; }
    public int AttendanceThresholdPercentage { get; set; }
    public decimal OverallPracticalPassPercentage { get; set; }
    public List<TradeSummaryDto> Trades { get; set; } = new();
}

public class TradeSummaryDto
{
    public Guid TradeId { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalSeats { get; set; }
    public decimal AttendancePercentage { get; set; }
    public decimal PracticalPassPercentage { get; set; }
}

public class ProgressiveAttendanceReportDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public string? TradeCode { get; set; }
    public string? TradeName { get; set; }
    public string? SessionYear { get; set; }
    public DateTime SessionStartDate { get; set; }
    public int AttendanceThresholdPercentage { get; set; }
    public List<MonthlyAttendanceBreakdownDto> Months { get; set; } = new();
    public CumulativeAttendanceDto Cumulative { get; set; } = new();
}

public class MonthlyAttendanceBreakdownDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int WorkingDays { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public int CL { get; set; }
    public int EL { get; set; }
    public int ML { get; set; }
    public int HO { get; set; }
    public decimal AttendancePercentage { get; set; }
}

public class CumulativeAttendanceDto
{
    public int TotalWorkingDays { get; set; }
    public int TotalPresent { get; set; }
    public int TotalAbsent { get; set; }
    public int TotalLate { get; set; }
    public int TotalCL { get; set; }
    public int TotalEL { get; set; }
    public int TotalML { get; set; }
    public int TotalHO { get; set; }
    public decimal CumulativePercentage { get; set; }
}

public class ProgressCardDto
{
    public string? InstituteName { get; set; }
    public string? InstituteAddress { get; set; }
    public string? InstituteLogoPath { get; set; }
    public string? StudentName { get; set; }
    public string? MotherName { get; set; }
    public string? FatherName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public string? AdmissionNumber { get; set; }
    public string? RollNumber { get; set; }
    public string? Religion { get; set; }
    public string? Category { get; set; }
    public string? EducationQualification { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? AadharNumber { get; set; }
    public string? Gender { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public int DurationInMonths { get; set; }
    public int YearLevel { get; set; }
    public string? YearLevelLabel { get; set; }
    public List<ProgressCardMonthlyPracticalDto> MonthlyPracticals { get; set; } = new();
    public List<ProgressCardMonthlyMarkDto> MonthlyMarks { get; set; } = new();
    public List<ProgressCardQuarterlyAssessmentDto> QuarterlyAssessments { get; set; } = new();
}

public class ProgressCardMonthlyPracticalDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string? MonthName { get; set; }
    public int? WeekNumber { get; set; }
    public string? PracticalName { get; set; }
    public string? ProfessionalSkillName { get; set; }
    public int TotalObtained { get; set; }
    public int TotalMarks { get; set; }
}

public class ProgressCardMonthlyMarkDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string? MonthName { get; set; }
    public int PracticalMarks { get; set; }
    public int PracticalMarksScaled { get; set; }
    public int PartATT { get; set; }
    public int PartBES { get; set; }
    public int PartsRecalSci { get; set; }
    public int EngDrg { get; set; }
    public int Total => PracticalMarksScaled + PartATT + PartBES + PartsRecalSci + EngDrg;
}

public class ProgressCardQuarterlyAssessmentDto
{
    public int Quarter { get; set; }
    public int PossibleDays { get; set; }
    public int WorkingDays { get; set; }
    public decimal AttendancePercentage { get; set; }
    public int SessionalPRT { get; set; }
    public int SessionalTT { get; set; }
    public int SessionalWCalSci { get; set; }
    public int SessionalEngDrg { get; set; }
    public int SessionalTotal => SessionalPRT + SessionalTT + SessionalWCalSci + SessionalEngDrg;
}
