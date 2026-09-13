namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService()
    {
        _rootPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData", "uploads");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var targetDirectory = Path.Combine(_rootPath, Path.GetDirectoryName(fileName) ?? string.Empty);
        Directory.CreateDirectory(targetDirectory);

        var fullPath = Path.Combine(_rootPath, fileName);
        await using var outputStream = File.Create(fullPath);
        await fileStream.CopyToAsync(outputStream);

        return fullPath;
    }

    public Task DeleteAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public async Task<Stream> DownloadAsync(string filePath)
    {
        return await Task.FromResult<Stream>(File.OpenRead(filePath));
    }

    public Task<string> GetUrlAsync(string filePath, TimeSpan expiration)
    {
        return Task.FromResult(filePath);
    }
}
