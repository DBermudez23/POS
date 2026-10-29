using UtpPos.Framework.Application.DTO.File;
using UtpPos.Framework.InfraStructure.Interface.Storage;
using UtpPos.Framework.Application.Interface.File;
using UtpPos.Framework.Transversal.Common;
using Microsoft.AspNetCore.Http;

namespace UtpPos.Framework.Application.Main.File
{
    public sealed class FileUploadApplication : IFileUploadApplication
    {
        private readonly IFileStorageService _storageService;

        public FileUploadApplication(IFileStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<Response<FileUploadResultDto>> UploadProductImageAsync(IFormFile file)
        {
            try
            {
                var result = await _storageService.UploadProductImageAsync(file);

                return new Response<FileUploadResultDto>
                {
                    IsSuccess = true,
                    Message = "Imagen subida correctamente.",
                    Data = new FileUploadResultDto
                    {
                        Url = result.Url,
                        PublicId = result.PublicId,
                        OriginalFileName = result.OriginalFileName,
                        SizeBytes = result.SizeBytes,
                        ContentType = result.ContentType
                    }
                };
            }
            catch (Exception ex)
            {
                return new Response<FileUploadResultDto>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
    }
}