using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Application.DTOs.AuditLog;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/audit-logs")]
    [ApiVersion("1.0")]
    [Authorize]
    public class AuditLogController : BaseApiController
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Reports.View)]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _auditLogService.GetAuditLogsAsync(new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
            return HandleResult(result);
        }
    }
}
