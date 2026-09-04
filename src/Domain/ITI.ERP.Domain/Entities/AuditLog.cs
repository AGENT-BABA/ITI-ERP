using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Domain.Entities;

public class AuditLog : AuditableEntity
{
    public Guid? InstituteId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? AdditionalData { get; set; }
    public DateTime Timestamp { get; set; }

    public Institute? Institute { get; set; }
    public User? User { get; set; }
}
