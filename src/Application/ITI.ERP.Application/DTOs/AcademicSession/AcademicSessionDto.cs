namespace ITI.ERP.Application.DTOs.AcademicSession;

public class AcademicSessionDto
{
    public Guid Id { get; set; }
    public Guid? InstituteId { get; set; }
    public string SessionYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
