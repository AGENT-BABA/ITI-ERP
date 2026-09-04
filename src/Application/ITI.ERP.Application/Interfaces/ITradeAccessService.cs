using System.Linq.Expressions;
using ITI.ERP.Application.Common.Models;

namespace ITI.ERP.Application.Interfaces;

public interface ITradeAccessService
{
    bool IsTradeHead { get; }
    bool IsAdmin { get; }
    bool IsInstituteAdmin { get; }
    Guid? EffectiveTradeId { get; }
    Guid? EffectiveBatchId { get; }

    Task<Result> ValidateTradeAccessAsync(Guid tradeId, CancellationToken ct = default);
    Task<Result> ValidateStudentAccessAsync(Guid studentId, CancellationToken ct = default);
    Task<Result> ValidatePracticalAccessAsync(Guid practicalId, bool isMonthly, CancellationToken ct = default);
    Task<Result> ValidateBatchAccessAsync(Guid batchId, CancellationToken ct = default);
    IQueryable<T> ApplyTradeFilter<T>(IQueryable<T> query, Expression<Func<T, Guid>> getTradeId) where T : class;
    IQueryable<T> ApplyBatchFilter<T>(IQueryable<T> query, Expression<Func<T, Guid?>> getBatchId) where T : class;
}
