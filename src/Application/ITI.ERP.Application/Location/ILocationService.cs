namespace ITI.ERP.Application.Location;

public interface ILocationService
{
    Task<List<LocationStateDto>> GetStatesAsync(CancellationToken ct = default);
    Task<List<LocationDistrictDto>> GetDistrictsAsync(string state, CancellationToken ct = default);
    Task<List<LocationCityDto>> GetCitiesAsync(string state, string district, CancellationToken ct = default);
    Task<PinLookupDto?> LookupPinAsync(string pinCode, CancellationToken ct = default);
}
