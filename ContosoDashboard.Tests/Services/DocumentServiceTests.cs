using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContosoDashboard.Tests.Services;

public class DocumentServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Users.AddRange(
            new User { UserId = 1, Email = "admin@contoso.com", DisplayName = "Admin", Department = "IT", JobTitle = "Administrator", Role = UserRole.Administrator },
            new User { UserId = 2, Email = "pm@contoso.com", DisplayName = "Project Manager", Department = "Engineering", JobTitle = "Project Manager", Role = UserRole.ProjectManager },
            new User { UserId = 3, Email = "employee@contoso.com", DisplayName = "Employee", Department = "Engineering", JobTitle = "Developer", Role = UserRole.Employee }
        );

        context.Projects.Add(new Project
        {
            ProjectId = 1,
            Name = "Project One",
            Description = "Test project",
            ProjectManagerId = 2,
            Status = ProjectStatus.Active,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        });

        context.ProjectMembers.Add(new ProjectMember
        {
            ProjectMemberId = 1,
            ProjectId = 1,
            UserId = 3,
            Role = "Developer",
            AssignedDate = DateTime.UtcNow
        });

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task UploadDocument_ShouldAcceptValidPdfAndPersistMetadata()
    {
        await using var context = CreateContext();
        var storage = new InMemoryFileStorageService();
        var service = new DocumentService(context, storage, new NotificationService(context));

        var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var result = await service.UploadAsync(
            3,
            1,
            "Quarterly Review",
            "Project report",
            "Reports",
            "quarterly-review.pdf",
            stream,
            "application/pdf");

        Assert.NotNull(result);
        Assert.Equal("Quarterly Review", result.Title);
        Assert.Equal("Reports", result.Category);
        Assert.Equal("quarterly-review.pdf", result.OriginalFileName);
        Assert.Equal(1, context.Documents.Count());
    }

    [Fact]
    public async Task UploadDocument_ShouldRejectUnsupportedFileType()
    {
        await using var context = CreateContext();
        var storage = new InMemoryFileStorageService();
        var service = new DocumentService(context, storage, new NotificationService(context));

        var result = await service.UploadAsync(
            3,
            1,
            "Rejected file",
            "Bad format",
            "Reports",
            "reject.exe",
            new MemoryStream(new byte[] { 1, 2, 3 }),
            "application/x-msdownload");

        Assert.Null(result);
    }

    [Fact]
    public async Task UploadDocument_ShouldRejectUnauthorizedProjectUser()
    {
        await using var context = CreateContext();
        var storage = new InMemoryFileStorageService();
        var service = new DocumentService(context, storage, new NotificationService(context));

        var result = await service.UploadAsync(
            1,
            1,
            "Unauthorized file",
            "Should not be stored",
            "Reports",
            "unauthorized.pdf",
            new MemoryStream(new byte[] { 1, 2, 3 }),
            "application/pdf");

        Assert.Null(result);
        Assert.Empty(context.Documents);
    }

    [Fact]
    public async Task GetProjectDocumentsAsync_ShouldAllowProjectMembersToReadProjectDocs()
    {
        await using var context = CreateContext();
        var storage = new InMemoryFileStorageService();
        var service = new DocumentService(context, storage, new NotificationService(context));

        await service.UploadAsync(
            3,
            1,
            "Quarterly Review",
            "Project report",
            "Reports",
            "quarterly-review.pdf",
            new MemoryStream(new byte[] { 1, 2, 3 }),
            "application/pdf");

        var documents = await service.GetProjectDocumentsAsync(1, 3);

        Assert.Single(documents);
    }
}

public class InMemoryFileStorageService : IFileStorageService
{
    public Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        return Task.FromResult($"/tmp/{fileName}");
    }

    public Task DeleteAsync(string filePath)
    {
        return Task.CompletedTask;
    }

    public Task<Stream> DownloadAsync(string filePath)
    {
        return Task.FromResult<Stream>(new MemoryStream(new byte[] { 9, 9, 9 }));
    }

    public Task<string> GetUrlAsync(string filePath, TimeSpan expiration)
    {
        return Task.FromResult($"/download/{filePath}");
    }
}
