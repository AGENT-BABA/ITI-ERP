namespace ITI.ERP.Application.DTOs.StudentImport;

public class StudentImportResultDto
{
    public bool Success { get; set; }
    public int StudentsImported { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
