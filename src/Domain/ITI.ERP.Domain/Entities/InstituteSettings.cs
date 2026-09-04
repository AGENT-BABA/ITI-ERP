using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class InstituteSettings : AuditableEntity
{
    public Guid InstituteId { get; set; }

    public string? AcademicSessionFormat { get; set; } = "YYYY-YY";
    public int AttendanceThresholdPercentage { get; set; } = 75;
    public int PassMarksPercentage { get; set; } = 40;
    public bool EnableNotifications { get; set; } = true;
    public string? NotificationEmail { get; set; }
    public string? LogoPath { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? PrincipalName { get; set; }
    public string? AffiliationNumber { get; set; }
    public string? RecognitionNumber { get; set; }

    public Institute Institute { get; set; } = null!;
}
