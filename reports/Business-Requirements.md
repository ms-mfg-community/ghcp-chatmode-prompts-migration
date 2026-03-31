# Business Requirements — AppModernization.Web

> **Generated**: 2026-03-30  
> **Phase**: 0 — Discovery  
> **Source**: Extracted from codebase analysis  

---

## Domain: Project Management

### BR-PM-01: Create Migration Project
- **Description**: Users can create a new migration project by providing a project name and selecting configuration options (application type, target framework, hosting platform, IaC type, database type).
- **Business Rule**: Project names default to "New Migration Project". A unique GUID is assigned as the project ID.
- **User Action**: Fill out the Home page form and submit.
- **System Response**: Creates `MigrationProject` with 8 phases (3 Phase 0 sub-phases + Phases 1–6), navigates to Dashboard.
- **Source**: `Models/MigrationProject.cs`, `Services/MigrationStateService.cs`
- **Confidence**: HIGH

### BR-PM-02: Phase 0 Opt-In
- **Description**: Users can choose to include or skip Phase 0 (Discovery) when creating a project.
- **Business Rule**: `UsePhase0` defaults to `true`. When false, Phase 0 sub-phases are created but can be skipped.
- **Source**: `Models/MigrationProject.cs` (`UsePhase0` property), `Components/Home.razor`
- **Confidence**: HIGH

### BR-PM-03: Save and Resume Projects
- **Description**: Projects are automatically persisted to disk and can be resumed from the Projects page.
- **Business Rule**: Projects are saved as `{projectId}.json` in `~/.appmod/projects/`. The Projects page lists all saved projects with their name, progress percentage, creation date, and current phase.
- **Source**: `Services/ProjectPersistenceService.cs`, `Components/Projects.razor`
- **Confidence**: HIGH

### BR-PM-04: Delete Project
- **Description**: Users can delete a saved project from the Projects page.
- **Business Rule**: Deletes the `{projectId}.json` file from disk. No confirmation dialog documented in service layer (may exist in UI).
- **Source**: `Services/ProjectPersistenceService.cs` (`DeleteProjectAsync`)
- **Confidence**: HIGH

### BR-PM-05: Progress Tracking
- **Description**: Overall project progress is calculated as a percentage.
- **Business Rule**: Progress = (Completed + Skipped phases) / Total phases × 100.
- **Source**: `Services/MigrationStateService.cs` (`GetProgress`)
- **Confidence**: HIGH

---

## Domain: Phase Workflow

### BR-PW-01: Phase Lifecycle
- **Description**: Each phase transitions through a defined lifecycle.
- **Business Rule**: Status transitions: `NotStarted` → `InProgress` → `Completed` or `Skipped`. Timestamps are recorded for `StartedAt` and `CompletedAt`.
- **Source**: `Models/PhaseInfo.cs` (`PhaseStatus` enum)
- **Confidence**: HIGH

### BR-PW-02: Phase Ordering
- **Description**: Phases follow a sequential order from Phase 0 through Phase 6.
- **Business Rule**: Phase 0 has 3 sub-phases (Discover → Specify → Validate). Phases 1–6 are linear (Plan → Assess → Migrate → Infra → Deploy → CI/CD). The Dashboard and NavMenu display phases in order.
- **Source**: `Services/MigrationStateService.cs` (`InitializeProject`)
- **Confidence**: HIGH

### BR-PW-03: Phase Navigation
- **Description**: Users navigate between phases via the Dashboard cards or sidebar navigation.
- **Business Rule**: Each phase has a dedicated route (e.g., `/phase0/discover`, `/phase1/plan`). The NavMenu groups Phase 0 sub-phases and Phases 1–6 separately.
- **Source**: `Components/NavMenu.razor`, `Components/Dashboard.razor`
- **Confidence**: HIGH

### BR-PW-04: Current Phase Detection
- **Description**: The system identifies the "current" phase for the user.
- **Business Rule**: Current phase = first phase with status `InProgress`, or first phase with status `NotStarted` if none are in progress.
- **Source**: `Services/MigrationStateService.cs` (`GetCurrentPhase`)
- **Confidence**: HIGH

---

## Domain: AI Chat Integration

### BR-AI-01: Copilot Session Per Phase
- **Description**: Each phase gets its own Copilot chat session with a phase-specific system prompt.
- **Business Rule**: When a phase page loads, it creates a Copilot session using the agent prompt from `.github/agents/{agentFile}`. The agent prompt markdown (with YAML frontmatter stripped) becomes the system message.
- **Source**: `Services/CopilotService.cs` (`CreateSessionForPhaseAsync`), `Services/AgentPromptService.cs`
- **Confidence**: HIGH

### BR-AI-02: Message Streaming
- **Description**: Copilot responses are streamed in real-time to the chat panel.
- **Business Rule**: Messages arrive as deltas via `OnMessageReceived` events. The `ChatPanel` accumulates deltas into complete messages. A streaming cursor (▌) is displayed during active streaming.
- **Source**: `Services/CopilotService.cs`, `Components/ChatPanel.razor`, `Components/ChatMessageBubble.razor`
- **Confidence**: HIGH

### BR-AI-03: Markdown Rendering
- **Description**: Assistant messages are rendered as formatted HTML from markdown.
- **Business Rule**: The Markdig library converts markdown content to HTML for display in chat bubbles. User messages are displayed as plain text.
- **Source**: `Components/ChatMessageBubble.razor`
- **Confidence**: HIGH

### BR-AI-04: Chat Error Handling
- **Description**: Chat errors are displayed with a retry option.
- **Business Rule**: If message sending fails, an error message is shown in the chat panel with a retry button. Errors are not persisted.
- **Source**: `Components/ChatPanel.razor`
- **Confidence**: MEDIUM (inferred from component structure)

---

## Domain: Authentication & Authorization

### BR-AUTH-01: GitHub OAuth Login
- **Description**: Users can authenticate via GitHub OAuth.
- **Business Rule**: Requires `GitHub:ClientId` and `GitHub:ClientSecret` configuration. When configured, a "Sign in with GitHub" button appears. The OAuth flow uses the `GitHub` challenge scheme and stores identity in a cookie named `AppMod.Auth`.
- **Source**: `Program.cs` (authentication setup), `Components/AuthBar.razor`
- **Confidence**: HIGH

### BR-AUTH-02: PAT Login
- **Description**: Users can authenticate with a GitHub Personal Access Token.
- **Business Rule**: POST to `/auth/pat-login` validates the token against the GitHub API (`api.github.com/user`). On success, creates cookie claims from the GitHub user profile (name, login, avatar).
- **Source**: `Program.cs` (`/auth/pat-login` endpoint)
- **Confidence**: HIGH

### BR-AUTH-03: Graceful Auth Degradation
- **Description**: The app works without OAuth configuration.
- **Business Rule**: If `GitHub:ClientId` is empty/missing, OAuth login is silently disabled. Users can still use PAT authentication. The login page does not show the OAuth button.
- **Source**: `Program.cs` (conditional OAuth registration)
- **Confidence**: HIGH

### BR-AUTH-04: Logout
- **Description**: Users can sign out.
- **Business Rule**: GET `/auth/logout` signs out the user and redirects to `/` (home page).
- **Source**: `Program.cs` (`/auth/logout` endpoint)
- **Confidence**: HIGH

---

## Domain: Persistence & Data

### BR-DATA-01: JSON File Storage
- **Description**: Project data is persisted as individual JSON files.
- **Business Rule**: Storage location is `{UserProfile}/.appmod/projects/`. Each project is one file: `{projectId}.json`. Directory is auto-created if missing.
- **Source**: `Services/ProjectPersistenceService.cs`
- **Confidence**: HIGH

### BR-DATA-02: Project Listing
- **Description**: The system can list all saved projects with summary information.
- **Business Rule**: Returns `ProjectSummary` objects containing: id, name, progress percentage, creation date, and current phase name.
- **Source**: `Services/ProjectPersistenceService.cs` (`ListProjectsAsync`)
- **Confidence**: HIGH

---

## Domain: Deployment & Operations

### BR-OPS-01: Health Check
- **Description**: A health check endpoint is available for monitoring.
- **Business Rule**: GET `/health` returns JSON with status ("healthy"), timestamp, and assembly version.
- **Source**: `HealthCheckEndpoints.cs`
- **Confidence**: HIGH

### BR-OPS-02: Azure Deployment
- **Description**: The app is designed for Azure App Service deployment.
- **Business Rule**: Bicep templates provision App Service Plan, Web App (.NET 10 runtime), and Application Insights. Deployed via `azd deploy`.
- **Source**: `azure.yaml`, `infra/main.bicep`, `infra/modules/appService.bicep`
- **Confidence**: HIGH

---

## Data Entities

| Entity | Key Fields | Storage |
|--------|-----------|---------|
| **MigrationProject** | Id (GUID), Name, SourcePath, HostingPlatform, IaCType, DatabaseType, TargetFramework, ApplicationType, UsePhase0, CreatedAt, Phases[] | JSON file |
| **PhaseInfo** | Id, Name, Description, AgentFile, Route, Status, StartedAt, CompletedAt, PhaseNumber, SubPhase | Embedded in MigrationProject |
| **ChatMessage** | Id (GUID), Role, Content, Timestamp, IsStreaming | In-memory only |
| **ProjectSummary** | Id, Name, Progress, CreatedAt, CurrentPhase | Derived at runtime |
