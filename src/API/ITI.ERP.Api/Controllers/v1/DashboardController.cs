using Asp.Versioning;
using ITI.ERP.Application.DTOs.Dashboard;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/dashboard")]
[ApiVersion("1.0")]
[Authorize(Policy = Permissions.Dashboard.View)]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetDashboardSummaryAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("attendance-trend")]
    [ProducesResponseType(typeof(List<AttendanceTrendDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceTrend([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetAttendanceTrendAsync(days, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("trade-occupancy")]
    [ProducesResponseType(typeof(List<TradeSeatOccupancyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTradeSeatOccupancy(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetTradeSeatOccupancyAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("recent-activities")]
    [ProducesResponseType(typeof(List<RecentActivityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentActivities([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetRecentActivitiesAsync(count, cancellationToken);
        return HandleResult(result);
    }
}
