---
name: azure-hosting-selection
description: >-
  Decision framework for selecting the right Azure hosting platform (App Service,
  AKS, Container Apps) based on application characteristics. Use when helping users
  choose a hosting platform or comparing Azure compute options.
---

## Purpose

Provide a structured decision framework for selecting the optimal Azure hosting platform based on application type, complexity, team capabilities, and operational requirements.

## Decision Matrix

| Factor | App Service | Container Apps | AKS |
|--------|------------|---------------|-----|
| **Complexity** | Low | Medium | High |
| **Best for** | Web apps, APIs | Microservices, event-driven | Complex distributed systems |
| **Scaling** | Built-in auto-scale | Event-driven auto-scale (KEDA) | Full Kubernetes HPA/VPA |
| **Container required** | No (code deploy) | Yes | Yes |
| **Custom networking** | Limited | Moderate | Full control |
| **Ops overhead** | Minimal | Low-Medium | High |
| **Cost** | Predictable (plan-based) | Pay-per-use | Cluster + node costs |
| **CI/CD** | Built-in deployment slots | Revisions-based | Helm/Kubectl/GitOps |
| **Team skill needed** | Web dev | Containers + basics | Kubernetes expertise |

## Quick Decision Flowchart

```
Is the app a simple web app or API?
├── YES → Do you need custom containers?
│         ├── NO → Azure App Service ✅
│         └── YES → Azure Container Apps ✅
└── NO → Is it a microservices architecture?
          ├── YES → Do you need full Kubernetes control?
          │         ├── YES → AKS ✅
          │         └── NO → Azure Container Apps ✅
          └── NO → Is it a background/batch workload?
                    ├── YES → Azure Container Apps (scale to zero) ✅
                    └── NO → Azure App Service ✅
```

## Recommendations by Application Type

### WinForms → Web Migration
- **Simple data entry apps**: App Service (Blazor Server or MVC)
- **Complex multi-form apps**: Container Apps (allows microservice decomposition)
- **Enterprise with existing K8s**: AKS

### ASP.NET WebForms / MVC Migration
- **Direct port**: App Service (simplest deployment)
- **Modernization with new architecture**: Container Apps
- **Part of larger platform**: AKS

### WCF Service Migration
- **Single service**: App Service (as Web API)
- **Multiple services**: Container Apps (natural microservice boundary)
- **Service mesh requirements**: AKS

### Java EE Migration
- **Single Spring Boot app**: App Service (Java support built-in)
- **Microservices**: Container Apps or AKS
- **Existing Kubernetes expertise**: AKS

## Cost Comparison Guidance

| Tier | App Service | Container Apps | AKS |
|------|------------|---------------|-----|
| **Dev/Test** | Free/Basic tier | Pay only for usage | Dev/test node pool |
| **Production (small)** | Standard S1 ~$75/mo | ~$50-150/mo variable | ~$200+/mo (min cluster) |
| **Production (large)** | Premium P1V3 ~$150/mo | Scales with demand | Scales with nodes |

## Key Considerations

- **App Service** is the default recommendation unless there's a specific reason to use containers
- **Container Apps** is ideal when you want containers without Kubernetes complexity
- **AKS** is for teams that need (and can manage) full Kubernetes capabilities
- All three support **managed identity**, **Entra ID**, and **private endpoints**
- All three integrate with **Application Insights** for monitoring
