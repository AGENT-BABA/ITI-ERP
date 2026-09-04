using Asp.Versioning;
using ITI.ERP.Application.DTOs.Holiday;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/holidays")]
[ApiVersion("1.0")]
[Authorize]
public class HolidayController : BaseApiController
{
    private readonly IHolidayService _holidayService;

    public HolidayController(IHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Holiday.View)]
    public async Task<IActionResult> GetHolidays([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _holidayService.GetHolidaysAsync(from, to, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Holiday.Create)]
    public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayRequest request, CancellationToken cancellationToken)
    {
        var result = await _holidayService.CreateHolidayAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Holiday.Delete)]
    public async Task<IActionResult> DeleteHoliday(Guid id, CancellationToken cancellationToken)
    {
        var result = await _holidayService.DeleteHolidayAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
