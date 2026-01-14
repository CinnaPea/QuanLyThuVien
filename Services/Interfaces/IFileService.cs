namespace EnglishCenterMVC.Services
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile file, string folder);
        Task<string> SaveVideoAsync(IFormFile file, string folder);
        Task<string> SaveAudioAsync(IFormFile file, string folder);
        Task<string> SaveFileAsync(IFormFile file, string folder);
        void Delete(string? publicPath);
        bool Exists(string publicPath);
    }
}
