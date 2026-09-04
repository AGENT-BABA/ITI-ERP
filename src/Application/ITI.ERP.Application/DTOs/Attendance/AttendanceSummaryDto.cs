namespace ITI.ERP.Application.DTOs.Attendance;

public class AttendanceSummaryDto
{
    public Guid TradeId { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public DateTime Date { get; set; }
    public int TotalStudents { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int CLCount { get; set; }
    public int ELCount { get; set; }
    public int MLCount { get; set; }
    public int HOCount { get; set; }
    public bool IsLocked { get; set; }
    public bool IsMarked { get; set; }
}

public class AttendanceStudentSummaryDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public string? TradeCode { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int CLDays { get; set; }
    public int ELDays { get; set; }
    public int MLDays { get; set; }
    public int HODays { get; set; }
    public decimal AttendancePercentage { get; set; }
}

public class AttendanceTradeReportDto
{
    public Guid TradeId { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalWorkingDays { get; set; }
    public List<AttendanceStudentSummaryDto> Students { get; set; } = new();
}
