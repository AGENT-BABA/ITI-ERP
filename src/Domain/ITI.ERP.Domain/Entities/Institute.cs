using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class Institute : AuditableEntity
{
    public string GRNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<AcademicSession> AcademicSessions { get; set; } = new List<AcademicSession>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
