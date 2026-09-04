namespace ITI.ERP.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalInstitutes { get; set; }
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalTrades { get; set; }
    public int TotalUsers { get; set; }
    public int TotalAcademicSessions { get; set; }
    public int ActiveAcademicSessions { get; set; }
    public decimal OverallAttendancePercentage { get; set; }
    public int TotalMonthlyPracticals { get; set; }
    public int TotalYearlyPracticals { get; set; }
    public decimal MonthlyPracticalPassPercentage { get; set; }
    public decimal YearlyPracticalPassPercentage { get; set; }
    public string? TradeName { get; set; }
    public List<TradeSeatOccupancyDto> TradeSeatOccupancy { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public List<AttendanceTrendDto> AttendanceTrends { get; set; } = new();
    public List<StudentStatusDistributionDto> StudentStatusDistribution { get; set; } = new();
}

public class TradeSeatOccupancyDto
{
    public Guid TradeId { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public int TotalSeats { get; set; }
    public int OccupiedSeats { get; set; }
    public decimal OccupancyPercentage { get; set; }
}

public class RecentActivityDto
{
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Description { get; set; }
}

public class AttendanceTrendDto
{
    public DateTime Date { get; set; }
    public decimal AttendancePercentage { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int TotalCount { get; set; }
}

public class StudentStatusDistributionDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
