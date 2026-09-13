using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public class DocumentService : IDocumentService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".ppt",
        ".pptx",
        ".txt",
        ".png",
        ".jpg",
        ".jpeg"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain",
        "image/png",
        "image/jpeg"
    };

    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly INotificationService _notificationService;

    public DocumentService(ApplicationDbContext context, IFileStorageService storageService, INotificationService notificationService)
    {
        _context = context;
        _storageService = storageService;
        _notificationService = notificationService;
    }

    public async Task<Document?> UploadAsync(int userId, int? projectId, string title, string? description, string category, string originalFileName, Stream fileStream, string contentType)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;
        if (string.IsNullOrWhiteSpace(category)) return null;

        var fileExtension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(fileExtension) || !AllowedExtensions.Contains(fileExtension))
        {
            return null;
        }

        if (!AllowedContentTypes.Contains(contentType))
        {
            return null;
        }

        if (fileStream.Length > 25 * 1024 * 1024)
        {
            return null;
        }

        if (projectId.HasValue)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId.Value);

            if (project == null) return null;

            var isProjectMember = project.ProjectMembers.Any(pm => pm.UserId == userId) || project.ProjectManagerId == userId;
            if (!isProjectMember)
            {
                return null;
            }
        }

        var safeFileName = $"{Guid.NewGuid():N}{fileExtension}";
        var relativePath = Path.Combine(userId.ToString(), projectId?.ToString() ?? "personal", safeFileName).Replace('\\', '/');
        var storedFilePath = await _storageService.UploadAsync(fileStream, relativePath, contentType);

        try
        {
            var document = new Document
            {
                Title = title,
                Description = description,
                Category = category,
                ProjectId = projectId,
                UploadedByUserId = userId,
                FileName = safeFileName,
                OriginalFileName = originalFileName,
                FilePath = relativePath,
                FileType = contentType,
                FileSizeBytes = fileStream.Length,
                UploadedDateUtc = DateTime.UtcNow,
                UpdatedDateUtc = DateTime.UtcNow,
                Status = DocumentStatus.Queued
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(new Notification
            {
                UserId = userId,
                Title = "Document uploaded",
                Message = $"Your document '{title}' was uploaded successfully.",
                Type = NotificationType.SystemAnnouncement,
                Priority = NotificationPriority.Informational
            });

            return document;
        }
        catch
        {
            await _storageService.DeleteAsync(storedFilePath);
            throw;
        }
    }

    public async Task<List<Document>> GetUserDocumentsAsync(int userId)
    {
        return await _context.Documents
            .Where(d => d.UploadedByUserId == userId && !d.IsDeleted)
            .OrderByDescending(d => d.UploadedDateUtc)
            .ToListAsync();
    }

    public async Task<List<Document>> GetProjectDocumentsAsync(int projectId, int requestingUserId)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);

        if (project == null) return new List<Document>();

        var isAuthorized = project.ProjectManagerId == requestingUserId || project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);
        if (!isAuthorized) return new List<Document>();

        return await _context.Documents
            .Where(d => d.ProjectId == projectId && !d.IsDeleted)
            .OrderByDescending(d => d.UploadedDateUtc)
            .ToListAsync();
    }

    public async Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

        if (document == null) return null;

        var isOwner = document.UploadedByUserId == requestingUserId;
        var isProjectManager = document.Project != null && document.Project.ProjectManagerId == requestingUserId;
        var isProjectMember = document.Project != null && document.Project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);

        if (!isOwner && !isProjectManager && !isProjectMember)
        {
            return null;
        }

        return document;
    }

    public async Task<bool> DeleteAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

        if (document == null) return false;

        var isOwner = document.UploadedByUserId == requestingUserId;
        var isProjectManager = document.Project != null && document.Project.ProjectManagerId == requestingUserId;

        if (!isOwner && !isProjectManager)
        {
            return false;
        }

        document.IsDeleted = true;
        document.UpdatedDateUtc = DateTime.UtcNow;
        await _storageService.DeleteAsync(Path.Combine(Directory.GetCurrentDirectory(), "AppData", "uploads", document.FilePath));
        await _context.SaveChangesAsync();
        return true;
    }
}
