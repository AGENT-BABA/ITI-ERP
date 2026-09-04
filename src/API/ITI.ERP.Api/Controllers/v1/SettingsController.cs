using Asp.Versioning;
using ITI.ERP.Application.DTOs.Settings;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/settings")]
[ApiVersion("1.0")]
[Authorize]
public class SettingsController : BaseApiController
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Settings.Manage)]
    [ProducesResponseType(typeof(InstituteSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _settingsService.GetSettingsAsync(ct);
        return HandleResult(result);
    }

    [HttpPut]
    [Authorize(Policy = Permissions.Settings.Manage)]
    [ProducesResponseType(typeof(InstituteSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateInstituteSettingsRequest request, CancellationToken ct)
    {
        var result = await _settingsService.UpdateSettingsAsync(request, ct);
        return HandleResult(result);
    }
}
