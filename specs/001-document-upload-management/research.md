# Research: Document Upload and Management

## Decision: Use SQLite metadata + local filesystem storage + interface-based storage abstraction

The application will store document metadata in SQLite through `ApplicationDbContext` and keep uploaded files in a secure application-managed directory outside of `wwwroot`, such as `AppData/uploads`. The file storage layer will be abstracted behind `IFileStorageService`, with a `LocalFileStorageService` implementation for the training environment and a future `AzureBlobStorageService` path for migration.

## Decision: Use an asynchronous malware scan via Azure Functions and Queue Storage

The feature will enqueue a document-scan job immediately after a file upload succeeds. An Azure Function triggered by Azure Queue Storage will process the queue message asynchronously, invoke the configured antivirus or malware scan service, and update the document status to `Approved`, `Quarantined`, or `ScanFailed` based on the result. This preserves fast uploads while aligning with the requirement to verify uploaded files before storage is considered final.

### Rationale

- It meets the requirement to scan files for malware without blocking the user experience during upload.
- It fits the repository’s migration-aware architecture and is compatible with a future cloud deployment.
- It keeps the Blazor server app focused on orchestration and authorization while leaving the scanning workload to a decoupled worker.

### Alternatives considered

- Synchronous virus scan in the web request: rejected because it makes upload latency worse and increases the risk of long-running requests.
- Local-only scan process in-process: rejected because it is harder to scale and less aligned with the cloud migration design.
- No scanning step: rejected because it violates the stakeholder requirement and the security objectives.

### Rationale

- This directly matches the repository’s offline-first training design and keeps the feature runnable in isolated environments.
- It preserves the project’s current architecture pattern of interface-based service abstractions and avoids cloud dependencies.
- It satisfies the security requirement to prevent direct web exposure and path traversal risks while remaining simple to teach.
- It allows a future cloud migration without changing the business flow or most UI code.

### Alternatives considered

- Store files directly in `wwwroot`: rejected because it exposes them as static web assets and creates direct access and authorization problems.
- Store file bytes directly in the SQLite database: rejected because it is less aligned with the project’s architecture guidance and less portable for future storage abstraction.
- Use Azure Blob Storage immediately: rejected because the feature must work offline and the training environment is intentionally self-contained.

## Decision: Default project access model

Project members can view and download project documents by default once they have project access. Only the document owner, project managers, or administrators can edit or delete project documents. Additional explicit sharing remains available for special-case access.

### Rationale

- This matches the clarified business rule and reduces friction for project collaboration.
- It preserves a clear distinction between read access and write management permissions.
- It aligns with the IDOR protection and service-layer authorization patterns already used in the app.

### Alternatives considered

- Only the uploader can access an uploaded document by default: rejected because it does not support project collaboration and is weaker than the project-driven workflow described by the stakeholders.
- Allow all employees to view all documents by default: rejected because it weakens access boundaries and conflicts with project-only and personal document expectations.

## Decision: Use integer document IDs and string category values

The data model will use integer `DocumentId` values and store the category as a string value such as "Project Documents" or "Personal Files". This matches the project’s existing database conventions and keeps the feature simple.

### Rationale

- The spec explicitly requires integer keys for consistency with the current user/project model.
- String categories are simpler to implement and easier to display in UI and reporting than enum-backed database values.

## Decision: Add a lightweight audit and sharing model

The feature will introduce a `DocumentShare` entity for direct sharing and an `ActivityLog` entity for document audit events. This is sufficient without adding a large event framework.

### Rationale

- Sharing is a key requirement and benefits from first-class tracking.
- Audit activity is required for administrators and aligns with the project’s security and reporting needs.
- The design remains compact and fits the repository’s modest implementation size.

### Alternatives considered

- Logging only to application files: rejected because it is less searchable and weaker for in-app reporting.
- Building a complex event bus: rejected as unnecessary for the training scope and project size.

## Decision: Validation strategy and testing approach

The feature will be validated with focused tests for security checks, validation rules, and shared project access. The repo will add a minimal .NET test project if needed for the feature, using xUnit and ASP.NET Core test infrastructure.

### Rationale

- The repo currently emphasizes service-layer authorization and security behavior; these are the highest-value tests for this feature.
- The app is a training sample, so targeted tests are more valuable than broad automated coverage.

### Alternatives considered

- No automated testing: rejected because the feature includes security-sensitive access and file validation logic.
- Full end-to-end UI testing only: rejected because service and contract tests are cheaper and more direct for the feature’s access rules.

## Open items resolved by the plan

- File storage model: local filesystem with service abstraction
- Default access model: project-member read access by default; owner/PM/admin write access
- Database types: integer `DocumentId`; string category values
- Security model: service authorization + page-level checks + file path isolation
- Reporting model: `ActivityLog` plus `DocumentShare` relation
