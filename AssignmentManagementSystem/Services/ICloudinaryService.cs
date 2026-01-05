namespace AssignmentManagementSystem.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<bool> DeleteFileAsync(string publicId);
    }
}
