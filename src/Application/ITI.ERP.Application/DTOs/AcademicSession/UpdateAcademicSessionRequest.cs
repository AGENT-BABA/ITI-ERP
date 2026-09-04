namespace ITI.ERP.Application.DTOs.AcademicSession;

public class UpdateAcademicSessionRequest
{
    public string? SessionYear { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
