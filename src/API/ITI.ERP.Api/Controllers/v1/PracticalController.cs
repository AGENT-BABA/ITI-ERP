using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Practical;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/practicals")]
[ApiVersion("1.0")]
[Authorize]
public class PracticalController : BaseApiController
{
    private readonly IPracticalService _practicalService;

    public PracticalController(IPracticalService practicalService)
    {
        _practicalService = practicalService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetMonthlyPracticals([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var result = await _practicalService.GetMonthlyPracticalsAsync(new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm }, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetMonthlyPracticalById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _practicalService.GetMonthlyPracticalByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Practical.Create)]
    public async Task<IActionResult> CreateMonthlyPractical([FromBody] CreateMonthlyPracticalRequest request, CancellationToken cancellationToken)
    {
        var result = await _practicalService.CreateMonthlyPracticalAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.Practical.Edit)]
    public async Task<IActionResult> UpdateMonthlyPractical(Guid id, [FromBody] UpdateMonthlyPracticalRequest request, CancellationToken cancellationToken)
    {
        var result = await _practicalService.UpdateMonthlyPracticalAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Practical.Delete)]
    public async Task<IActionResult> DeleteMonthlyPractical(Guid id, CancellationToken cancellationToken)
    {
        var result = await _practicalService.DeleteMonthlyPracticalAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{monthlyPracticalId}/marks")]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetPracticalMarks(Guid monthlyPracticalId, CancellationToken cancellationToken)
    {
        var result = await _practicalService.GetPracticalMarksAsync(monthlyPracticalId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("marks")]
    [Authorize(Policy = Permissions.Practical.Create)]
    public async Task<IActionResult> SubmitPracticalMarks([FromBody] SubmitPracticalMarksRequest request, CancellationToken cancellationToken)
    {
        var result = await _practicalService.SubmitPracticalMarksAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/lock")]
    [Authorize(Policy = Permissions.Practical.Lock)]
    public async Task<IActionResult> LockPractical(Guid id, CancellationToken cancellationToken)
    {
        var result = await _practicalService.LockPracticalAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}/unlock")]
    [Authorize(Policy = Permissions.Practical.Unlock)]
    public async Task<IActionResult> UnlockPractical(Guid id, [FromQuery] string reason, CancellationToken cancellationToken)
    {
        var result = await _practicalService.UnlockPracticalAsync(id, reason, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}/report")]
    [Authorize(Policy = Permissions.Practical.View)]
    public async Task<IActionResult> GetPracticalReport(Guid id, CancellationToken cancellationToken)
    {
        var result = await _practicalService.GetPracticalReportAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
