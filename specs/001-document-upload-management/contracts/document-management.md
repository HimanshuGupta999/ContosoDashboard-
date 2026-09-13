# Document Management Contract

## Scope

This contract defines the expected object and authorization model for the document feature in the current Blazor Server application. The implementation is primarily page- and service-driven rather than a standalone external API, but the same contract should be honored if a future controller or API layer is introduced.

## Shared Types

### DocumentUploadRequest

```json
{
  "title": "Quarterly Review",
  "description": "Review deck for Q3 planning",
  "category": "Reports",
  "projectId": 1,
  "tags": ["review", "finance"],
  "file": "binary-data"
}
```

Validation rules:
- `title` is required and must be non-empty.
- `category` must match one of the supported category values.
- `projectId` is optional and is only valid when the user has project access.
- `file` must be a supported document type and must not exceed 25 MB.
- `tags` is optional and stores user-defined search labels.

### DocumentSummary

```json
{
  "documentId": 101,
  "title": "Quarterly Review",
  "category": "Reports",
  "uploadedByUserId": 2,
  "projectId": 1,
  "uploadedDateUtc": "2026-09-13T08:00:00Z",
  "fileSizeBytes": 245760,
  "fileType": "application/pdf"
}
```

### ShareRequest

```json
{
  "documentId": 101,
  "sharedWithUserId": 4,
  "message": "Please review the updated report"
}
```

Rules:
- Only the document owner, a project manager, or an administrator can initiate a share.
- The recipient must be a valid user in the application.
- In-app notifications are emitted after a successful share.

### ScanQueueMessage

```json
{
  "documentId": 101,
  "filePath": "userId/projectId/9f8a6d2f-1a1a-4aa3-9b22-8f3947af7d13.pdf",
  "uploadedByUserId": 2,
  "scanRequestedUtc": "2026-09-13T08:00:00Z"
}
```

Rules:
- This message is emitted after the upload is persisted and before the file is considered fully approved for shared access.
- An Azure Function with a Queue Storage trigger reads the message and invokes the malware or virus scan provider.
- The function updates the document status to reflect the scan outcome; quarantined or failed scans must remain inaccessible until a security review occurs.

## Authorization Contract

The following service contract applies to all document reads, edits, and deletes:

- Document read access is granted to:
  - the uploader
  - project members with access to the associated project
  - users explicitly shared with the document
  - administrators
- Document edit access is granted to:
  - the upload owner
  - project managers for project documents
  - administrators
- Document delete access is granted to:
  - the upload owner
  - project managers for project documents
  - administrators

## Error Contract

```json
{
  "errorCode": "UNSUPPORTED_FILE_TYPE",
  "message": "The selected file type is not allowed. Supported types include PDF, Office documents, text files, and images."
}
```

Common codes:
- `FILE_TOO_LARGE`
- `UNSUPPORTED_FILE_TYPE`
- `UPLOAD_FAILED`
- `NOT_AUTHORIZED`
- `PROJECT_ACCESS_REQUIRED`
- `DOCUMENT_NOT_FOUND`

## Storage Contract

- Files are stored outside `wwwroot` in an application-managed directory.
- File paths are generated server-side and must never be based on user-provided filenames.
- The file system implementation is pluggable through `IFileStorageService`.
- The database stores the file path and metadata; the file body is not stored directly in the Entity Framework model.
