# Phase 0 Discovery — Guided Demo Script

> **Purpose**: Walk a customer through the Phase 0 Discovery workflow, demonstrating how GitHub Copilot agents automatically extract business requirements, map processes, and generate developer-ready specifications from an undocumented legacy codebase.
>
> **Duration**: 30–45 minutes  
> **Audience**: Technical decision-makers, development leads, business analysts

---

## Pre-Demo Setup

### 1. Prerequisites
- [ ] App Modernization Web running locally (`dotnet run` from `src/AppModernization.Web/`)
- [ ] Authenticated via GitHub OAuth or PAT
- [ ] Legacy codebase available on local disk (customer-provided or sample WinForms/SQL app)
- [ ] Browser open to `https://localhost:7292`

### 2. Reset for Clean Demo
If you've previously run Phase 0 on this codebase, either:
- Delete the existing project from the **Projects** page
- Create a new project with a fresh name

---

## Demo Script

### Opening (2 min)

> **Talk track**: "Today I'm going to show you how we can use GitHub Copilot to solve one of the biggest challenges in application modernization — understanding what a legacy system actually does when the original developers are gone and the documentation is outdated or missing."

**Key customer pain points to reference:**
- Original developers no longer with the organization
- Business logic spread across UI code, shared components, and stored procedures
- Developers refuse to start work without detailed documentation
- Manual reverse engineering takes weeks or months

---

### Part 1: Project Setup (3 min)

**Navigate to**: Home page (`/`)

1. **Create a new project**:
   - Project Name: Use the customer's app name (e.g., "Benefits Management System")
   - Source Path: Point to the legacy codebase on disk
   - Click **Create Project**

> **Talk track**: "I'm pointing the tool at your legacy codebase. No uploading to the cloud — the AI analyzes the code locally on your machine."

2. **Show the Dashboard** (`/dashboard`):
   - Highlight the 9-phase migration workflow
   - Point out Phase 0 (Discover → Specify → Validate)

> **Talk track**: "The modernization process is broken into phases. Phase 0 is where we extract everything we need to understand from the legacy system before we write a single line of new code."

---

### Part 2: Automated Discovery (10–15 min)

**Navigate to**: Discover page (`/phase0/discover`)

> **Talk track**: "This is where the magic happens. The Copilot agent is going to read every file in your codebase — every form, every class, every stored procedure — and extract the business logic automatically."

1. **Click "Get Started"** — the auto-prompt fires:
   - Watch the chat panel as Copilot begins analysis
   - It will scan project files, identify the application type, and start extracting

2. **While Copilot works, narrate what it's doing**:

> **Talk track for each step**:

| What you see | What to say |
|---|---|
| "Scanning the codebase..." | "It's identifying all the projects, forms, classes, and SQL scripts in the solution." |
| Reading `.cs` files | "Now it's reading the C# code — looking for business rules embedded in event handlers, validation logic, and conditional branches." |
| Reading `.sql` files | "Here it's analyzing stored procedures — this is where a huge amount of business logic lives in legacy apps. Things like eligibility checks, calculations, and data transformations." |
| Generating reports | "It's now generating three reports: a Discovery Report with the full system architecture, a Business Requirements document, and Process Maps showing how everything connects." |

3. **Point out the reports appearing** in the left panel:
   - **Legacy-Discovery-Report.md** — System architecture, technology stack, component inventory
   - **Business-Requirements.md** — Extracted business rules with source code references
   - **Process-Maps.md** — End-to-end flows with Mermaid diagrams

4. **Open a report** (click the link):
   - Show the architecture diagram (Mermaid)
   - Show a specific business requirement with its source code location
   - Show a process map tracing UI → Business Logic → Database

> **VALUE MOMENT #1** — Automated Requirements Extraction:
> "What you're looking at would normally take a team of developers and business analysts weeks to produce manually. The AI just did it in minutes by reading the source code directly."

---

### Part 3: Process Mapping Deep-Dive (5 min)

**Open**: Process-Maps report

> **Talk track**: "Your team asked to see how everything connects — how UI components talk to shared libraries, which call stored procedures, which hit database tables. Let me show you exactly that."

**Key things to highlight:**
- Mermaid sequence diagrams showing multi-layer interactions
- Decision points and conditional branches documented
- Error handling paths captured
- Data transformations at each layer

> **VALUE MOMENT #2** — End-to-End Process Mapping:
> "This is the 'how everything connects and works' view your team has been asking for. Every business process traced from the user's click all the way down to the database — automatically."

---

### Part 4: Structured Specifications (10 min)

**Navigate to**: Specify page (`/phase0/specify`)

> **Talk track**: "Now that we understand what the system does, let's generate the detailed specifications your developers need before they can start building. Your team told us they need field-level specs — data types, max lengths, validation rules, tooltips, behavior on save. Let's generate those automatically."

1. **Click "Get Started"** — the auto-prompt fires:
   - Copilot reads the discovery reports it just generated
   - Begins producing field-level specifications

2. **While it works, explain the outputs**:

| Report | What it contains | Why developers need it |
|---|---|---|
| **Field-Specifications.md** | Every field on every form: data type, max length, validation rules, format mask, default value, behavior on save/change, read-only conditions, visibility conditions, database column mapping | "Developers can start building forms immediately — no guessing, no asking questions." |
| **User-Stories.md** | User stories in standard format with acceptance criteria (Given/When/Then) | "QA and BA teams can review and validate against business expectations." |
| **Data-Dictionary.md** | Complete data model: tables, columns, relationships, constraints | "Database developers know exactly what schema to build." |

3. **Open Field-Specifications.md** and show a specific form:
   - Point out the 16 properties per field
   - Show source code line references ("This rule came from line 47 of CustomerForm.cs")
   - Show confidence levels ("High confidence — extracted from explicit validation code")

> **VALUE MOMENT #3** — Structured Output for Developers:
> "This is the documentation your developers said they need before they'll start work. Data types, max lengths, validation rules, tooltips, behavior on save — all extracted directly from the code. Not written by a human who might miss something, but by AI that reads every line."

---

### Part 5: QA Validation (5 min)

**Navigate to**: Validate page (`/phase0/validate`)

> **Talk track**: "But how do we know the AI got it right? This is where our adversarial QA agent comes in. It reviews everything the other agents produced and identifies gaps, inconsistencies, and missing edge cases."

1. **Click "Get Started"** — the validation agent runs:
   - Reviews all 6 reports from Discover and Specify
   - Generates a Validation Report

2. **Show the Validation Report**:
   - Missing edge cases identified
   - Validation rules that seem incomplete
   - Fields without database column mappings
   - Business rules that need human SME confirmation

> **VALUE MOMENT #4** — A Repeatable Workflow:
> "This three-step process — Discover, Specify, Validate — is completely repeatable. Point it at any legacy codebase and it produces the same structured output. Requirements, user stories, field specs, process maps, and QA validation — every time."

---

### Part 6: Business Value Summary (3 min)

**Navigate to**: Dashboard or Reports page

> **Talk track**: "Let me summarize what we just accomplished and the value it delivers."

| Metric | Traditional Approach | With Copilot Phase 0 |
|---|---|---|
| **Time to extract requirements** | 4–8 weeks | 30–60 minutes |
| **Documentation completeness** | Depends on SME availability | Every code path analyzed |
| **Field-level specifications** | Manual, error-prone | Automated with source references |
| **Process maps** | Whiteboard sessions, tribal knowledge | Generated from actual code |
| **QA validation** | Separate review cycle | Built into the workflow |

**Business value points to emphasize:**

1. **Cost Reduction**: "What would have been hundreds of hours of manual documentation is now automated. Your analysts can focus on validating and refining, not writing from scratch."

2. **Faster Time-to-Value**: "Your modernization project just accelerated by weeks. The analysis bottleneck is eliminated."

3. **Risk Mitigation**: "Business logic — especially for health, eligibility, and policy programs — is captured from the actual code, not from someone's memory. Compliance and operational risk is reduced."

4. **Cross-Team Alignment**: "Business, development, and QA all look at the same AI-generated artifacts. One source of truth, validated by an adversarial QA agent."

---

### Closing (2 min)

> **Talk track**: "This is just Phase 0. Once we have this foundation, the same tool walks you through migration planning, code assessment, actual code migration, infrastructure generation, deployment, and CI/CD setup — all powered by specialized Copilot agents."

**Next steps to offer:**
- "Let's run this against a module of your actual codebase right now."
- "I can set this up for your team to start using on their own projects."
- "Let's schedule a follow-up to review the Phase 0 output with your BA and QA teams."

---

## Handling Common Questions

| Question | Response |
|---|---|
| "Does the code leave my machine?" | "No. The AI analyzes the code locally through the Copilot CLI. Your source code stays on your machine." |
| "How accurate is this?" | "The extraction is based on static code analysis — it reads what the code actually does, not what someone thinks it does. The Validate phase specifically identifies areas where human SME confirmation is needed." |
| "What if the AI misses something?" | "That's exactly what the Validate phase is for. The QA agent operates as an adversarial reviewer, looking for gaps and inconsistencies. We also assign confidence levels to every extracted requirement." |
| "Does this work with [X technology]?" | "Phase 0 supports .NET Framework (WinForms, WebForms, MVC, WCF), Java EE, and SQL Server stored procedures. The skills can be extended for other stacks." |
| "Can we customize the output format?" | "Yes — the field specification template and agent prompts are all configurable. You can adjust what fields are captured, add your own templates, or change the output format." |
| "What happens after Phase 0?" | "The output feeds directly into Phase 1 (Migration Planning) where the AI helps architect the target system. Then through assessment, code migration, infrastructure, deployment, and CI/CD — each with its own specialized agent." |
| "How does this integrate with our existing tools?" | "The output is standard Markdown — it works with GitHub Issues, wikis, Azure DevOps, Confluence, or any documentation platform. User stories can be pushed directly to your backlog." |

---

## Demo Failure Recovery

| Issue | Recovery |
|---|---|
| Copilot session fails to connect | Click "Retry Connection". If persistent, restart the app and re-authenticate. |
| Reports not generating | Check the chat panel for error messages. Ensure the source path is correct and accessible. |
| Empty chat responses | The agent is executing tools (reading files). Wait for it to finish — the spinner should be showing. |
| Phase stuck on "InProgress" | Reports may have been generated in the source directory. Check `reports/` folder. Click "Complete & Continue" manually. |
| Slow analysis | Large codebases take longer. Narrate what's happening: "It's reading through [X] files — this is the most thorough analysis you'll get." |
