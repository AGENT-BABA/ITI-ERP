using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Student;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.Common.Mappers;

public static class StudentMapper
{
    public static StudentDto ToDto(this Student s) => new()
    {
        Id = s.Id,
        InstituteId = s.InstituteId,
        AcademicSessionId = s.AcademicSessionId,
        FirstName = s.FirstName,
        MiddleName = s.MiddleName,
        LastName = s.LastName,
        DateOfBirth = s.DateOfBirth,
        Gender = s.Gender,
        BloodGroup = s.BloodGroup,
        Phone = s.Phone,
        Email = s.Email,
        Address = s.Address,
        City = s.City,
        District = s.District,
        State = s.State,
        PinCode = s.PinCode,
        FatherName = s.FatherName,
        MotherName = s.MotherName,
        GuardianPhone = s.GuardianPhone,
        GuardianRelation = s.GuardianRelation,
        TradeId = s.TradeId,
        TradeName = s.Trade?.Name,
        TradeCode = s.Trade?.Code,
        BatchId = s.BatchId,
        BatchName = s.Batch?.Name,
        RollNumber = s.RollNumber,
        AdmissionNumber = s.AdmissionNumber,
        AdmissionDate = s.AdmissionDate,
        AnnualIncome = s.AnnualIncome,
        CasteCategory = s.CasteCategory,
        IsPhysicallyHandicapped = s.IsPhysicallyHandicapped,
        PreviousSchool = s.PreviousSchool,
        PreviousQualification = s.PreviousQualification,
        PreviousPercentage = s.PreviousPercentage,
        Status = s.Status,
        StatusReason = s.StatusReason,
        AadharNumber = s.AadharNumber,
        PhotoPath = s.PhotoPath,
        EmergencyContactName = s.EmergencyContactName,
        EmergencyContactPhone = s.EmergencyContactPhone,
        EmergencyContactRelation = s.EmergencyContactRelation,
        DraftStatus = (int)s.DraftStatus,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
