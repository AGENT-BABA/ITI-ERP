namespace ITI.ERP.Infrastructure.FileStorage
{
    public class FileStorageOptions
    {
        public string Provider { get; set; } = "Local";

        // Local storage
        public string LocalBasePath { get; set; } = string.Empty;
        public string LocalBaseUrl { get; set; } = string.Empty;

        // Azure Blob Storage
        public string AzureConnectionString { get; set; } = string.Empty;
        public string AzureContainerName { get; set; } = string.Empty;

        // AWS S3
        public string AwsAccessKey { get; set; } = string.Empty;
        public string AwsSecretKey { get; set; } = string.Empty;
        public string AwsRegion { get; set; } = string.Empty;
        public string AwsBucketName { get; set; } = string.Empty;

        // Cloudinary
        public string CloudinaryCloudName { get; set; } = string.Empty;
        public string CloudinaryApiKey { get; set; } = string.Empty;
        public string CloudinaryApiSecret { get; set; } = string.Empty;
    }
}
