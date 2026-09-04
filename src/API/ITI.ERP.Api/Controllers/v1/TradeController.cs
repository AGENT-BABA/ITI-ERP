using Asp.Versioning;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Application.DTOs.Trade;
using ITI.ERP.Domain.Constants;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/trades")]
    [ApiVersion("1.0")]
    [Authorize]
    public class TradeController : BaseApiController
    {
        private readonly ITradeService _tradeService;
        private readonly ICurrentUserService _currentUserService;

        public TradeController(ITradeService tradeService, ICurrentUserService currentUserService)
        {
            _tradeService = tradeService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Trade.View)]
        [ProducesResponseType(typeof(PaginatedList<TradeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTrades([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _tradeService.GetTradesAsync(new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Trade.View)]
        [ProducesResponseType(typeof(TradeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTradeById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _tradeService.GetTradeByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Trade.Create)]
        [ProducesResponseType(typeof(TradeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTrade([FromBody] CreateTradeRequest request, CancellationToken cancellationToken)
        {
            var result = await _tradeService.CreateTradeAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Trade.Edit)]
        [ProducesResponseType(typeof(TradeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTrade(Guid id, [FromBody] UpdateTradeRequest request, CancellationToken cancellationToken)
        {
            var result = await _tradeService.UpdateTradeAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}/assign-head")]
        [Authorize(Policy = Permissions.Trade.AssignHead)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignHead(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            var result = await _tradeService.AssignHeadAsync(id, userId, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Trade.Archive)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTrade(Guid id, CancellationToken cancellationToken)
        {
            if (!_currentUserService.HasRole(RoleConstants.Admin))
                return Forbid();

            var result = await _tradeService.PermanentDeleteTradeAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/archive-impact")]
        [Authorize(Policy = Permissions.Trade.Archive)]
        [ProducesResponseType(typeof(TradeArchiveImpactDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTradeArchiveImpact(Guid id, CancellationToken cancellationToken)
        {
            var result = await _tradeService.GetTradeArchiveImpactAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/archive")]
        [Authorize(Policy = Permissions.Trade.Archive)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ArchiveTrade(Guid id, CancellationToken cancellationToken)
        {
            var result = await _tradeService.ArchiveTradeAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/restore")]
        [Authorize(Policy = Permissions.Trade.Archive)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreTrade(Guid id, CancellationToken cancellationToken)
        {
            var result = await _tradeService.RestoreTradeAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/delete-impact")]
        [Authorize(Policy = Permissions.Trade.Archive)]
        [ProducesResponseType(typeof(TradeDeleteImpactDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTradeDeleteImpact(Guid id, CancellationToken cancellationToken)
        {
            if (!_currentUserService.HasRole(RoleConstants.Admin))
                return Forbid();

            var result = await _tradeService.GetTradeDeleteImpactAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("by-institute/{instituteId}")]
        [Authorize(Policy = Permissions.Trade.View)]
        [ProducesResponseType(typeof(PaginatedList<TradeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTradesByInstitute(Guid instituteId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _tradeService.GetTradesByInstituteAsync(instituteId, new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("export")]
        [Authorize(Policy = Permissions.Trade.View)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportTrades(CancellationToken cancellationToken)
        {
            var result = await _tradeService.ExportTradesAsync(cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Trades.xlsx");
        }
    }
}
