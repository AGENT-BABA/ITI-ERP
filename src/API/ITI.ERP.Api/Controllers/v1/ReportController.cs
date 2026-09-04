using Asp.Versioning;
using ITI.ERP.Application.DTOs.Reports;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/reports")]
[ApiVersion("1.0")]
[Authorize(Policy = Permissions.Reports.View)]
public class ReportController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("student/{studentId}/attendance")]
    [ProducesResponseType(typeof(StudentAttendanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentAttendanceReport(Guid studentId, [FromQuery] int month, [FromQuery] int year, CancellationToken ct)
    {
        if (month < 1 || month > 12)
            return BadRequest(new { error = "Month must be between 1 and 12." });
        if (year < 2000 || year > 2100)
            return BadRequest(new { error = "Year must be between 2000 and 2100." });

        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);
        var result = await _reportService.GetStudentAttendanceReportAsync(studentId, fromDate, toDate, ct);
        return HandleResult(result);
    }

    [HttpGet("trade/{tradeId}/attendance")]
    [ProducesResponseType(typeof(TradeAttendanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTradeAttendanceReport(Guid tradeId, [FromQuery] int month, [FromQuery] int year, CancellationToken ct)
    {
        if (month < 1 || month > 12)
            return BadRequest(new { error = "Month must be between 1 and 12." });
        if (year < 2000 || year > 2100)
            return BadRequest(new { error = "Year must be between 2000 and 2100." });

        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);
        var result = await _reportService.GetTradeAttendanceReportAsync(tradeId, fromDate, toDate, ct);
        return HandleResult(result);
    }

    [HttpGet("monthly-practical/{monthlyPracticalId}")]
    [ProducesResponseType(typeof(MonthlyPracticalReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMonthlyPracticalReport(Guid monthlyPracticalId, CancellationToken ct)
    {
        var result = await _reportService.GetMonthlyPracticalReportAsync(monthlyPracticalId, ct);
        return HandleResult(result);
    }

    [HttpGet("yearly-practical/{yearlyPracticalId}")]
    [ProducesResponseType(typeof(YearlyPracticalReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetYearlyPracticalReport(Guid yearlyPracticalId, CancellationToken ct)
    {
        var result = await _reportService.GetYearlyPracticalReportAsync(yearlyPracticalId, ct);
        return HandleResult(result);
    }

    [HttpGet("student/{studentId}/performance")]
    [ProducesResponseType(typeof(StudentPerformanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentPerformanceReport(Guid studentId, CancellationToken ct)
    {
        var result = await _reportService.GetStudentPerformanceReportAsync(studentId, ct);
        return HandleResult(result);
    }

    [HttpGet("student/{studentId}/progressive")]
    [ProducesResponseType(typeof(ProgressiveAttendanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgressiveAttendanceReport(Guid studentId, CancellationToken ct)
    {
        var result = await _reportService.GetProgressiveAttendanceReportAsync(studentId, ct);
        return HandleResult(result);
    }

    [HttpGet("institute-summary")]
    [ProducesResponseType(typeof(InstituteSummaryReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInstituteSummaryReport([FromQuery] Guid instituteId, CancellationToken ct)
    {
        var result = await _reportService.GetInstituteSummaryReportAsync(instituteId, ct);
        return HandleResult(result);
    }

    [HttpGet("student/{studentId}/progress-card")]
    [ProducesResponseType(typeof(ProgressCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgressCard(Guid studentId, CancellationToken ct)
    {
        var result = await _reportService.GetProgressCardAsync(studentId, ct);
        return HandleResult(result);
    }
}
