namespace ITI.ERP.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? InstituteId { get; }
    Guid? AcademicSessionId { get; }
    Guid? TradeId { get; }
    Guid? BatchId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    bool HasRole(string role);
}
