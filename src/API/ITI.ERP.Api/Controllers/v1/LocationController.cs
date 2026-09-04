using Asp.Versioning;
using ITI.ERP.Application.Location;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/locations")]
[ApiVersion("1.0")]
[Authorize]
public class LocationController : BaseApiController
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet("states")]
    [ProducesResponseType(typeof(List<LocationStateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates(CancellationToken cancellationToken)
    {
        var states = await _locationService.GetStatesAsync(cancellationToken);
        return Ok(states);
    }

    [HttpGet("districts")]
    [ProducesResponseType(typeof(List<LocationDistrictDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistricts([FromQuery] string state, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state))
            return BadRequest("State is required.");

        var districts = await _locationService.GetDistrictsAsync(state.Trim(), cancellationToken);
        return Ok(districts);
    }

    [HttpGet("cities")]
    [ProducesResponseType(typeof(List<LocationCityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities([FromQuery] string state, [FromQuery] string district, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(district))
            return BadRequest("State and District are required.");

        var cities = await _locationService.GetCitiesAsync(state.Trim(), district.Trim(), cancellationToken);
        return Ok(cities);
    }

    [HttpGet("pin/{pinCode}")]
    [ProducesResponseType(typeof(PinLookupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupPin(string pinCode, CancellationToken cancellationToken)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(pinCode, @"^\d{6}$"))
            return BadRequest("Pin Code must be exactly 6 digits.");

        var result = await _locationService.LookupPinAsync(pinCode.Trim(), cancellationToken);
        if (result is null)
            return NotFound($"No data found for PIN code {pinCode}.");

        return Ok(result);
    }
}
