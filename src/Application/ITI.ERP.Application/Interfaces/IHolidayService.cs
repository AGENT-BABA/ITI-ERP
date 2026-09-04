using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Holiday;

namespace ITI.ERP.Application.Interfaces;

public interface IHolidayService
{
    Task<Result<List<HolidayDto>>> GetHolidaysAsync(DateTime from, DateTime to, CancellationToken ct);
    Task<Result<HolidayDto>> CreateHolidayAsync(CreateHolidayRequest request, CancellationToken ct);
    Task<Result> DeleteHolidayAsync(Guid id, CancellationToken ct);
}
