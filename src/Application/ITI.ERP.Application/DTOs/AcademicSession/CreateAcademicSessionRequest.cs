namespace ITI.ERP.Application.DTOs.AcademicSession;

public class CreateAcademicSessionRequest
{
    public string SessionYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? InstituteId { get; set; }
}
