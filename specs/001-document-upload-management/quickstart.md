# Quickstart Validation Guide

## Prerequisites

- .NET 8 SDK installed
- Repository checked out locally
- Application built and running via `dotnet run` from the `ContosoDashboard` project
- A browser with access to the local dashboard

## Validation Scenarios

### 1. Upload an approved document

1. Start the app and sign in as a valid training user such as `camille.nicole@contoso.com`.
2. Open the document management page or project document section.
3. Upload a supported file such as a PDF or Word document with a title and category selected.
4. Confirm the upload completes successfully and the document appears in the list.

Expected outcome:
- Document metadata is saved.
- The file is stored in the secure local upload directory.
- The page shows the document title, category, uploader, and upload date.

### 2. Reject unsupported or oversized files

1. Attempt to upload a file larger than 25 MB or with an unsupported extension.
2. Submit the upload.

Expected outcome:
- The request is rejected before the file is saved.
- A clear validation message explains the rejection.
- No orphaned database entries are created.

### 3. Validate queued virus scanning workflow

1. Upload an approved file that is eligible for scanning.
2. Confirm the upload is accepted and the system enqueues a scan job.
3. Review the queue message and the Azure Function worker or local mock processor handling the message.

Expected outcome:
- The upload succeeds quickly without blocking the user on a full antivirus scan.
- A queue trigger processes the file asynchronously.
- The document status is updated to show whether it is safe, quarantined, or failed the scan.

### 4. Validate default project access

1. Sign in as a project member that is part of the same project.
2. Navigate to the project documents view.
3. Confirm the uploaded file is visible and downloadable.
4. Sign in as a different user without project access and attempt to open the document.

Expected outcome:
- Authorized project members can access the file.
- Unauthorized users are denied access and see no document metadata or file content.

### 4. Validate edit, replace, and delete flows

1. Sign in as the document owner.
2. Edit the document title, description, or category.
3. Replace the uploaded file with a new version.
4. Delete the document after confirmation.

Expected outcome:
- Metadata updates persist.
- The new file version replaces the old one without corrupting the document record.
- A delete confirmation prevents accidental removal.

### 5. Validate explicit sharing and notifications

1. Upload a document.
2. Share it with another user who is not already project-authorized.
3. Sign in as the recipient.

Expected outcome:
- The recipient receives an in-app notification.
- The shared document appears in the appropriate shared documents area.
- The document remains protected from unauthorized access outside the grant.

### 6. Validate project dashboard integration

1. Add a document to a project.
2. Open the dashboard or project page.
3. Review the recent document section or summary indicator.

Expected outcome:
- Recent document activity appears in the expected dashboard area.
- Document counts and recent items are clearly visible to authorized users.

## Success Criteria for Validation

The feature is considered ready for implementation review when all of the above scenarios complete successfully and the repository confirms that the access rules remain aligned with the constitutional security principles.
