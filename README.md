# GitHub Copilot Agents for Code Migration & Modernization

This repository provides a set of **GitHub Copilot custom agents** that guide the migration of legacy .NET and Java applications to modern Azure-hosted solutions. Each phase of the migration process is implemented as a standalone agent with specialized expertise, enabling a structured, AI-assisted modernization workflow.

> **Note**: This project was originally built with the deprecated "chatmodes" format (`.chatmode.md`) and has been migrated to the current [custom agents](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents) format (`.agent.md`).

## Overview

The agents implement a structured 7-phase approach to application migration, starting with an optional discovery phase for undocumented legacy systems:

```
Phase 0 (optional): Discover → Specify → Validate
                            ↓
Phase 1: Plan → Phase 2: Assess → Phase 3: Migrate Code
                                        ↓
              Phase 4: Generate Infra → Phase 5: Deploy → Phase 6: CI/CD
```

Through agent-guided workflows, developers can efficiently transform legacy applications into modern, cloud-native solutions running on Azure.

## Requirements

- GitHub Copilot (Pro, Pro+, Business, or Enterprise)
- Visual Studio Code with GitHub Copilot Chat
- Azure CLI (`az`) and Azure Developer CLI (`azd`)
- Development tools appropriate for your application (.NET SDK, JDK, etc.)

## Repository Structure

```
.github/
├── agents/                                    # Custom agent definitions
│   ├── code-migration-modernization.agent.md  # Primary orchestrator agent
│   │
│   ├── phase0-discover.agent.md               # Phase 0: Legacy system discovery
│   ├── phase0-specify.agent.md                # Phase 0: Spec & user story generation
│   ├── phase0-validate.agent.md               # Phase 0: QA validation of artifacts
│   │
│   ├── phase1-plan-migration.agent.md         # Phase 1: Planning
│   ├── phase2-assess-project.agent.md         # Phase 2: Assessment
│   ├── phase3-migrate-code.agent.md           # Phase 3: Code Migration
│   ├── phase4-generate-infra.agent.md         # Phase 4: Infrastructure as Code
│   ├── phase5-deploy-to-azure.agent.md        # Phase 5: Deployment
│   ├── phase6-setup-cicd.agent.md             # Phase 6: CI/CD Pipelines
│   │
│   ├── get-status.agent.md                    # Status tracking agent
│   └── playwright-testing.agent.md            # E2E testing agent

templates/
└── field-specification-template.md            # Standardized field spec template
```

## Agents

### Phase 0 — Legacy Discovery (Optional)

Use Phase 0 when the legacy system is undocumented, original developers are unavailable, or business logic is spread across UI, libraries, and stored procedures.

| Agent | Purpose |
|-------|---------|
| `@phase0-discover` | Reverse-engineer legacy codebase: extract business requirements, map processes across WinForms UI → class libraries → SQL stored procedures |
| `@phase0-specify` | Generate field-level specifications (using standardized template), user stories with acceptance criteria, and data dictionary |
| `@phase0-validate` | QA persona that reviews all extracted artifacts for missing edge cases, validation gaps, and logic inconsistencies |

### Phases 1–6 — Migration & Modernization

| Agent | Purpose |
|-------|---------|
| `@code-migration-modernization` | Primary orchestrator — full migration guidance and best practices |
| `@phase1-plan-migration` | Gather requirements: hosting platform, IaC type, database, target framework |
| `@phase2-assess-project` | Analyze application architecture, dependencies, risks, and generate assessment report |
| `@phase3-migrate-code` | Upgrade code to modern framework, transform configs, migrate services |
| `@phase4-generate-infra` | Generate Bicep or Terraform IaC for Azure deployment |
| `@phase5-deploy-to-azure` | Deploy application using Azure Developer CLI (azd) |
| `@phase6-setup-cicd` | Configure GitHub Actions or Azure DevOps CI/CD pipelines |

### Utility Agents

| Agent | Purpose |
|-------|---------|
| `@get-status` | Check migration progress, quality metrics, and next steps |
| `@playwright-testing` | Implement Playwright end-to-end tests for the migrated application |

## Getting Started

1. Clone this repository
2. Install [GitHub Copilot](https://copilot.github.com/) in VS Code
3. Copy the `.github/agents/` folder (and optionally `templates/`) into your target project's `.github/` directory
4. Open GitHub Copilot Chat and select an agent from the agents dropdown

### For Undocumented Legacy Systems (recommended starting point)

5. Start with `@phase0-discover` to reverse-engineer the legacy codebase
6. Run `@phase0-specify` to generate field-level specs and user stories
7. Run `@phase0-validate` to QA-review the extracted artifacts
8. Proceed to `@phase1-plan-migration` with full system understanding

### For Well-Documented Systems

5. Start directly with `@phase1-plan-migration` to begin planning
6. Use `@get-status` at any time to check migration progress
7. Follow the guided workflow through each phase

## Phase 0: Legacy Discovery Workflow

Phase 0 is designed for scenarios where:
- Original developers are no longer available
- Documentation is outdated, incomplete, or missing
- Business logic is spread across UI code, shared libraries, and SQL stored procedures
- The target architecture hasn't been decided yet

### What Phase 0 Produces

| Report | Contents |
|--------|----------|
| `reports/Legacy-Discovery-Report.md` | System inventory, architecture overview, component map, key findings |
| `reports/Business-Requirements.md` | Business rules extracted from code with source references |
| `reports/Process-Maps.md` | End-to-end process flows with Mermaid diagrams |
| `reports/Field-Specifications.md` | Detailed field specs using the standardized template |
| `reports/User-Stories.md` | User stories with acceptance criteria, organized by epic |
| `reports/Data-Dictionary.md` | Database schema mapped to business meaning |
| `reports/Requirements-Validation-Report.md` | QA findings, gaps, and SME questions |

### Field Specification Template

The `templates/field-specification-template.md` file defines the standardized format for documenting field-level specifications, including: data type, max length, validation rules, format mask, tooltip text, behavior on save/change, read-only conditions, visibility conditions, and source code traceability.

## Phases 1–6: Migration Workflow

### Phase 1: Plan Migration (`@phase1-plan-migration`)
Collects requirements — hosting platform (App Service, AKS, Container Apps), IaC type (Bicep/Terraform), database, and target framework. If Phase 0 was run, uses its architecture recommendations to pre-populate planning decisions.

### Phase 2: Assessment (`@phase2-assess-project`)
Generates a comprehensive report covering application structure, dependencies, architecture, risk analysis, and a detailed migration plan with effort estimation.

### Phase 3: Code Migration (`@phase3-migrate-code`)
Upgrades application code to modern framework versions. Handles config transformation, service migration (WCF→REST, SOAP→REST), auth modernization (→Entra ID), and containerization.

### Phase 4: Infrastructure Generation (`@phase4-generate-infra`)
Creates IaC files (Bicep or Terraform) with modules for compute, monitoring, database, identity/security, networking, and Key Vault.

### Phase 5: Deployment (`@phase5-deploy-to-azure`)
Deploys the application using `azd up` with validation, health checks, and comprehensive deployment reporting.

### Phase 6: CI/CD Setup (`@phase6-setup-cicd`)
Configures GitHub Actions or Azure DevOps pipelines with CI, CD, infrastructure deployment, and security scanning stages.

## Status Tracking

The `@get-status` agent provides:
- Executive summary with key metrics
- Phase-by-phase progress with timestamps (including Phase 0 sub-phases)
- Quality scores and completion percentages
- Risk tracking with severity levels
- Next steps with specific agent recommendations

Reports are maintained in the `reports/` folder.

## Grounding & Reducing Hallucinations

The agents use report files in `reports/` to maintain context and reduce hallucinations across phases:
- `reports/Report-Status.md` — migration status dashboard
- `reports/Application-Assessment-Report.md` — assessment details
- Phase 0 reports (when applicable) — discovery artifacts

Update these files at any phase to fit your requirements.

## Target Azure Hosting Platforms

- **Azure App Service** — Web applications and APIs
- **Azure Kubernetes Service (AKS)** — Complex microservices architectures
- **Azure Container Apps** — Containerized applications with serverless scaling

## Key Features

- **Legacy Discovery** — Reverse-engineer undocumented systems with AI-driven requirements extraction
- **Field-Level Specs** — Standardized template for detailed developer-ready specifications
- **User Story Generation** — Automated conversion of code logic to user stories with acceptance criteria
- **QA Validation** — Adversarial review agent to catch gaps and edge cases
- **Comprehensive Assessment** — Analyze .NET Framework or Java applications for cloud readiness
- **Automated Code Migration** — Transform legacy code to modern Azure-compatible versions
- **Infrastructure as Code** — Generate Bicep or Terraform for Azure resources
- **Multi-Platform Support** — Target App Service, AKS, or Container Apps
- **Authentication Modernization** — Migrate to Azure Entra ID
- **Service Migration** — WCF/SOAP to REST API conversion
- **CI/CD Integration** — GitHub Actions or Azure DevOps pipelines
- **Status Tracking** — Progress monitoring with quality metrics
- **E2E Testing** — Playwright test scaffolding for migrated applications

## Contributing

Contributions to improve the agents or add new migration scenarios are welcome. Please submit pull requests or open issues.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
