using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.YearlyPractical;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/yearly-practicals")]
[ApiVersion("1.0")]
[Authorize]
public class YearlyPracticalController : BaseApiController
{
    private readonly IYearlyPracticalService _yearlyPracticalService;

    public YearlyPracticalController(IYearlyPracticalService yearlyPracticalService)
    {
        _yearlyPracticalService = yearlyPracticalService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetYearlyPracticals([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var result = await _yearlyPracticalService.GetYearlyPracticalsAsync(new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm }, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetYearlyPracticalById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.GetYearlyPracticalByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Practical.Create)]
    public async Task<IActionResult> CreateYearlyPractical([FromBody] CreateYearlyPracticalRequest request, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.CreateYearlyPracticalAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.Practical.Edit)]
    public async Task<IActionResult> UpdateYearlyPractical(Guid id, [FromBody] UpdateYearlyPracticalRequest request, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.UpdateYearlyPracticalAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Practical.Delete)]
    public async Task<IActionResult> DeleteYearlyPractical(Guid id, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.DeleteYearlyPracticalAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{yearlyPracticalId}/marks")]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetYearlyPracticalMarks(Guid yearlyPracticalId, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.GetYearlyPracticalMarksAsync(yearlyPracticalId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("marks")]
    [Authorize(Policy = Permissions.Practical.Create)]
    public async Task<IActionResult> SubmitYearlyPracticalMarks([FromBody] SubmitYearlyPracticalMarksRequest request, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.SubmitYearlyPracticalMarksAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/lock")]
    [Authorize(Policy = Permissions.Practical.Lock)]
    public async Task<IActionResult> LockYearlyPractical(Guid id, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.LockYearlyPracticalAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/unlock")]
    [Authorize(Policy = Permissions.Practical.Unlock)]
    public async Task<IActionResult> UnlockYearlyPractical(Guid id, [FromQuery] string reason, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.UnlockYearlyPracticalAsync(id, reason, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}/report")]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetYearlyPracticalReport(Guid id, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.GetYearlyPracticalReportAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("student/{studentId}/performance")]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetStudentYearlyPerformance(Guid studentId, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.GetStudentYearlyPerformanceAsync(studentId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{yearlyPracticalId}/students")]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetYearlyPracticalStudents(Guid yearlyPracticalId, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.GetYearlyPracticalStudentsAsync(yearlyPracticalId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{yearlyPracticalId}/student/{studentId}")]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetYearlyPracticalStudentDetail(Guid yearlyPracticalId, Guid studentId, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.GetYearlyPracticalStudentDetailAsync(yearlyPracticalId, studentId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{yearlyPracticalId}/student/{studentId}/entries")]
    [Authorize(Policy = Permissions.Practical.Create)]
    public async Task<IActionResult> SaveStudentYearlyEntries(Guid yearlyPracticalId, Guid studentId, [FromBody] SaveYearlyEntriesRequest request, CancellationToken cancellationToken)
    {
        var result = await _yearlyPracticalService.SaveStudentYearlyEntriesAsync(yearlyPracticalId, studentId, request, cancellationToken);
        return HandleResult(result);
    }
}
