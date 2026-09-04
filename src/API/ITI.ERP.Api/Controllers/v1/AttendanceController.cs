using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Attendance;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/attendance")]
[ApiVersion("1.0")]
[Authorize]
public class AttendanceController : BaseApiController
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet("{tradeId}/date/{date}")]
    [Authorize(Policy = Permissions.Attendance.View)]
    [ProducesResponseType(typeof(List<AttendanceRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceByDate(Guid tradeId, DateTime date, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetAttendanceByDateAsync(tradeId, date, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{tradeId}/summary/{date}")]
    [Authorize(Policy = Permissions.Attendance.View)]
    [ProducesResponseType(typeof(AttendanceSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceSummary(Guid tradeId, DateTime date, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetAttendanceSummaryAsync(tradeId, date, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{tradeId}/summary-range")]
    [Authorize(Policy = Permissions.Attendance.View)]
    [ProducesResponseType(typeof(List<AttendanceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceSummaryRange(
        Guid tradeId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetAttendanceSummaryRangeAsync(tradeId, fromDate, toDate, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("student/{studentId}/summary")]
    [Authorize(Policy = Permissions.Attendance.View)]
    [ProducesResponseType(typeof(AttendanceStudentSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentAttendanceSummary(Guid studentId, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetStudentAttendanceSummaryAsync(studentId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{tradeId}/report")]
    [Authorize(Policy = Permissions.Attendance.View)]
    [ProducesResponseType(typeof(AttendanceTradeReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTradeAttendanceReport(
        Guid tradeId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetTradeAttendanceReportAsync(tradeId, fromDate, toDate, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Attendance.Mark)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceRequest request, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.MarkAttendanceAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{tradeId}/lock/{date}")]
    [Authorize(Policy = Permissions.Attendance.Mark)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LockAttendance(Guid tradeId, DateTime date, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.LockAttendanceAsync(tradeId, date, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{tradeId}/unlock/{date}")]
    [Authorize(Policy = Permissions.Attendance.Unlock)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnlockAttendance(Guid tradeId, DateTime date, [FromQuery] string reason, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.UnlockAttendanceAsync(tradeId, date, reason, cancellationToken);
        return HandleResult(result);
    }
}
