using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.DTOs.Attendance;

public class AttendanceRecordDto
{
    public Guid Id { get; set; }
    public Guid? InstituteId { get; set; }
    public Guid AcademicSessionId { get; set; }
    public Guid TradeId { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? RollNumber { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
    public string? MarkedByUserName { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
}
