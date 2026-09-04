using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Attendance;

namespace ITI.ERP.Application.Interfaces;

public interface IAttendanceService
{
    Task<Result<List<AttendanceRecordDto>>> GetAttendanceByDateAsync(Guid tradeId, DateTime date, CancellationToken ct);
    Task<Result<AttendanceSummaryDto>> GetAttendanceSummaryAsync(Guid tradeId, DateTime date, CancellationToken ct);
    Task<Result<List<AttendanceSummaryDto>>> GetAttendanceSummaryRangeAsync(Guid tradeId, DateTime fromDate, DateTime toDate, CancellationToken ct);
    Task<Result<AttendanceStudentSummaryDto>> GetStudentAttendanceSummaryAsync(Guid studentId, CancellationToken ct);
    Task<Result<AttendanceTradeReportDto>> GetTradeAttendanceReportAsync(Guid tradeId, DateTime fromDate, DateTime toDate, CancellationToken ct);
    Task<Result> MarkAttendanceAsync(MarkAttendanceRequest request, CancellationToken ct);
    Task<Result> LockAttendanceAsync(Guid tradeId, DateTime date, CancellationToken ct);
    Task<Result> UnlockAttendanceAsync(Guid tradeId, DateTime date, string reason, CancellationToken ct);
}
