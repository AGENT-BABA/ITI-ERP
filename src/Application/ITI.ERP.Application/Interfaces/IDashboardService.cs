using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Dashboard;

namespace ITI.ERP.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetDashboardSummaryAsync(CancellationToken ct);
    Task<Result<List<AttendanceTrendDto>>> GetAttendanceTrendAsync(int days, CancellationToken ct);
    Task<Result<List<TradeSeatOccupancyDto>>> GetTradeSeatOccupancyAsync(CancellationToken ct);
    Task<Result<List<RecentActivityDto>>> GetRecentActivitiesAsync(int count, CancellationToken ct);
}
