# Data Model: Document Upload and Management

## Overview

The document feature adds a document-focused data layer to the current dashboard architecture while preserving the repository’s patterns for integer keys, role-based security, and service-driven access control.

## Core Entities

### Document

Represents an uploaded file and its metadata.

- DocumentId: integer, primary key
- Title: required string, human-readable title
- Description: optional string, up to 2000 characters
- Category: required string, one of the allowed values such as Project Documents, Team Resources, Personal Files, Reports, Presentations, Other
- ProjectId: optional integer, linked to a project when applicable
- UploadedByUserId: required integer, references a user
- FileName: required string, stored name used for file system safety
- OriginalFileName: optional string, display name or original upload name
- FilePath: required string, relative or app-managed path to the stored file
- FileType: required string, MIME type or normalized content type; supports up to 255 characters
- FileSizeBytes: required integer
- UploadedDateUtc: required DateTime
- UpdatedDateUtc: required DateTime
- IsDeleted: boolean, default false

Relationships:
- Many-to-one with `User` (uploader)
- Many-to-one with `Project` (optional association)
- One-to-many with `DocumentShare`
- One-to-many with `ActivityLog`

Validation rules:
- Title is required and non-empty.
- Category must be one of the feature’s defined categories.
- File size must be <= 25 MB.
- File type must be in the approved whitelist.
- FilePath must be generated before database insertion; it must not be user-controlled.

### DocumentShare

Represents explicit sharing rules for a document beyond project default access.

- DocumentShareId: integer, primary key
- DocumentId: required integer
- SharedWithUserId: required integer, recipient user
- SharedByUserId: required integer, owner or manager who initiated the share
- SharedDateUtc: required DateTime
- Message: optional string
- IsActive: boolean, default true

Relationships:
- Many-to-one with `Document`
- Many-to-one with `User` (recipient)
- Many-to-one with `User` (sharer)

### ActivityLog

Captures audit activity for uploads, downloads, deletes, edits, and sharing.

- ActivityLogId: integer, primary key
- DocumentId: optional integer
- UserId: required integer, actor
- ActionType: required string, e.g. Upload, Download, Delete, UpdateMetadata, Share
- Details: optional string, human-readable summary
- OccurredUtc: required DateTime

Relationships:
- Many-to-one with `Document` if the action relates to a specific file
- Many-to-one with `User` (actor)

## Existing Entity Relationships

The new document entities will integrate with the current application model:

- `User` may upload many documents and receive many notifications.
- `Project` may have many documents associated with it.
- `ProjectMember` continues to determine project participation, which helps drive default access checks.
- `Notification` remains the in-app user alert model for document-related events.

## State and Access Model

Document lifecycle status is small and mostly operational rather than complex stateful:

- Draft/queued during upload validation
- Stored after the file is saved successfully and metadata is committed
- Shared when `DocumentShare` entries are created
- Archived or soft-deleted when the document is removed from the active list

Access rules:
- Project members can view/download project documents by default.
- Only the owner, a project manager, or an administrator can edit or delete a document unless there is an explicit override in the sharing model.
- Shared documents are visible to recipients and can trigger in-app notifications.
- Unauthorized access attempts are denied at the service layer before the file or metadata is returned.

## Notes for Implementation

- Use integer keys to match the current user and project model.
- Use string category values instead of integer enums for simplicity and UI readability.
- Generate a GUID-based file name before saving to disk to avoid path collisions and path traversal issues.
- Store file paths in a relative application-managed form such as `userId/projectId/{guid}.{ext}`.
