using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.YearlyPractical;

namespace ITI.ERP.Application.Interfaces;

public interface IYearlyPracticalService
{
    Task<Result<PaginatedList<YearlyPracticalDto>>> GetYearlyPracticalsAsync(PaginationRequest request, CancellationToken ct);
    Task<Result<YearlyPracticalDto>> GetYearlyPracticalByIdAsync(Guid id, CancellationToken ct);
    Task<Result<YearlyPracticalDto>> CreateYearlyPracticalAsync(CreateYearlyPracticalRequest request, CancellationToken ct);
    Task<Result<YearlyPracticalDto>> UpdateYearlyPracticalAsync(Guid id, UpdateYearlyPracticalRequest request, CancellationToken ct);
    Task<Result> DeleteYearlyPracticalAsync(Guid id, CancellationToken ct);
    Task<Result<List<YearlyPracticalMarkDto>>> GetYearlyPracticalMarksAsync(Guid yearlyPracticalId, CancellationToken ct);
    Task<Result> SubmitYearlyPracticalMarksAsync(SubmitYearlyPracticalMarksRequest request, CancellationToken ct);
    Task<Result> LockYearlyPracticalAsync(Guid yearlyPracticalId, CancellationToken ct);
    Task<Result> UnlockYearlyPracticalAsync(Guid yearlyPracticalId, string reason, CancellationToken ct);
    Task<Result<YearlyPracticalReportDto>> GetYearlyPracticalReportAsync(Guid yearlyPracticalId, CancellationToken ct);
    Task<Result<StudentYearlyPerformanceDto>> GetStudentYearlyPerformanceAsync(Guid studentId, CancellationToken ct);
    Task<Result<List<YearlyPracticalStudentListDto>>> GetYearlyPracticalStudentsAsync(Guid yearlyPracticalId, CancellationToken ct);
    Task<Result<YearlyPracticalStudentDetailDto>> GetYearlyPracticalStudentDetailAsync(Guid yearlyPracticalId, Guid studentId, CancellationToken ct);
    Task<Result> SaveStudentYearlyEntriesAsync(Guid yearlyPracticalId, Guid studentId, SaveYearlyEntriesRequest request, CancellationToken ct);
}
