using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.TradeMaster;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/trade-master")]
[ApiVersion("1.0")]
[Authorize]
public class TradeMasterController : BaseApiController
{
    private readonly ITradeMasterService _tradeMasterService;

    public TradeMasterController(ITradeMasterService tradeMasterService)
    {
        _tradeMasterService = tradeMasterService;
    }

    [HttpPost("preview")]
    [Authorize(Policy = Permissions.Trade.Import)]
    [ProducesResponseType(typeof(TradeMasterPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreviewImport(IFormFile file, [FromQuery] Guid? instituteId = null, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Please upload a file.");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx files are supported.");

        await using var stream = file.OpenReadStream();
        var result = await _tradeMasterService.PreviewImportAsync(stream, file.FileName, instituteId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("confirm")]
    [Authorize(Policy = Permissions.Trade.Import)]
    [ProducesResponseType(typeof(TradeMasterImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmImport([FromBody] TradeMasterPreviewDto preview, [FromQuery] Guid? instituteId = null, CancellationToken cancellationToken = default)
    {
        var result = await _tradeMasterService.ConfirmImportAsync(preview, instituteId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("template")]
    [Authorize(Policy = Permissions.Trade.Import)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadTemplate(CancellationToken cancellationToken)
    {
        var result = await _tradeMasterService.DownloadTemplateAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TradeMasterTemplate.xlsx");
    }

    [HttpGet("history")]
    [Authorize(Policy = Permissions.Trade.View)]
    [ProducesResponseType(typeof(PaginatedList<TradeMasterHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImportHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _tradeMasterService.GetImportHistoryAsync(
            new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);
        return HandleResult(result);
    }
}
