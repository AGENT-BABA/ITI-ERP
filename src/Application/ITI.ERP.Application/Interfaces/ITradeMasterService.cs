using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.TradeMaster;

namespace ITI.ERP.Application.Interfaces;

public interface ITradeMasterService
{
    Task<Result<TradeMasterPreviewDto>> PreviewImportAsync(Stream fileStream, string fileName, Guid? instituteIdOverride = null, CancellationToken ct = default);
    Task<Result<TradeMasterImportResultDto>> ConfirmImportAsync(TradeMasterPreviewDto preview, Guid? instituteIdOverride = null, CancellationToken ct = default);
    Task<Result<byte[]>> DownloadTemplateAsync(CancellationToken ct);
    Task<Result<PaginatedList<TradeMasterHistoryDto>>> GetImportHistoryAsync(PaginationRequest request, CancellationToken ct);
}
