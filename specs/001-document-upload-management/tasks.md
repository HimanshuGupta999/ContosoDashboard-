# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/document-management.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of the feature.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize the feature-specific infrastructure and shared configuration.

- [X] T001 Create the feature-specific storage and service scaffolding for document uploads in `ContosoDashboard/Services` and the application upload directory
- [ ] T002 [P] Add upload directory, local storage configuration, and queue settings in `ContosoDashboard/appsettings.json` and `ContosoDashboard/appsettings.Development.json`
- [X] T003 [P] Register the document services and file storage abstractions in `ContosoDashboard/Program.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build the underlying document domain and storage model before any story can run.

- [X] T004 Create the `Document` model in `ContosoDashboard/Models/Document.cs` with `DocumentId`, `Title`, `Description`, `Category`, `ProjectId`, `UploadedByUserId`, `FileName`, `FilePath`, `FileType`, `FileSizeBytes`, `UploadedDateUtc`, `UpdatedDateUtc`, and `IsDeleted` fields
- [X] T005 Create the `DocumentShare` model in `ContosoDashboard/Models/DocumentShare.cs` to track shared access and explicit recipient permissions
- [X] T006 Create the `ActivityLog` model in `ContosoDashboard/Models/ActivityLog.cs` to capture upload, download, share, delete, and edit operations
- [X] T007 [P] Extend `ApplicationDbContext` in `ContosoDashboard/Data/ApplicationDbContext.cs` with `DbSet<Document>`, `DbSet<DocumentShare>`, and `DbSet<ActivityLog>` plus indexes and constraints for `DocumentId`, `ProjectId`, `UploadedByUserId`, and document audit queries
- [X] T008 [P] Add the `IFileStorageService` interface and `LocalFileStorageService` implementation in `ContosoDashboard/Services/IFileStorageService.cs` and `ContosoDashboard/Services/LocalFileStorageService.cs` using the required local-file storage pattern with GUID-based filenames, `UploadAsync`, `DeleteAsync`, and `DownloadAsync`
- [X] T009 Add the `IDocumentService` interface and `DocumentService` implementation in `ContosoDashboard/Services/IDocumentService.cs` and `ContosoDashboard/Services/DocumentService.cs` with validation, authorization, and metadata persistence logic
- [ ] T010 Add the queue publisher and scan request contract in `ContosoDashboard/Services/DocumentScanQueueService.cs` to emit `ScanQueueMessage` payloads after file upload completion

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Upload and secure a document (Priority: P1) 🎯 MVP

**Goal**: Employees can safely upload a file and have it stored with proper metadata, validation, and access control.

**Independent Test**: A signed-in user can upload a supported file, see it appear in their document list, and verify that unauthorized users cannot access it.

### Implementation for User Story 1

- [X] T011 [US1] Implement upload validation, type restrictions, size enforcement, and GUID-based file naming in `ContosoDashboard/Services/DocumentService.cs` for the required `Title`, `Category`, and file validation rules
- [X] T012 [US1] Implement the file-save and metadata-persistence flow in `ContosoDashboard/Services/DocumentService.cs` so the file is saved to disk before the document record is created and no orphaned entries remain on failure
- [X] T013 [US1] Create the upload form and document list page in `ContosoDashboard/Pages/Documents.razor`, including the required title/category workflow and progress/error handling for upload outcomes
- [X] T014 [US1] Add project and user authorization checks in `ContosoDashboard/Services/DocumentService.cs` so only authorized users can upload to project-linked documents and unauthorized access is rejected before returning data
- [X] T015 [US1] Add success/error notifications and user-visible upload state handling in `ContosoDashboard/Pages/Documents.razor` and `ContosoDashboard/Services/NotificationService.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional and independently testable.

---

## Phase 4: User Story 2 - Organize, search, and manage existing documents (Priority: P2)

**Goal**: Users can search, sort, filter, and update document metadata within the dashboard.

**Independent Test**: A user can search, filter, update, and delete an uploaded document while only seeing documents they are allowed to access.

### Implementation for User Story 2

- [ ] T016 [US2] Implement document listing, sorting, and filter logic in `ContosoDashboard/Services/DocumentService.cs` for category, project, and date-based queries with access filtering
- [ ] T017 [US2] Create the project document view in `ContosoDashboard/Pages/ProjectDocuments.razor` so project members can browse all project files they are allowed to access
- [ ] T018 [US2] Create the shared-document and search workflow in `ContosoDashboard/Pages/SharedDocuments.razor` to expose shared files and searchable document results by title, description, tags, uploader, and project
- [ ] T019 [US2] Add metadata edit and file replacement logic in `ContosoDashboard/Services/DocumentService.cs` to allow owners to change `Title`, `Description`, `Category`, and tags while preserving project linkage and file safety
- [ ] T020 [US2] Add delete confirmation, cleanup, and access enforcement in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Pages/Documents.razor` to permanently remove documents after user confirmation and only permit authorized removals

**Checkpoint**: At this point, User Stories 1 and 2 should both work independently.

---

## Phase 5: User Story 3 - Share documents and surface them in everyday workflow (Priority: P3)

**Goal**: Documents are shareable, auditable, and visible in the surrounding dashboard workflows.

**Independent Test**: A document owner can share a file with another user, the recipient gets a notification, and the file appears in the relevant project or dashboard workflow without leaking unauthorized access.

### Implementation for User Story 3

- [ ] T021 [US3] Implement sharing and recipient notification logic in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Services/NotificationService.cs` for `DocumentShare` creation, in-app alerts, and recipient visibility
- [ ] T022 [US3] Add task and dashboard document integration in `ContosoDashboard/Pages/TaskDocuments.razor` and `ContosoDashboard/Pages/Index.razor` to surface recent documents and document counts in the existing workflow views
- [ ] T023 [US3] Add auditing and reporting support in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Models/ActivityLog.cs` so upload, download, share, delete, and edit actions are logged for administrators
- [ ] T024 [US3] Add the Azure Function worker in `functions/DocumentScanWorker/DocumentScanFunction.cs` with a Queue Storage trigger that reads `ScanQueueMessage` payloads and invokes the configured malware scan provider
- [ ] T025 [US3] Add scan-result handling and status updates in `ContosoDashboard/Services/DocumentService.cs` to mark files as approved, quarantined, or failed after asynchronous scanning completes

**Checkpoint**: All user stories should now be independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finalize the feature for consistent navigation, security, and validation.

- [ ] T026 [P] Validate the overall feature against the flow in `specs/001-document-upload-management/quickstart.md` and confirm upload, access control, search, share, and queue-processing behavior remains correct
- [ ] T027 [P] Update the dashboard navigation and summary entry points in `ContosoDashboard/Shared/NavMenu.razor` and any relevant dashboard cards so document features are discoverable to authorized users
- [ ] T028 [P] Update repository documentation in `README.md` and `StakeholderDocs/document-upload-and-management-feature.md` to explain the local storage pattern, file validation behavior, and Azure Function queue-processing migration path

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion and is the MVP
- **User Story 2 (Phase 4)**: Depends on Foundational completion and can proceed in parallel with User Story 1 if staffing allows
- **User Story 3 (Phase 5)**: Depends on Foundational completion and may proceed after User Story 1 or in parallel with User Story 2
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1**: No dependencies on other stories; minimum viable feature
- **US2**: Depends on the upload/storage model from US1 but should remain independently testable
- **US3**: Depends on the document access and notification model from US1/US2 but should remain independently testable

### Parallel Opportunities

- `T002` and `T003` can run in parallel after the feature start
- `T007` and `T008` can run in parallel in the foundational phase
- `T011` and `T013` can run in parallel for the upload story once foundation is complete
- `T016`, `T017`, and `T018` can run in parallel within the organization story
- `T021`, `T022`, and `T023` can run in parallel for the sharing and dashboard story
- `T026`, `T027`, and `T028` can run in parallel during the final polish phase

---

## Parallel Example: User Story 1

```bash
# Upload story tasks can proceed concurrently once the foundation is complete
Task: "Implement upload validation in ContosoDashboard/Services/DocumentService.cs"
Task: "Create the upload form in ContosoDashboard/Pages/Documents.razor"
Task: "Add success/error notification handling in ContosoDashboard/Services/NotificationService.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Stop and validate the upload journey independently
5. Expand to User Story 2 and User Story 3 as needed

### Incremental Delivery

1. Setup + Foundation → core document infrastructure ready
2. Upload and authorization → MVP story complete
3. Organization and search → productivity improvements for active users
4. Sharing and workflow integration → collaboration and auditability
5. Final polish and validation → feature is production-ready within the training constraints

---

## Notes

- Each task is intentionally scoped to one file cluster or one design decision to reduce merge conflicts and improve independence
- All task descriptions include a concrete file path so implementation can proceed with no further clarification
- The queue-based virus-scan pattern uses the Azure Function worker path described in the research and contract documents without requiring the training app to become Azure-dependent at runtime
