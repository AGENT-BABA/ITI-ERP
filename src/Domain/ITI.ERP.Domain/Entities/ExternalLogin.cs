using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class ExternalLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ExternalUserId { get; set; } = string.Empty;
    public string? Email { get; set; }

    public User User { get; set; } = null!;
}
