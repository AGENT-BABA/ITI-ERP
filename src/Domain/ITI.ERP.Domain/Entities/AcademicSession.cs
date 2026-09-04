using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class AcademicSession : SoftDeletableEntity
{
    public string SessionYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }

    public Institute Institute { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
}
