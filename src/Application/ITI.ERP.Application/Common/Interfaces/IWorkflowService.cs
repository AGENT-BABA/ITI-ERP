using ITI.ERP.Application.Common.Models;
using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.Common.Interfaces;

public interface IWorkflowService
{
    Task<Result<DraftStatus>> SubmitAsync(DraftEntity entity, Guid userId, CancellationToken ct);
    Task<Result<DraftStatus>> VerifyAsync(DraftEntity entity, Guid userId, CancellationToken ct);
    Task<Result<DraftStatus>> LockAsync(DraftEntity entity, Guid userId, CancellationToken ct);
    Task<Result<DraftStatus>> UnlockAsync(DraftEntity entity, Guid userId, string reason, CancellationToken ct);
    Task<Result<DraftStatus>> FinalizeAsync(DraftEntity entity, Guid userId, CancellationToken ct);
    Task<Result<DraftStatus>> ArchiveAsync(DraftEntity entity, Guid userId, CancellationToken ct);
    Task<Result<DraftStatus>> AutoSaveAsync(DraftEntity entity, Guid userId, CancellationToken ct);
}
