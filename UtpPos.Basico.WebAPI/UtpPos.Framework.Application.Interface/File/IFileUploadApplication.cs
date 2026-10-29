using UtpPos.Framework.Application.DTO.File;
using UtpPos.Framework.Transversal.Common;
using Microsoft.AspNetCore.Http;

namespace UtpPos.Framework.Application.Interface.File
{
    public interface IFileUploadApplication
    {
        Task<Response<FileUploadResultDto>> UploadProductImageAsync(IFormFile file);
    }
}