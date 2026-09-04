using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.DTOs.StudentImport;

public class StudentImportHistoryDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ImportedByName { get; set; } = string.Empty;
    public DateTime ImportDate { get; set; }
    public int NumberOfStudentsImported { get; set; }
    public ImportStatus Status { get; set; }
    public string? ValidationErrors { get; set; }
    public string TradeName { get; set; } = string.Empty;
}
