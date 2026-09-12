using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Student;

namespace ITI.ERP.Application.Interfaces;

public interface IStudentService
{
    Task<Result<PaginatedList<StudentDto>>> GetStudentsAsync(PaginationRequest request, CancellationToken ct);
    Task<Result<PaginatedList<StudentDto>>> GetArchivedStudentsAsync(PaginationRequest request, CancellationToken ct);
    Task<Result<StudentDto>> GetStudentByIdAsync(Guid id, CancellationToken ct);
    Task<Result<StudentDto>> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct);
    Task<Result<StudentDto>> UpdateStudentAsync(Guid id, UpdateStudentRequest request, CancellationToken ct);
    Task<Result> TransferStudentAsync(Guid id, TransferStudentRequest request, CancellationToken ct);
    Task<Result> ChangeBatchAsync(Guid id, ChangeBatchRequest request, CancellationToken ct);
    Task<Result> ChangeStatusAsync(Guid id, ChangeStudentStatusRequest request, CancellationToken ct);
    Task<Result> ArchiveStudentAsync(Guid id, string reason, CancellationToken ct);
    Task<Result> UnarchiveStudentAsync(Guid id, string reason, CancellationToken ct);
    Task<Result> DeleteStudentAsync(Guid id, string reason, CancellationToken ct);
    Task<Result> UploadPhotoAsync(Guid id, Stream fileStream, string fileName, CancellationToken ct);
}
