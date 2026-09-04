using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.DTOs.Student;

public class ChangeStudentStatusRequest
{
    public StudentStatus NewStatus { get; set; }
    public string Reason { get; set; } = string.Empty;
}
