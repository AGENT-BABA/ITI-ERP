namespace ITI.ERP.Application.Location;

public class LocationStateDto
{
    public string Name { get; set; } = string.Empty;
}

public class LocationDistrictDto
{
    public string Name { get; set; } = string.Empty;
}

public class LocationCityDto
{
    public string Name { get; set; } = string.Empty;
}

public class PinLookupDto
{
    public string PinCode { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public List<PostOfficeDto> PostOffices { get; set; } = new();
}

public class PostOfficeDto
{
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Taluk { get; set; }
    public string? Division { get; set; }
}
