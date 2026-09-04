namespace ITI.ERP.Application.DTOs.StudentImport;

public class StudentImportPreviewDto
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public List<StudentImportRowDto> Rows { get; set; } = new();
    public List<StudentImportValidationError> Errors { get; set; } = new();
    public string? FileName { get; set; }
    public Guid TradeId { get; set; }
    public Guid? BatchId { get; set; }
    public string TradeName { get; set; } = string.Empty;
    public string InstituteName { get; set; } = string.Empty;
    public string SessionYear { get; set; } = string.Empty;
}
