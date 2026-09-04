using ITI.ERP.Application.DTOs.Trade;
using ITI.ERP.Domain.Entities;

namespace ITI.ERP.Application.Common.Mappers;

public static class TradeMapper
{
    public static TradeDto ToDto(this Trade t) => new()
    {
        Id = t.Id,
        InstituteId = t.InstituteId,
        AcademicSessionId = t.AcademicSessionId,
        Name = t.Name,
        Code = t.Code,
        DurationInMonths = t.DurationInMonths,
        TotalSeats = t.TotalSeats,
        HeadUserId = t.HeadUserId,
        HeadUserName = t.HeadUser != null ? $"{t.HeadUser.FirstName} {t.HeadUser.LastName}" : null,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
