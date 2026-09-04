using System.Linq.Expressions;
using ITI.ERP.Application.Common.Exceptions;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class TradeAccessService : ITradeAccessService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    private const string TradeHeadRole = "TradeHead";
    private const string InstituteAdminRole = "InstituteAdmin";

    public TradeAccessService(ICurrentUserService currentUserService, IApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
    }

    public bool IsTradeHead => _currentUserService.HasRole(TradeHeadRole);

    public bool IsAdmin => _currentUserService.HasRole("Admin");

    public bool IsInstituteAdmin => _currentUserService.HasRole(InstituteAdminRole);

    public Guid? EffectiveTradeId => _currentUserService.TradeId;

    public Guid? EffectiveBatchId => _currentUserService.BatchId;

    private void EnsureTradeIdConfigured()
    {
        if (IsTradeHead && !EffectiveTradeId.HasValue)
        {
            throw new ForbiddenAccessException(
                "Configuration error: TradeHead role requires an assigned trade. " +
                "Contact your administrator to link your account to a trade.");
        }
    }

    private void EnsureBatchIdConfigured()
    {
        if (IsTradeHead && !EffectiveBatchId.HasValue)
        {
            throw new ForbiddenAccessException(
                "Configuration error: TradeHead role requires an assigned batch. " +
                "Contact your administrator to link your account to a batch.");
        }
    }

    public IQueryable<T> ApplyTradeFilter<T>(IQueryable<T> query, Expression<Func<T, Guid>> getTradeId) where T : class
    {
        if (!IsTradeHead)
            return query;

        EnsureTradeIdConfigured();

        var tradeId = EffectiveTradeId!.Value;
        var parameter = getTradeId.Parameters[0];
        var body = Expression.Equal(getTradeId.Body, Expression.Constant(tradeId));
        var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
        return query.Where(predicate);
    }

    public IQueryable<T> ApplyBatchFilter<T>(IQueryable<T> query, Expression<Func<T, Guid?>> getBatchId) where T : class
    {
        if (!IsTradeHead)
            return query;

        EnsureBatchIdConfigured();

        var batchId = EffectiveBatchId!.Value;
        var parameter = getBatchId.Parameters[0];
        var body = Expression.Equal(getBatchId.Body, Expression.Constant(batchId, typeof(Guid?)));
        var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
        return query.Where(predicate);
    }

    public async Task<Result> ValidateTradeAccessAsync(Guid tradeId, CancellationToken ct = default)
    {
        if (!IsTradeHead)
            return Result.Success();

        EnsureTradeIdConfigured();

        if (tradeId != EffectiveTradeId!.Value)
            return Result.Failure("Access denied: you can only access your assigned trade.");

        return Result.Success();
    }

    public async Task<Result> ValidateStudentAccessAsync(Guid studentId, CancellationToken ct = default)
    {
        if (!IsTradeHead)
            return Result.Success();

        EnsureTradeIdConfigured();
        EnsureBatchIdConfigured();

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId, ct);

        if (student is null)
            return Result.Failure("Student not found.");

        if (student.TradeId != EffectiveTradeId!.Value)
            return Result.Failure("Access denied: student does not belong to your trade.");

        if (student.BatchId != EffectiveBatchId!.Value)
            return Result.Failure("Access denied: student does not belong to your batch.");

        return Result.Success();
    }

    public async Task<Result> ValidatePracticalAccessAsync(Guid practicalId, bool isMonthly, CancellationToken ct = default)
    {
        if (!IsTradeHead)
            return Result.Success();

        EnsureTradeIdConfigured();
        EnsureBatchIdConfigured();

        if (isMonthly)
        {
            var practical = await _context.MonthlyPracticals
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == practicalId, ct);

            if (practical is null)
                return Result.Failure("Monthly practical not found.");

            if (practical.TradeId != EffectiveTradeId!.Value)
                return Result.Failure("Access denied: practical does not belong to your trade.");

            if (practical.BatchId != EffectiveBatchId!.Value)
                return Result.Failure("Access denied: practical does not belong to your batch.");
        }
        else
        {
            var practical = await _context.YearlyPracticals
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == practicalId, ct);

            if (practical is null)
                return Result.Failure("Yearly practical not found.");

            if (practical.TradeId != EffectiveTradeId!.Value)
                return Result.Failure("Access denied: practical does not belong to your trade.");

            if (practical.BatchId != EffectiveBatchId!.Value)
                return Result.Failure("Access denied: practical does not belong to your batch.");
        }

        return Result.Success();
    }

    public async Task<Result> ValidateBatchAccessAsync(Guid batchId, CancellationToken ct = default)
    {
        if (!IsTradeHead)
            return Result.Success();

        EnsureTradeIdConfigured();

        var batch = await _context.Batches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDeleted, ct);

        if (batch is null)
            return Result.Failure("Batch not found.");

        if (batch.TradeId != EffectiveTradeId!.Value)
            return Result.Failure("Access denied: batch does not belong to your trade.");

        return Result.Success();
    }
}
