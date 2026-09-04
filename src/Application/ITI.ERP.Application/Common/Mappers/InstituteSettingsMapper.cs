using ITI.ERP.Application.DTOs.Settings;
using ITI.ERP.Domain.Entities;

namespace ITI.ERP.Application.Common.Mappers;

public static class InstituteSettingsMapper
{
    public static InstituteSettingsDto ToDto(this InstituteSettings s) => new()
    {
        Id = s.Id,
        InstituteId = s.InstituteId,
        AcademicSessionFormat = s.AcademicSessionFormat,
        AttendanceThresholdPercentage = s.AttendanceThresholdPercentage,
        PassMarksPercentage = s.PassMarksPercentage,
        EnableNotifications = s.EnableNotifications,
        NotificationEmail = s.NotificationEmail,
        LogoPath = s.LogoPath,
        Address = s.Address,
        City = s.City,
        District = s.District,
        State = s.State,
        Phone = s.Phone,
        Email = s.Email,
        Website = s.Website,
        PrincipalName = s.PrincipalName,
        AffiliationNumber = s.AffiliationNumber,
        RecognitionNumber = s.RecognitionNumber,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
