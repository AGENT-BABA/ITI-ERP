using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.DTOs.Attendance;

public class MarkAttendanceRequest
{
    public Guid TradeId { get; set; }
    public DateTime Date { get; set; }
    public List<StudentAttendanceItem> Students { get; set; } = new();
}

public class StudentAttendanceItem
{
    public Guid StudentId { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}
