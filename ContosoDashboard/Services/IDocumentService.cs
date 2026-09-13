using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<Document?> UploadAsync(int userId, int? projectId, string title, string? description, string category, string originalFileName, Stream fileStream, string contentType);
    Task<List<Document>> GetUserDocumentsAsync(int userId);
    Task<List<Document>> GetProjectDocumentsAsync(int projectId, int requestingUserId);
    Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId);
    Task<bool> DeleteAsync(int documentId, int requestingUserId);
}
