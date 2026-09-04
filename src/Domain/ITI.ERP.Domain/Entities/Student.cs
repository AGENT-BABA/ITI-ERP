using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Domain.Entities;

public class Student : DraftEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }

    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? GuardianPhone { get; set; }
    public string? GuardianRelation { get; set; }

    public Guid AcademicSessionId { get; set; }
    public Guid TradeId { get; set; }
    public Guid? BatchId { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public decimal AnnualIncome { get; set; }
    public string? CasteCategory { get; set; }
    public string? AllottedRound { get; set; }
    public bool IsPhysicallyHandicapped { get; set; }
    public string? PreviousSchool { get; set; }
    public string? PreviousQualification { get; set; }
    public decimal? PreviousPercentage { get; set; }

    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public string? StatusReason { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public Guid? StatusChangedBy { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public string? WithdrawalReason { get; set; }

    public string? AadharNumber { get; set; }
    public string? PhotoPath { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }

    public Institute Institute { get; set; } = null!;
    public AcademicSession AcademicSession { get; set; } = null!;
    public Trade Trade { get; set; } = null!;
    public Batch? Batch { get; set; }

    public string GetFullName()
    {
        return string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";
    }

    public int GetAge(DateTime? asOfDate = null)
    {
        var referenceDate = asOfDate ?? DateTime.UtcNow;
        var age = referenceDate.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > referenceDate.AddYears(-age))
            age--;
        return age;
    }

    public bool CanTransitionToStatus(StudentStatus targetStatus)
    {
        return Status switch
        {
            StudentStatus.Active => targetStatus is StudentStatus.Inactive or StudentStatus.Transferred
                or StudentStatus.Withdrawn or StudentStatus.Completed or StudentStatus.CancelledAdmission,
            StudentStatus.Inactive => targetStatus is StudentStatus.Active,
            _ => false
        };
    }

    public void ChangeStatus(StudentStatus newStatus, string? reason, Guid changedBy, DateTime utcNow)
    {
        if (!CanTransitionToStatus(newStatus))
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}.");

        Status = newStatus;
        StatusReason = reason;
        StatusChangedAt = utcNow;
        StatusChangedBy = changedBy;
    }

    public void UpdatePhoto(string? storedFileName, string? previousPath)
    {
        PhotoPath = storedFileName;
    }
}
