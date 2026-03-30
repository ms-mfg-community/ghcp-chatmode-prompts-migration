# GitHub Copilot Agents for Code Migration & Modernization

This repository provides a set of **GitHub Copilot custom agents** that guide the migration of legacy .NET and Java applications to modern Azure-hosted solutions. Each phase of the migration process is implemented as a standalone agent with specialized expertise, enabling a structured, AI-assisted modernization workflow.

> **Note**: This project was originally built with the deprecated "chatmodes" format (`.chatmode.md`) and has been migrated to the current [custom agents](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents) format (`.agent.md`).

## Overview

The agents implement a structured 6-phase approach to application migration:

1. **Plan** → **Assess** → **Migrate Code** → **Generate Infra** → **Deploy** → **CI/CD**

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
│   ├── phase1-plan-migration.agent.md         # Phase 1: Planning
│   ├── phase2-assess-project.agent.md         # Phase 2: Assessment
│   ├── phase3-migrate-code.agent.md           # Phase 3: Code Migration
│   ├── phase4-generate-infra.agent.md         # Phase 4: Infrastructure as Code
│   ├── phase5-deploy-to-azure.agent.md        # Phase 5: Deployment
│   ├── phase6-setup-cicd.agent.md             # Phase 6: CI/CD Pipelines
│   ├── get-status.agent.md                    # Status tracking agent
│   └── playwright-testing.agent.md            # E2E testing agent
```

## Agents

| Agent | Purpose |
|-------|---------|
| `@code-migration-modernization` | Primary orchestrator — full migration guidance and best practices |
| `@phase1-plan-migration` | Gather requirements: hosting platform, IaC type, database, target framework |
| `@phase2-assess-project` | Analyze application architecture, dependencies, risks, and generate assessment report |
| `@phase3-migrate-code` | Upgrade code to modern framework, transform configs, migrate services |
| `@phase4-generate-infra` | Generate Bicep or Terraform IaC for Azure deployment |
| `@phase5-deploy-to-azure` | Deploy application using Azure Developer CLI (azd) |
| `@phase6-setup-cicd` | Configure GitHub Actions or Azure DevOps CI/CD pipelines |
| `@get-status` | Check migration progress, quality metrics, and next steps |
| `@playwright-testing` | Implement Playwright end-to-end tests for the migrated application |

## Getting Started

1. Clone this repository
2. Install [GitHub Copilot](https://copilot.github.com/) in VS Code
3. Copy the `.github/agents/` folder into your target project's `.github/` directory
4. Open GitHub Copilot Chat and select an agent from the agents dropdown
5. Start with `@phase1-plan-migration` to begin the migration planning
6. Use `@get-status` at any time to check migration progress
7. Follow the guided workflow through each phase

## Migration Workflow

### Phase 1: Plan Migration (`@phase1-plan-migration`)
Collects requirements — hosting platform (App Service, AKS, Container Apps), IaC type (Bicep/Terraform), database, and target framework. Creates initial status and assessment report files.

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
- Phase-by-phase progress with timestamps
- Quality scores and completion percentages
- Risk tracking with severity levels
- Next steps with specific agent recommendations

Reports are maintained in the `reports/` folder.

## Grounding & Reducing Hallucinations

The agents use two files in `reports/` to maintain context and reduce hallucinations across phases:
- `reports/Report-Status.md` — migration status dashboard
- `reports/Application-Assessment-Report.md` — assessment details

Update these files at any phase to fit your requirements.

## Target Azure Hosting Platforms

- **Azure App Service** — Web applications and APIs
- **Azure Kubernetes Service (AKS)** — Complex microservices architectures
- **Azure Container Apps** — Containerized applications with serverless scaling

## Key Features

- **Comprehensive Assessment** — Analyze .NET Framework or Java applications for cloud readiness
- **Automated Code Migration** — Transform legacy code to modern Azure-compatible versions
- **Infrastructure as Code** — Generate Bicep or Terraform for Azure resources
- **Multi-Platform Support** — Target App Service, AKS, or Container Apps
- **Authentication Modernization** — Migrate to Azure Entra ID
- **Service Migration** — WCF/SOAP to REST API conversion
- **CI/CD Integration** — GitHub Actions or Azure DevOps pipelines
- **Status Tracking** — Progress monitoring with quality metrics
- **Risk Assessment** — Identification and mitigation strategies
- **E2E Testing** — Playwright test scaffolding for migrated applications

## Contributing

Contributions to improve the agents or add new migration scenarios are welcome. Please submit pull requests or open issues.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

