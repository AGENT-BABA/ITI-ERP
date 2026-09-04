namespace ITI.ERP.Application.DTOs.TradeMaster;

public class TradeMasterPreviewDto
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public List<TradeMasterRowDto> Rows { get; set; } = new();
    public List<TradeMasterValidationError> Errors { get; set; } = new();
    public string? FileName { get; set; }
}
