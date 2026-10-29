namespace UtpPos.Framework.Shared.File
{
    public sealed class FileUploadResultDto
    {
        public string Url { get; set; } = string.Empty;
        public string? PublicId { get; set; }
        public string? OriginalFileName { get; set; }
        public long SizeBytes { get; set; }
        public string? ContentType { get; set; }
    }
}