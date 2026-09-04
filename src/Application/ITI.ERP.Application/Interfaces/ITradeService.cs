using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Trade;

namespace ITI.ERP.Application.Interfaces;

public interface ITradeService
{
    Task<Result<PaginatedList<TradeDto>>> GetTradesAsync(PaginationRequest request, CancellationToken ct = default);
    Task<Result<TradeDto>> GetTradeByIdAsync(Guid id, CancellationToken ct);
    Task<Result<TradeDto>> CreateTradeAsync(CreateTradeRequest request, CancellationToken ct);
    Task<Result<TradeDto>> UpdateTradeAsync(Guid id, UpdateTradeRequest request, CancellationToken ct);
    Task<Result> AssignHeadAsync(Guid tradeId, Guid userId, CancellationToken ct);
    Task<Result> DeleteTradeAsync(Guid id, CancellationToken ct);
    Task<Result> ArchiveTradeAsync(Guid id, CancellationToken ct);
    Task<Result<TradeArchiveImpactDto>> GetTradeArchiveImpactAsync(Guid id, CancellationToken ct);
    Task<Result> RestoreTradeAsync(Guid id, CancellationToken ct);
    Task<Result> PermanentDeleteTradeAsync(Guid id, CancellationToken ct);
    Task<Result<TradeDeleteImpactDto>> GetTradeDeleteImpactAsync(Guid id, CancellationToken ct);
    Task<Result<byte[]>> ExportTradesAsync(CancellationToken ct);
    Task<Result<PaginatedList<TradeDto>>> GetTradesByInstituteAsync(Guid instituteId, PaginationRequest request, CancellationToken ct);
}
