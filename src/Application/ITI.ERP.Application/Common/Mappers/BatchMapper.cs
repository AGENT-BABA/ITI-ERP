using ITI.ERP.Application.DTOs.Batch;
using ITI.ERP.Domain.Entities;

namespace ITI.ERP.Application.Common.Mappers;

public static class BatchMapper
{
    public static BatchDto ToDto(this Batch b) => new()
    {
        Id = b.Id,
        InstituteId = b.InstituteId,
        TradeId = b.TradeId,
        StartAcademicSessionId = b.StartAcademicSessionId,
        StartDate = b.StartDate,
        Name = b.Name,
        Code = b.Code,
        Capacity = b.Capacity,
        IsActive = b.IsActive,
        TradeName = b.Trade?.Name,
        TradeCode = b.Trade?.Code,
        TradeDurationInMonths = b.Trade?.DurationInMonths,
        StartSessionYear = b.StartAcademicSession?.SessionYear,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt,
        DeletedAt = b.DeletedAt,
        DeletedBy = b.DeletedBy
    };
}
