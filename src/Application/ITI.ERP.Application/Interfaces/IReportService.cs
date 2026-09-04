using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Reports;

namespace ITI.ERP.Application.Interfaces;

public interface IReportService
{
    Task<Result<StudentAttendanceReportDto>> GetStudentAttendanceReportAsync(Guid studentId, DateTime fromDate, DateTime toDate, CancellationToken ct);
    Task<Result<TradeAttendanceReportDto>> GetTradeAttendanceReportAsync(Guid tradeId, DateTime fromDate, DateTime toDate, CancellationToken ct);
    Task<Result<MonthlyPracticalReportDto>> GetMonthlyPracticalReportAsync(Guid monthlyPracticalId, CancellationToken ct);
    Task<Result<YearlyPracticalReportDto>> GetYearlyPracticalReportAsync(Guid yearlyPracticalId, CancellationToken ct);
    Task<Result<StudentPerformanceReportDto>> GetStudentPerformanceReportAsync(Guid studentId, CancellationToken ct);
    Task<Result<InstituteSummaryReportDto>> GetInstituteSummaryReportAsync(Guid instituteId, CancellationToken ct);
    Task<Result<ProgressiveAttendanceReportDto>> GetProgressiveAttendanceReportAsync(Guid studentId, CancellationToken ct);
    Task<Result<ProgressCardDto>> GetProgressCardAsync(Guid studentId, CancellationToken ct);
}
