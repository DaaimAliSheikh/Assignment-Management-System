using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace AssignmentManagementSystem.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            var cloudinarySettings = configuration.GetSection("Cloudinary");
            var account = new Account(
                cloudinarySettings["CloudName"],
                cloudinarySettings["ApiKey"],
                cloudinarySettings["ApiSecret"]
            );
            
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File size cannot exceed 10MB");

            // Validate file type (allow common document and image formats)
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".zip" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException($"File type {extension} is not allowed");

            try
            {
                using var stream = file.OpenReadStream();
                
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "assignment-submissions",
                    PublicId = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName)}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError($"Cloudinary upload error: {uploadResult.Error.Message}");
                    throw new Exception($"File upload failed: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file to Cloudinary: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Raw
                };

                var result = await _cloudinary.DestroyAsync(deleteParams);
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file from Cloudinary: {ex.Message}");
                return false;
            }
        }
    }
}
