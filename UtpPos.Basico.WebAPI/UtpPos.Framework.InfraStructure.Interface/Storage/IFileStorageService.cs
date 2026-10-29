using Microsoft.AspNetCore.Http;

namespace UtpPos.Framework.InfraStructure.Interface.Storage
{
    public interface IFileStorageService
    {
        Task<FileStorageUploadResult> UploadProductImageAsync(IFormFile file);
    }

    public sealed class FileStorageUploadResult
    {
        public string Url { get; set; } = string.Empty;
        public string? PublicId { get; set; }
        public string? OriginalFileName { get; set; }
        public long SizeBytes { get; set; }
        public string? ContentType { get; set; }
    }
}