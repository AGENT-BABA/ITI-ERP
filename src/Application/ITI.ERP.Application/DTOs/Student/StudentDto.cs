using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.DTOs.Student;

public class StudentDto
{
    public Guid Id { get; set; }
    public Guid? InstituteId { get; set; }
    public Guid AcademicSessionId { get; set; }
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
    public Guid TradeId { get; set; }
    public string? TradeName { get; set; }
    public string? TradeCode { get; set; }
    public Guid? BatchId { get; set; }
    public string? BatchName { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public decimal AnnualIncome { get; set; }
    public string? CasteCategory { get; set; }
    public bool IsPhysicallyHandicapped { get; set; }
    public string? PreviousSchool { get; set; }
    public string? PreviousQualification { get; set; }
    public decimal? PreviousPercentage { get; set; }
    public StudentStatus Status { get; set; }
    public string? StatusReason { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public string? WithdrawalReason { get; set; }
    public string? AadharNumber { get; set; }
    public string? PhotoPath { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public int DraftStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RetentionUntil { get; set; }
}
