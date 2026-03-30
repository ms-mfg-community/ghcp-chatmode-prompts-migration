# Process Maps — AppModernization.Web

> **Generated**: 2026-03-30  
> **Phase**: 0 — Discovery  

---

## Process 1: Create New Migration Project

**Trigger**: User visits Home page and fills out the project creation form.

```mermaid
sequenceDiagram
    actor User
    participant Home as Home.razor (/)
    participant MSS as MigrationStateService
    participant PPS as ProjectPersistenceService

    User->>Home: Enter project name & config
    User->>Home: Click "Create Project"
    Home->>MSS: InitializeProject(name)
    MSS->>MSS: Create MigrationProject with GUID
    MSS->>MSS: Build 8 PhaseInfo objects
    MSS->>MSS: Set all phases to NotStarted
    Home->>PPS: SaveProjectAsync(project)
    PPS->>PPS: Serialize to JSON
    PPS->>PPS: Write ~/.appmod/projects/{id}.json
    Home->>User: Navigate to /dashboard
```

**Decision Points**:
- `UsePhase0`: If true (default), Phase 0 sub-phases are available. If false, they can be skipped.
- `ApplicationType`: Determines which agent prompts are most relevant.

---

## Process 2: Resume Saved Project

**Trigger**: User visits Projects page and selects a saved project.

```mermaid
sequenceDiagram
    actor User
    participant Projects as Projects.razor
    participant PPS as ProjectPersistenceService
    participant MSS as MigrationStateService

    User->>Projects: Navigate to /projects
    Projects->>PPS: ListProjectsAsync()
    PPS->>PPS: Read all *.json from ~/.appmod/projects/
    PPS-->>Projects: List<ProjectSummary>
    Projects->>User: Display project cards (name, progress, date)
    User->>Projects: Click "Resume" on a project
    Projects->>PPS: LoadProjectAsync(projectId)
    PPS->>PPS: Read {id}.json, deserialize
    PPS-->>Projects: MigrationProject
    Projects->>MSS: LoadProject(project)
    MSS->>MSS: Restore phase statuses
    Projects->>User: Navigate to /dashboard
```

---

## Process 3: Execute a Migration Phase (Chat Workflow)

**Trigger**: User navigates to a phase page from Dashboard or NavMenu.

```mermaid
sequenceDiagram
    actor User
    participant PP as PhasePage.razor
    participant CP as ChatPanel.razor
    participant CS as CopilotService
    participant APS as AgentPromptService
    participant SDK as Copilot SDK (stdio)
    participant MSS as MigrationStateService

    User->>PP: Navigate to phase route (e.g., /phase1/plan)
    PP->>MSS: StartPhase(phaseId)
    MSS->>MSS: Set status = InProgress, record StartedAt
    PP->>CP: Render ChatPanel
    CP->>APS: GetAgentPrompt(agentFileName)
    APS->>APS: Load .github/agents/{file}.agent.md
    APS->>APS: Strip YAML frontmatter, cache result
    APS-->>CP: Agent prompt markdown
    CP->>CS: CreateSessionForPhaseAsync(phaseInfo)
    CS->>SDK: Create session with system message = agent prompt
    SDK-->>CS: Session created (sessionId)
    CS-->>CP: sessionId

    loop User Chat Interaction
        User->>CP: Type message, press Enter
        CP->>CS: SendMessageAsync(sessionId, message)
        CS->>SDK: Send user message
        SDK-->>CS: Streaming response deltas
        CS->>CS: Fire OnMessageReceived events
        CS-->>CP: ChatMessage (delta)
        CP->>CP: Accumulate deltas, show streaming cursor ▌
        CS-->>CP: ChatMessage (complete)
        CP->>User: Render formatted markdown bubble
    end

    User->>PP: Click "Complete Phase"
    PP->>MSS: CompletePhase(phaseId)
    MSS->>MSS: Set status = Completed, record CompletedAt
    PP->>User: Navigate to next phase
```

**Error Handling**:
- If `CreateSessionForPhaseAsync` fails → error message in chat panel with retry button
- If `SendMessageAsync` fails → error displayed, retry available
- If SignalR circuit drops → `ReconnectModal` shown, state may be lost

---

## Process 4: Authentication Flow (GitHub OAuth)

**Trigger**: User clicks "Sign in with GitHub" button.

```mermaid
sequenceDiagram
    actor User
    participant AuthBar as AuthBar.razor
    participant Server as Program.cs
    participant GitHub as GitHub OAuth

    User->>AuthBar: Click "Sign in with GitHub"
    AuthBar->>Server: GET /auth/login
    Server->>GitHub: OAuth challenge redirect
    GitHub->>User: GitHub login page
    User->>GitHub: Authenticate
    GitHub->>Server: Callback with auth code
    Server->>Server: Create cookie (AppMod.Auth)
    Server->>User: Redirect to / with auth cookie
```

---

## Process 5: Authentication Flow (PAT)

**Trigger**: User enters a Personal Access Token in the auth bar.

```mermaid
sequenceDiagram
    actor User
    participant AuthBar as AuthBar.razor
    participant Server as Program.cs
    participant GitHubAPI as api.github.com

    User->>AuthBar: Enter PAT in text field
    User->>AuthBar: Click "Sign in"
    AuthBar->>Server: POST /auth/pat-login {token}
    Server->>GitHubAPI: GET /user (Authorization: token {pat})
    GitHubAPI-->>Server: User profile (login, name, avatar_url)
    Server->>Server: Create ClaimsIdentity
    Server->>Server: Sign in with cookie (AppMod.Auth)
    Server-->>User: 200 OK + redirect to /
```

**Error Handling**:
- Invalid PAT → GitHub API returns 401 → Server returns error response
- Network failure → Exception caught, error returned to client

---

## Process 6: Phase Progress Visualization

**Trigger**: Dashboard or PhaseProgressBar renders.

```mermaid
flowchart LR
    subgraph Phase 0
        D[Discover] --> S[Specify] --> V[Validate]
    end
    subgraph Phases 1-6
        P1[Plan] --> P2[Assess] --> P3[Migrate] --> P4[Infra] --> P5[Deploy] --> P6["CI/CD"]
    end
    V --> P1

    style D fill:#28a745,color:#fff
    style S fill:#ffc107,color:#000
    style V fill:#6c757d,color:#fff
    style P1 fill:#6c757d,color:#fff
    style P2 fill:#6c757d,color:#fff
    style P3 fill:#6c757d,color:#fff
    style P4 fill:#6c757d,color:#fff
    style P5 fill:#6c757d,color:#fff
    style P6 fill:#6c757d,color:#fff
```

**Legend**: 🟢 Completed | 🟡 In Progress | ⚫ Not Started | ➖ Skipped

**Visual Indicators** (`PhaseProgressBar.razor`):
- ✓ checkmark for Completed
- Pulsing circle for InProgress
- — dash for Skipped
- Empty circle for NotStarted
- Connecting lines between phase circles

---

## Process 7: Application Startup

**Trigger**: `dotnet run` or Azure App Service starts the process.

```mermaid
sequenceDiagram
    participant Host as WebApplication
    participant DI as DI Container
    participant CS as CopilotService
    participant MW as Middleware Pipeline

    Host->>DI: Register Razor Components
    Host->>DI: Register Authentication (Cookie + OAuth)
    Host->>DI: Register Authorization
    Host->>DI: Register AgentPromptService (Singleton)
    Host->>DI: Register CopilotService (Singleton)
    Host->>DI: Register ProjectPersistenceService (Singleton)
    Host->>DI: Register MigrationStateService (Scoped)
    Host->>MW: Configure pipeline
    Note over MW: ExceptionHandler → HSTS → StatusCodePages<br/>→ HTTPS → Auth → Authz → Antiforgery → Static Files
    Host->>Host: Map endpoints (/auth/login, /auth/pat-login, /auth/logout, /health)
    Host->>Host: Map Razor Components (Interactive Server)
    Host->>CS: InitializeAsync() [fire-and-forget]
    CS->>CS: Start CopilotClient (stdio transport)
    Host->>Host: app.RunAsync()
```

---

## End-to-End Data Flow

```
┌──────────┐     ┌──────────┐     ┌──────────────────┐     ┌──────────────┐
│  Browser  │────▶│  Blazor   │────▶│    Services       │────▶│  Persistence │
│  (User)   │◀────│  Server   │◀────│                  │◀────│              │
└──────────┘     │ (SignalR) │     │ MigrationState   │     │ JSON Files   │
                 └──────────┘     │ CopilotService   │     │ ~/.appmod/   │
                                  │ AgentPromptService│     └──────────────┘
                                  │ ProjectPersistence│
                                  └────────┬─────────┘
                                           │
                                  ┌────────┴─────────┐
                                  │  GitHub Copilot   │
                                  │  SDK (stdio)      │
                                  │                   │
                                  │  Agent Prompts    │
                                  │  .github/agents/  │
                                  └───────────────────┘
```
