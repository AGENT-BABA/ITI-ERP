using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Domain.Entities;

public class Trade : DraftEntity
{
    public Guid? AcademicSessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DurationInMonths { get; set; }
    public int TotalSeats { get; set; } = 0;
    public Guid? HeadUserId { get; set; }
    public DraftStatus? PreviousDraftStatus { get; set; }

    public Institute Institute { get; set; } = null!;
    public AcademicSession? AcademicSession { get; set; }
    public User? HeadUser { get; set; }
}
