using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Application.DTOs.AcademicSession;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/academic-sessions")]
    [ApiVersion("1.0")]
    [Authorize]
    public class AcademicSessionController : BaseApiController
    {
        private readonly IAcademicSessionService _academicSessionService;

        public AcademicSessionController(IAcademicSessionService academicSessionService)
        {
            _academicSessionService = academicSessionService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.AcademicSession.View)]
        [ProducesResponseType(typeof(PaginatedList<AcademicSessionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAcademicSessions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var result = await _academicSessionService.GetAcademicSessionsAsync(new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm }, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.AcademicSession.View)]
        [ProducesResponseType(typeof(AcademicSessionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAcademicSessionById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _academicSessionService.GetAcademicSessionByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.AcademicSession.Create)]
        [ProducesResponseType(typeof(AcademicSessionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAcademicSession([FromBody] CreateAcademicSessionRequest request, CancellationToken cancellationToken)
        {
            var result = await _academicSessionService.CreateAcademicSessionAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.AcademicSession.Edit)]
        [ProducesResponseType(typeof(AcademicSessionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAcademicSession(Guid id, [FromBody] UpdateAcademicSessionRequest request, CancellationToken cancellationToken)
        {
            var result = await _academicSessionService.UpdateAcademicSessionAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}/activate")]
        [Authorize(Policy = Permissions.AcademicSession.Activate)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActivateSession(Guid id, CancellationToken cancellationToken)
        {
            var result = await _academicSessionService.ActivateSessionAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}/lock")]
        [Authorize(Policy = Permissions.AcademicSession.Lock)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LockSession(Guid id, CancellationToken cancellationToken)
        {
            var result = await _academicSessionService.LockSessionAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.AcademicSession.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAcademicSession(Guid id, CancellationToken cancellationToken)
        {
            var result = await _academicSessionService.DeleteAcademicSessionAsync(id, cancellationToken);
            return HandleResult(result);
        }
    }
}
