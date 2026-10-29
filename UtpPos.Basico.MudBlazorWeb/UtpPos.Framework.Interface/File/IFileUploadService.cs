using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.File;
using Microsoft.AspNetCore.Components.Forms;

namespace UtpPos.Framework.Interface.File
{
    public interface IFileUploadService
    {
        Task<Response<FileUploadResultDto>> UploadProductImageAsync(IBrowserFile file);
    }
}