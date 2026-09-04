using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ITI.ERP.Application.Location;

public class LocationService : ILocationService
{
    private static List<StateData>? _stateData;
    private static Dictionary<string, List<PinCodeEntry>>? _pinDatabase;
    private static readonly object _pinLock = new();

    public Task<List<LocationStateDto>> GetStatesAsync(CancellationToken ct = default)
    {
        var data = LoadData();
        var states = data
            .Select(s => new LocationStateDto { Name = s.Name })
            .OrderBy(s => s.Name)
            .ToList();
        return Task.FromResult(states);
    }

    public Task<List<LocationDistrictDto>> GetDistrictsAsync(string state, CancellationToken ct = default)
    {
        var data = LoadData();
        var stateEntry = data.FirstOrDefault(s =>
            string.Equals(s.Name, state, StringComparison.OrdinalIgnoreCase));
        if (stateEntry is null)
            return Task.FromResult(new List<LocationDistrictDto>());

        var districts = stateEntry.Districts
            .Select(d => new LocationDistrictDto { Name = d.Name })
            .OrderBy(d => d.Name)
            .ToList();
        return Task.FromResult(districts);
    }

    public Task<List<LocationCityDto>> GetCitiesAsync(string state, string district, CancellationToken ct = default)
    {
        var data = LoadData();
        var stateEntry = data.FirstOrDefault(s =>
            string.Equals(s.Name, state, StringComparison.OrdinalIgnoreCase));
        if (stateEntry is null)
            return Task.FromResult(new List<LocationCityDto>());

        var districtEntry = stateEntry.Districts.FirstOrDefault(d =>
            string.Equals(d.Name, district, StringComparison.OrdinalIgnoreCase));
        if (districtEntry is null)
            return Task.FromResult(new List<LocationCityDto>());

        var cities = districtEntry.Cities
            .Select(c => new LocationCityDto { Name = c })
            .OrderBy(c => c.Name)
            .ToList();
        return Task.FromResult(cities);
    }

    public Task<PinLookupDto?> LookupPinAsync(string pinCode, CancellationToken ct = default)
    {
        if (!Regex.IsMatch(pinCode, @"^\d{6}$"))
            return Task.FromResult<PinLookupDto?>(null);

        var db = EnsurePinDatabaseLoaded();
        if (!db.TryGetValue(pinCode, out var entries) || entries.Count == 0)
            return Task.FromResult<PinLookupDto?>(null);

        var first = entries[0];
        var result = new PinLookupDto
        {
            PinCode = pinCode,
            State = first.State,
            District = first.District,
            PostOffices = entries.Select(e => new PostOfficeDto
            {
                Name = e.Name,
                District = e.District,
                State = e.State,
                Taluk = e.Taluk,
                Division = e.Division
            }).ToList()
        };

        return Task.FromResult<PinLookupDto?>(result);
    }

    public Task<List<PinLookupDto>> LookupAllPostOfficesAsync(string pinCode, CancellationToken ct = default)
    {
        if (!Regex.IsMatch(pinCode, @"^\d{6}$"))
            return Task.FromResult(new List<PinLookupDto>());

        var db = EnsurePinDatabaseLoaded();
        if (!db.TryGetValue(pinCode, out var entries) || entries.Count == 0)
            return Task.FromResult(new List<PinLookupDto>());

        var results = entries.Select(e => new PinLookupDto
        {
            PinCode = pinCode,
            State = e.State,
            District = e.District,
            PostOffices = new List<PostOfficeDto>
            {
                new()
                {
                    Name = e.Name,
                    District = e.District,
                    State = e.State,
                    Taluk = e.Taluk,
                    Division = e.Division
                }
            }
        }).ToList();

        return Task.FromResult(results);
    }

    private static Dictionary<string, List<PinCodeEntry>> EnsurePinDatabaseLoaded()
    {
        if (_pinDatabase is not null)
            return _pinDatabase;

        lock (_pinLock)
        {
            if (_pinDatabase is not null)
                return _pinDatabase;

            var assembly = Assembly.GetAssembly(typeof(LocationService))!;
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("IndiaPinCodes.json", StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
            {
                _pinDatabase = new Dictionary<string, List<PinCodeEntry>>(StringComparer.Ordinal);
                return _pinDatabase;
            }

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            var raw = JsonSerializer.Deserialize<Dictionary<string, List<PinCodeEntry>>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _pinDatabase = raw ?? new Dictionary<string, List<PinCodeEntry>>(StringComparer.Ordinal);
            return _pinDatabase;
        }
    }

    private static List<StateData> LoadData()
    {
        if (_stateData is not null)
            return _stateData;

        var assembly = Assembly.GetAssembly(typeof(LocationService))!;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("IndiaStates.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            _stateData = new List<StateData>();
            return _stateData;
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        _stateData = JsonSerializer.Deserialize<List<StateData>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<StateData>();
        return _stateData;
    }

    private class StateData
    {
        public string Name { get; set; } = string.Empty;
        public List<DistrictData> Districts { get; set; } = new();
    }

    private class DistrictData
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Cities { get; set; } = new();
    }

    public class PinCodeEntry
    {
        public string Name { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? Taluk { get; set; }
        public string? Division { get; set; }
    }
}
