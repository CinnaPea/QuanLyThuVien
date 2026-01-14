using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EnglishCenterMVC.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        private static readonly string[] ImageExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private static readonly string[] VideoExtensions =
        {
            ".mp4", ".webm", ".mov"
        };

        private static readonly string[] AudioExtensions =
        {
            ".mp3", ".wav", ".ogg"
        };

        private static readonly string[] DocumentExtensions =
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".zip", ".rar"
        };

        private const long MaxImageSize = 5 * 1024 * 1024;     // 5MB
        private const long MaxAudioSize = 20 * 1024 * 1024;   // 20MB
        private const long MaxVideoSize = 200 * 1024 * 1024;  // 200MB
        private const long MaxFileSize = 50 * 1024 * 1024;   // 50MB

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        /* ================= IMAGE ================= */
        public Task<string> SaveImageAsync(IFormFile file, string folder)
            => SaveAsync(file, folder, ImageExtensions, MaxImageSize);

        /* ================= VIDEO ================= */
        public Task<string> SaveVideoAsync(IFormFile file, string folder)
            => SaveAsync(file, folder, VideoExtensions, MaxVideoSize);

        /* ================= AUDIO ================= */
        public Task<string> SaveAudioAsync(IFormFile file, string folder)
            => SaveAsync(file, folder, AudioExtensions, MaxAudioSize);

        /* ================= FILE ================= */
        public Task<string> SaveFileAsync(IFormFile file, string folder)
            => SaveAsync(file, folder, DocumentExtensions, MaxFileSize);

        /* ================= CORE ================= */
        private async Task<string> SaveAsync(
            IFormFile file,
            string folder,
            string[] allowedExtensions,
            long maxSize)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File rỗng");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException($"Không hỗ trợ định dạng {extension}");

            if (file.Length > maxSize)
                throw new ArgumentException($"File vượt quá {maxSize / 1024 / 1024}MB");

            var uploadFolder = Path.Combine(
                _env.WebRootPath,
                "uploads",
                folder
            );

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var physicalPath = Path.Combine(uploadFolder, fileName);

            using var stream = new FileStream(physicalPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }

        /* ================= DELETE ================= */
        public void Delete(string? publicPath)
        {
            if (string.IsNullOrWhiteSpace(publicPath))
                return;

            var physicalPath = MapToPhysicalPath(publicPath);

            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }

        public bool Exists(string publicPath)
        {
            var physicalPath = MapToPhysicalPath(publicPath);
            return File.Exists(physicalPath);
        }

        private string MapToPhysicalPath(string publicPath)
        {
            var relativePath = publicPath
                .TrimStart('/')
                .Replace("/", Path.DirectorySeparatorChar.ToString());

            return Path.Combine(_env.WebRootPath, relativePath);
        }
    }
}
