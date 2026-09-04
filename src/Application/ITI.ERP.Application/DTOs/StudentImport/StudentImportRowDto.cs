namespace ITI.ERP.Application.DTOs.StudentImport;

public class StudentImportRowDto
{
    public int RowNumber { get; set; }
    public string ApplicationIdDisplay { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string DOB { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string AllottedCategory { get; set; } = string.Empty;
    public string AllottedRound { get; set; } = string.Empty;
    public string AdmittedDateTime { get; set; } = string.Empty;
    public DateTime? ParsedDOB { get; set; }
    public int? ParsedGender { get; set; }
    public DateTime? ParsedAdmittedDateTime { get; set; }
}
