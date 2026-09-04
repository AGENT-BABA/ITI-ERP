using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public Guid? InitiatedByUserId { get; set; }
    public Guid? InstituteId { get; set; }

    public User User { get; set; } = null!;
    public User? InitiatedByUser { get; set; }
    public Institute? Institute { get; set; }
}
