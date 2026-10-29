using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using UtpPos.Framework.InfraStructure.Interface.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace UtpPos.Framework.InfraStructure.Repository.Storage
{
    public sealed class CloudinaryFileStorageService : IFileStorageService
    {
        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private const long MaxFileSizeBytes = 3 * 1024 * 1024;

        private readonly Cloudinary _cloudinary;
        private readonly CloudinaryOptions _options;

        public CloudinaryFileStorageService(IOptions<CloudinaryOptions> options)
        {
            _options = options.Value;

            var account = new Account(
                _options.CloudName,
                _options.ApiKey,
                _options.ApiSecret);

            _cloudinary = new Cloudinary(account)
            {
                Api =
                {
                    Secure = true
                }
            };
        }

        public async Task<FileStorageUploadResult> UploadProductImageAsync(IFormFile file)
        {
            ValidateImage(file);

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = _options.ProductFolder,
                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false,
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error is not null)
            {
                throw new InvalidOperationException(uploadResult.Error.Message);
            }

            return new FileStorageUploadResult
            {
                Url = uploadResult.SecureUrl?.ToString() ?? string.Empty,
                PublicId = uploadResult.PublicId,
                OriginalFileName = file.FileName,
                SizeBytes = file.Length,
                ContentType = file.ContentType
            };
        }

        private static void ValidateImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("La imagen es obligatoria.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("La imagen no puede superar 3 MB.");

            if (!AllowedContentTypes.Contains(file.ContentType))
                throw new InvalidOperationException("Solo se permiten imágenes JPG, PNG o WEBP.");
        }
    }
}
