using Asp.Versioning;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Batch;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/batches")]
[ApiVersion("1.0")]
[Authorize]
public class BatchController : BaseApiController
{
    private readonly IBatchService _batchService;
    private readonly ICurrentUserService _currentUserService;

    public BatchController(IBatchService batchService, ICurrentUserService currentUserService)
    {
        _batchService = batchService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Batch.View)]
    [ProducesResponseType(typeof(PaginatedList<BatchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatches(
        [FromQuery] Guid? instituteId = null,
        [FromQuery] Guid? tradeId = null,
        [FromQuery] Guid? academicSessionId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _batchService.GetBatchesAsync(instituteId, tradeId, academicSessionId, new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.Batch.View)]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchById(Guid id, [FromQuery] Guid? academicSessionId = null, CancellationToken cancellationToken = default)
    {
        var result = await _batchService.GetBatchByIdAsync(id, academicSessionId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("by-trade/{tradeId}")]
    [Authorize(Policy = Permissions.Batch.View)]
    [ProducesResponseType(typeof(List<BatchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatchesByTrade(Guid tradeId, [FromQuery] Guid? academicSessionId = null, CancellationToken cancellationToken = default)
    {
        var result = await _batchService.GetBatchesByTradeAsync(tradeId, academicSessionId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Batch.Create)]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBatch([FromBody] CreateBatchRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.HasRole(RoleConstants.Admin))
            return Forbid();

        var result = await _batchService.CreateBatchAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.Batch.Edit)]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBatch(Guid id, [FromBody] UpdateBatchRequest request, [FromQuery] Guid? academicSessionId = null, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.HasRole(RoleConstants.Admin))
            return Forbid();

        var result = await _batchService.UpdateBatchAsync(id, request, academicSessionId, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Batch.Archive)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBatch(Guid id, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasRole(RoleConstants.Admin))
            return Forbid();

        var result = await _batchService.PermanentDeleteBatchAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id}/archive-impact")]
    [Authorize(Policy = Permissions.Batch.Archive)]
    [ProducesResponseType(typeof(BatchArchiveImpactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchArchiveImpact(Guid id, CancellationToken cancellationToken)
    {
        var result = await _batchService.GetBatchArchiveImpactAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id}/archive")]
    [Authorize(Policy = Permissions.Batch.Archive)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveBatch(Guid id, CancellationToken cancellationToken)
    {
        var result = await _batchService.ArchiveBatchAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = Permissions.Batch.Archive)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreBatch(Guid id, CancellationToken cancellationToken)
    {
        var result = await _batchService.RestoreBatchAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id}/delete-impact")]
    [Authorize(Policy = Permissions.Batch.Archive)]
    [ProducesResponseType(typeof(BatchDeleteImpactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchDeleteImpact(Guid id, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasRole(RoleConstants.Admin))
            return Forbid();

        var result = await _batchService.GetBatchDeleteImpactAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
