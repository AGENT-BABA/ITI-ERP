using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.StudentImport;

namespace ITI.ERP.Application.Interfaces;

public interface IStudentImportService
{
    Task<Result<StudentImportPreviewDto>> PreviewImportAsync(Guid tradeId, Stream fileStream, string fileName, Guid? batchId, CancellationToken ct);
    Task<Result<StudentImportResultDto>> ConfirmImportAsync(StudentImportPreviewDto preview, CancellationToken ct);
    Task<Result<byte[]>> DownloadTemplateAsync(CancellationToken ct);
    Task<Result<PaginatedList<StudentImportHistoryDto>>> GetImportHistoryAsync(PaginationRequest request, CancellationToken ct);
}
