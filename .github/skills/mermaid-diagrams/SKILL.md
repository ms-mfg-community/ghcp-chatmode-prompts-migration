---
name: mermaid-diagrams
description: >-
  Generates rich Mermaid diagrams for process maps, architecture visualization,
  data flows, class hierarchies, and entity relationships. Use when creating
  reports that need visual diagrams — always prefer Mermaid over ASCII art.
---

## Purpose

Generate professional, readable Mermaid diagrams in markdown reports. All process maps, architecture diagrams, data flows, and relationship visualizations MUST use Mermaid syntax — never ASCII art, box-drawing characters, or plain text diagrams.

## Diagram Types and When to Use Them

### Flowchart — Business Process Flows
Use for: UI workflows, decision trees, business logic paths, stored procedure logic

```mermaid
flowchart TD
    A[User Action] --> B{Decision Point}
    B -->|Yes| C[Process Step]
    B -->|No| D[Alternative Path]
    C --> E[Database Operation]
    E --> F[Result]
    D --> F
```

**Rules:**
- Use `TD` (top-down) for business processes
- Use `LR` (left-right) for data pipelines
- Decision nodes use `{curly braces}`
- Process nodes use `[square brackets]`
- Database/storage nodes use `[(cylinder)]` or `[(Database)]`
- Group related steps with `subgraph`

### Sequence Diagram — Component Interactions
Use for: API calls, form submissions, stored procedure call chains, service interactions

```mermaid
sequenceDiagram
    actor User
    participant UI as WinForm
    participant BL as Business Layer
    participant DB as SQL Server

    User->>UI: Click Save
    UI->>BL: Validate()
    BL->>DB: EXEC sp_ValidateRecord
    DB-->>BL: ValidationResult
    alt Valid
        BL->>DB: EXEC sp_SaveRecord
        DB-->>BL: Success
        BL-->>UI: Show confirmation
    else Invalid
        BL-->>UI: Show errors
    end
```

**Rules:**
- Always include `actor User` for user-initiated flows
- Use `participant X as Display Name` for readable labels
- Use `->>` for sync calls, `-->>` for responses
- Use `alt/else/end` for conditional branches
- Use `loop` for repeated operations
- Use `Note over X,Y: text` for annotations

### Class Diagram — Code Architecture
Use for: Class hierarchies, inheritance, form structures, component relationships

```mermaid
classDiagram
    class MainForm {
        -DataGridView dgvRecords
        -TextBox txtSearch
        +LoadData() void
        +SaveRecord() bool
        -ValidateInput() bool
    }
    class DataAccess {
        -SqlConnection conn
        +ExecuteSP(name, params) DataTable
        +GetRecords(filter) List
    }
    MainForm --> DataAccess : uses
    MainForm ..|> Form : inherits
```

**Rules:**
- Show key fields and methods (not every one)
- Use `-->` for composition/dependency
- Use `..|>` for inheritance
- Use `--o` for aggregation
- Mark visibility: `+` public, `-` private, `#` protected

### ER Diagram — Data Models
Use for: Database schemas, table relationships, stored procedure I/O

```mermaid
erDiagram
    CUSTOMER ||--o{ ORDER : places
    ORDER ||--|{ ORDER_LINE : contains
    ORDER_LINE }o--|| PRODUCT : references
    CUSTOMER {
        int CustomerID PK
        string Name
        string Email
        datetime CreatedDate
    }
    ORDER {
        int OrderID PK
        int CustomerID FK
        datetime OrderDate
        decimal Total
    }
```

**Rules:**
- Always mark PK and FK
- Show data types
- Use correct cardinality notation
- Group related tables visually

### State Diagram — Phase/Status Flows
Use for: Record lifecycle, approval workflows, migration phase progression

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> Pending : Submit
    Pending --> Approved : Approve
    Pending --> Rejected : Reject
    Rejected --> Draft : Revise
    Approved --> Active : Activate
    Active --> Archived : Archive
    Active --> [*] : Delete
```

### Architecture / Deployment Diagram (Flowchart with subgraphs)
Use for: System architecture, deployment topology, integration maps

```mermaid
flowchart TB
    subgraph Frontend["Frontend Layer"]
        A[WinForms App]
        B[Web Portal]
    end
    subgraph Business["Business Layer"]
        C[Business Logic DLL]
        D[Shared Components]
    end
    subgraph Data["Data Layer"]
        E[(SQL Server)]
        F[Stored Procedures]
    end
    A --> C
    B --> C
    C --> D
    C --> E
    E --> F
```

## Formatting Standards

1. **Every process map MUST have at least one Mermaid diagram** — preferably a flowchart for the overall flow and sequence diagrams for detailed interactions
2. **Label all nodes clearly** — use business-friendly names, not code identifiers
3. **Add subgraphs** to group related components (UI layer, business layer, data layer)
4. **Use consistent styling:**
   - Colors via `style` or `classDef` for status: green=success, red=error, blue=active, gray=inactive
   - Keep diagrams under 20 nodes for readability — split larger flows into sub-diagrams
5. **Include a legend** if using color coding
6. **Wrap diagram in markdown fenced code block**: ` ```mermaid ... ``` `
7. **Pair each diagram with a brief text description** explaining the flow in 2-3 sentences

## Anti-Patterns (Never Do These)

- ❌ ASCII art boxes: `+--------+`
- ❌ Arrow text: `Form --> BL --> DB`
- ❌ Plain text flowcharts: `Step 1 -> Step 2 -> Step 3`
- ❌ Tables pretending to be flowcharts
- ❌ Diagrams with 30+ nodes (split them)
- ❌ Unlabeled arrows or generic node names like "Process" or "Step"
