namespace ITI.ERP.Application.DTOs.Settings;

public class InstituteSettingsDto
{
    public Guid Id { get; set; }
    public Guid? InstituteId { get; set; }

    public string? AcademicSessionFormat { get; set; }
    public int AttendanceThresholdPercentage { get; set; }
    public int PassMarksPercentage { get; set; }

    public bool EnableNotifications { get; set; }
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
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public AcademicSettingsDto AcademicSettings => new()
    {
        AcademicSessionFormat = AcademicSessionFormat,
        AttendanceThresholdPercentage = AttendanceThresholdPercentage,
        PassMarksPercentage = PassMarksPercentage
    };

    public InstituteProfileDto InstituteProfile => new()
    {
        LogoPath = LogoPath,
        Address = Address,
        City = City,
        District = District,
        State = State,
        Phone = Phone,
        Email = Email,
        Website = Website,
        PrincipalName = PrincipalName,
        AffiliationNumber = AffiliationNumber,
        RecognitionNumber = RecognitionNumber
    };
}

public class AcademicSettingsDto
{
    public string? AcademicSessionFormat { get; set; }
    public int AttendanceThresholdPercentage { get; set; }
    public int PassMarksPercentage { get; set; }
}

public class InstituteProfileDto
{
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
}

public class UpdateInstituteSettingsRequest
{
    public string? AcademicSessionFormat { get; set; }
    public int? AttendanceThresholdPercentage { get; set; }
    public int? PassMarksPercentage { get; set; }
    public bool? EnableNotifications { get; set; }
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
}
