using ITI.ERP.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ITI.ERP.Infrastructure.FileStorage
{
    public class FileStorageFactory : IFileStorageService
    {
        private readonly IFileStorageService _service;
        private readonly FileStorageOptions _options;

        public FileStorageFactory(
            IOptions<FileStorageOptions> options,
            IServiceProvider serviceProvider)
        {
            _options = options.Value;

            _service = _options.Provider switch
            {
                "Local" => serviceProvider.GetRequiredService<LocalFileStorageService>(),
                _ => throw new NotSupportedException($"File storage provider '{_options.Provider}' is not supported.")
            };
        }

        public Task<string> UploadAsync(string containerPath, string fileName, Stream fileStream, CancellationToken cancellationToken = default)
        {
            return _service.UploadAsync(containerPath, fileName, fileStream, cancellationToken);
        }

        public Task<Stream> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return _service.DownloadAsync(filePath, cancellationToken);
        }

        public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return _service.DeleteAsync(filePath, cancellationToken);
        }

        public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return _service.ExistsAsync(filePath, cancellationToken);
        }

        public string GetPublicUrl(string filePath)
        {
            return _service.GetPublicUrl(filePath);
        }
    }
}
