using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Batch;

namespace ITI.ERP.Application.Interfaces;

public interface IBatchService
{
    Task<Result<PaginatedList<BatchDto>>> GetBatchesAsync(Guid? instituteId, Guid? tradeId, Guid? academicSessionId, string? sessionYear, bool? isDeleted, PaginationRequest request, CancellationToken ct);
    Task<Result<BatchDto>> GetBatchByIdAsync(Guid id, Guid? academicSessionId, CancellationToken ct);
    Task<Result<List<BatchDto>>> GetBatchesByTradeAsync(Guid tradeId, Guid? academicSessionId, CancellationToken ct);
    Task<Result<BatchDto>> CreateBatchAsync(CreateBatchRequest request, CancellationToken ct);
    Task<Result<BatchDto>> UpdateBatchAsync(Guid id, UpdateBatchRequest request, Guid? academicSessionId, CancellationToken ct);
    Task<Result> DeleteBatchAsync(Guid id, CancellationToken ct);
    Task<Result> ArchiveBatchAsync(Guid id, CancellationToken ct);
    Task<Result<BatchArchiveImpactDto>> GetBatchArchiveImpactAsync(Guid id, CancellationToken ct);
    Task<Result> RestoreBatchAsync(Guid id, CancellationToken ct);
    Task<Result> PermanentDeleteBatchAsync(Guid id, CancellationToken ct);
    Task<Result<BatchDeleteImpactDto>> GetBatchDeleteImpactAsync(Guid id, CancellationToken ct);
    Task<Result<BatchPurgeResultDto>> PermanentDeleteBatchInternalAsync(Guid id, CancellationToken ct);
}
