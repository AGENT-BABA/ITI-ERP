using ITI.ERP.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ITI.ERP.Infrastructure.FileStorage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly FileStorageOptions _options;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(
            IOptions<FileStorageOptions> options,
            ILogger<LocalFileStorageService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> UploadAsync(string containerPath, string fileName, Stream fileStream, CancellationToken cancellationToken = default)
        {
            var relativePath = Path.Combine(containerPath, Guid.NewGuid().ToString(), fileName);
            var fullPath = Path.Combine(_options.LocalBasePath, relativePath);

            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStreamOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

            _logger.LogInformation("File uploaded to {Path}", relativePath);

            return relativePath;
        }

        public Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_options.LocalBasePath, path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {path}");
            }

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
            return Task.FromResult(stream);
        }

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_options.LocalBasePath, path);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted from {Path}", path);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_options.LocalBasePath, path);
            return Task.FromResult(File.Exists(fullPath));
        }

        public string GetPublicUrl(string path)
        {
            return $"{_options.LocalBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        }
    }
}
