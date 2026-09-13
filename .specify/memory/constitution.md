<!--
Sync Impact Report
- Version change: 0.0.0 -> 1.0.0
- Modified principles: N/A (template instantiated into project-specific constitution)
- Added sections: Core Principles, Security Boundaries and Training Constraints, Development Workflow and Review Expectations, Governance
- Removed sections: None
- Deferred items: TODO(RATIFICATION_DATE): original adoption date not yet recorded.
-->

# ContosoDashboard Constitution

## Core Principles

### I. Training-First Scope
ContosoDashboard MUST remain a training-oriented sample application and MUST NOT be presented as a production system. All features, defaults, and documentation MUST clearly describe their educational purpose, known limitations, and any security caveats. The project MUST not imply that the mock identity, local database, or simplified authorization model is production-ready.

Rationale: This keeps the repository safe for classroom use and ensures learners understand which safeguards are intentionally omitted for simplicity, offline development, and demonstration value.

### II. Offline-First and Migration-Aware Architecture
The application MUST prefer local, offline-first implementations for persistence, authentication, and file handling when the learning objective is to demonstrate architecture patterns. Any cloud dependency or infrastructure abstraction MUST be isolated behind explicit interfaces and documented as a migration path rather than a current implementation requirement.

Rationale: The project is designed for low-friction training in isolated environments, and the architecture must teach separation of concerns without requiring external services.

### III. Evidence-Based Delivery
All changes MUST be traceable to a defined user need, requirement, or governance rule. The project MUST validate behavior with the smallest relevant checks, and defects MUST be fixed at the root cause rather than masked by workaround code or undocumented assumptions.

Rationale: Reliable training material requires deliberate, explainable changes and a clear audit trail from requirement to implementation to verification.

### IV. User-Scope and Access Integrity
The application MUST enforce user isolation, authorization checks, and object-level access validation for all project, task, notification, profile, and team data. Protected pages and services MUST reject unauthorized access, and any user-scoped data MUST never be exposed across identities or roles.

Rationale: Security demonstrations are central to the project, and access violations are not acceptable even in a simplified training context.

### V. Simplicity, Clarity, and Maintainability
The codebase MUST favor explicit, readable implementations over clever or hidden behavior. New abstractions MUST have a clear purpose, and changes MUST preserve the training-friendly structure of Models, Services, Data, and Pages.

Rationale: Educational software is most valuable when its intent is obvious, its structure is easy to follow, and its decisions can be discussed during a lab or review.

## Security Boundaries and Training Constraints
The repository MUST remain explicitly non-production. The mock authentication system, local database, and simplified authorization model are acceptable only for training scenarios. Any production-grade implementation MUST be documented as out of scope and MUST NOT be treated as recommended deployment guidance.

The project MUST maintain the following constraints:

- Authentication and authorization examples MUST remain clearly labeled as mock or training-only.
- No external service dependency MUST be required for the default developer experience.
- Sensitive configuration such as secrets, tokens, or production credentials MUST not be committed or implied as default runtime values.
- Security features that are intentionally simplified MUST be called out in code and documentation to prevent confusion.
- User data and access checks MUST be treated as privacy-sensitive even in a sample application.

Rationale: This project teaches secure design patterns without creating a misleading production baseline.

## Development Workflow and Review Expectations
All work on ContosoDashboard MUST align with the repository’s training objectives and the principles above. For any change affecting authorization, data access, user identity, or file handling, the relevant review must confirm that the behavior is appropriate for a training environment and does not introduce misleading production assumptions.

The following expectations apply:

- Requirements or design intent MUST be documented before implementation when a change introduces behavior or contracts.
- Code review MUST confirm clarity, scope, and adherence to the architecture boundaries.
- Validation MUST include the smallest relevant check that demonstrates the changed behavior.
- Security-sensitive changes MUST be reviewed for user isolation, authorization, and data exposure risks.
- Documentation MUST be updated when behavior, architecture, or limitations change.

Rationale: Good governance in a teaching project depends on clear intent, visible review, and reproducible evidence.

## Governance
This Constitution supersedes informal practice for decisions that affect scope, architecture, security, and project quality. Any exception must be documented, reviewed, and justified against the governing principles above.

Amendment Procedure:
- Proposed updates MUST be described in writing with the reason for the change and the intended effect.
- Changes MUST be reviewed for consistency with the project’s training purpose, security boundaries, and architectural constraints.
- Each amendment MUST update the version number and the last amended date.

Versioning Policy:
- MAJOR changes remove, redefine, or invalidate a core principle or governance rule in a backward-incompatible way.
- MINOR changes add a new principle or materially expand guidance without breaking the existing constitution.
- PATCH changes clarify wording, correct errors, or refine non-semantic guidance.

Compliance Review Expectations:
- All pull requests or material changes MUST be checked for conformance with the constitution.
- Reviewers MUST verify that changes do not quietly turn a training demo into a production claim.
- Any decision that broadens scope, adds external dependencies, or weakens security boundaries MUST be explicitly justified.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date not yet recorded | **Last Amended**: 2026-09-13
