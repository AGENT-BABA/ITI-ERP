using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Practical;

namespace ITI.ERP.Application.Interfaces;

public interface IPracticalService
{
    Task<Result<PaginatedList<MonthlyPracticalDto>>> GetMonthlyPracticalsAsync(PaginationRequest request, CancellationToken ct);
    Task<Result<MonthlyPracticalDto>> GetMonthlyPracticalByIdAsync(Guid id, CancellationToken ct);
    Task<Result<MonthlyPracticalDto>> CreateMonthlyPracticalAsync(CreateMonthlyPracticalRequest request, CancellationToken ct);
    Task<Result<MonthlyPracticalDto>> UpdateMonthlyPracticalAsync(Guid id, UpdateMonthlyPracticalRequest request, CancellationToken ct);
    Task<Result> DeleteMonthlyPracticalAsync(Guid id, CancellationToken ct);
    Task<Result<List<PracticalMarkDto>>> GetPracticalMarksAsync(Guid monthlyPracticalId, CancellationToken ct);
    Task<Result> SubmitPracticalMarksAsync(SubmitPracticalMarksRequest request, CancellationToken ct);
    Task<Result> LockPracticalAsync(Guid monthlyPracticalId, CancellationToken ct);
    Task<Result> UnlockPracticalAsync(Guid monthlyPracticalId, string reason, CancellationToken ct);
    Task<Result<PracticalReportDto>> GetPracticalReportAsync(Guid monthlyPracticalId, CancellationToken ct);
}
