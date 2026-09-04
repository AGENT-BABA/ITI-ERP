using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Application.DTOs.Institute;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/institutes")]
    [ApiVersion("1.0")]
    [Authorize]
    public class InstituteController : BaseApiController
    {
        private readonly IInstituteService _instituteService;

        public InstituteController(IInstituteService instituteService)
        {
            _instituteService = instituteService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Institute.View)]
        public async Task<IActionResult> GetInstitutes([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var result = await _instituteService.GetInstitutesAsync(new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm }, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Institute.View)]
        public async Task<IActionResult> GetInstituteById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _instituteService.GetInstituteByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Institute.Edit)]
        public async Task<IActionResult> CreateInstitute([FromBody] CreateInstituteRequest request, CancellationToken cancellationToken)
        {
            var result = await _instituteService.CreateInstituteAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Institute.Edit)]
        public async Task<IActionResult> UpdateInstitute(Guid id, [FromBody] UpdateInstituteRequest request, CancellationToken cancellationToken)
        {
            var result = await _instituteService.UpdateInstituteAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Institute.Delete)]
        public async Task<IActionResult> DeleteInstitute(Guid id, CancellationToken cancellationToken)
        {
            var result = await _instituteService.DeleteInstituteAsync(id, cancellationToken);
            return HandleResult(result);
        }
    }
}
