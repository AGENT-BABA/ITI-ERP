using ITI.ERP.Application.DTOs.AcademicSession;
using ITI.ERP.Domain.Entities;

namespace ITI.ERP.Application.Common.Mappers;

public static class AcademicSessionMapper
{
    public static AcademicSessionDto ToDto(this AcademicSession a) => new()
    {
        Id = a.Id,
        InstituteId = a.InstituteId,
        SessionYear = a.SessionYear,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        IsActive = a.IsActive,
        IsLocked = a.IsLocked,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
