namespace ITI.ERP.Application.DTOs.StudentImport;

public class StudentImportValidationError
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
