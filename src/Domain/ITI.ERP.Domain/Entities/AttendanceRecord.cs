using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Domain.Entities;

public class AttendanceRecord : SoftDeletableEntity
{
    public Guid AcademicSessionId { get; set; }
    public Guid TradeId { get; set; }
    public Guid BatchId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
    public Guid MarkedBy { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public Guid? LockedBy { get; set; }

    public Institute Institute { get; set; } = null!;
    public AcademicSession AcademicSession { get; set; } = null!;
    public Trade Trade { get; set; } = null!;
    public Batch Batch { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public User MarkedByUser { get; set; } = null!;
}
