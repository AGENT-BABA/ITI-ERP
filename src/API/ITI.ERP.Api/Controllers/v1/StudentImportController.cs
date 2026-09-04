using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.StudentImport;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/student-import")]
[ApiVersion("1.0")]
[Authorize]
public class StudentImportController : BaseApiController
{
    private readonly IStudentImportService _studentImportService;

    public StudentImportController(IStudentImportService studentImportService)
    {
        _studentImportService = studentImportService;
    }

    [HttpPost("{tradeId}/preview")]
    [Authorize(Policy = Permissions.Student.Import)]
    [ProducesResponseType(typeof(StudentImportPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreviewImport(Guid tradeId, IFormFile file, [FromQuery] Guid? batchId = null, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Please upload a file.");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx and .xls files are supported.");

        await using var stream = file.OpenReadStream();
        var result = await _studentImportService.PreviewImportAsync(tradeId, stream, file.FileName, batchId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("confirm")]
    [Authorize(Policy = Permissions.Student.Import)]
    [ProducesResponseType(typeof(StudentImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmImport([FromBody] StudentImportPreviewDto preview, CancellationToken cancellationToken)
    {
        var result = await _studentImportService.ConfirmImportAsync(preview, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("template")]
    [Authorize(Policy = Permissions.Student.Import)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadTemplate(CancellationToken cancellationToken)
    {
        var result = await _studentImportService.DownloadTemplateAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StudentImportTemplate.xlsx");
    }

    [HttpGet("history")]
    [Authorize(Policy = Permissions.Student.View)]
    [ProducesResponseType(typeof(PaginatedList<StudentImportHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImportHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _studentImportService.GetImportHistoryAsync(
            new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);
        return HandleResult(result);
    }
}
