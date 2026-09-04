namespace ITI.ERP.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(string containerPath, string fileName, Stream fileStream, CancellationToken ct);
    Task<Stream> DownloadAsync(string filePath, CancellationToken ct);
    Task DeleteAsync(string filePath, CancellationToken ct);
    Task<bool> ExistsAsync(string filePath, CancellationToken ct);
    string GetPublicUrl(string filePath);
}
