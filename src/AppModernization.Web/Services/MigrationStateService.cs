using AppModernization.Web.Models;

namespace AppModernization.Web.Services;

/// <summary>
/// Scoped service that manages the current migration project and its phases.
/// Registered as Scoped (per-circuit in Blazor Server).
/// </summary>
public class MigrationStateService
{
    public MigrationProject? CurrentProject { get; private set; }

    public MigrationProject InitializeProject(string name)
    {
        var project = new MigrationProject
        {
            Name = name,
            Phases = BuildPhaseDefinitions()
        };
        CurrentProject = project;
        return project;
    }

    public void LoadProject(MigrationProject project)
    {
        CurrentProject = project;
    }

    public PhaseInfo? GetCurrentPhase()
    {
        if (CurrentProject is null) return null;

        // Return the first InProgress phase, or the first NotStarted phase
        return CurrentProject.Phases.FirstOrDefault(p => p.Status == PhaseStatus.InProgress)
            ?? CurrentProject.Phases.FirstOrDefault(p => p.Status == PhaseStatus.NotStarted);
    }

    public void StartPhase(string phaseId)
    {
        var phase = CurrentProject?.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase is null || phase.Status != PhaseStatus.NotStarted) return;

        phase.Status = PhaseStatus.InProgress;
        phase.StartedAt = DateTime.UtcNow;
    }

    public void CompletePhase(string phaseId)
    {
        var phase = CurrentProject?.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase is null) return;

        phase.Status = PhaseStatus.Completed;
        phase.CompletedAt = DateTime.UtcNow;
    }

    public void SkipPhase(string phaseId)
    {
        var phase = CurrentProject?.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase is null) return;

        phase.Status = PhaseStatus.Skipped;
        phase.CompletedAt = DateTime.UtcNow;
    }

    public double GetProgress()
    {
        if (CurrentProject is null || CurrentProject.Phases.Count == 0) return 0;

        var completed = CurrentProject.Phases.Count(p =>
            p.Status == PhaseStatus.Completed || p.Status == PhaseStatus.Skipped);
        return (double)completed / CurrentProject.Phases.Count * 100;
    }

    public List<PhaseInfo> GetPhases()
    {
        return CurrentProject?.Phases ?? new List<PhaseInfo>();
    }

    private static List<PhaseInfo> BuildPhaseDefinitions()
    {
        return new List<PhaseInfo>
        {
            // Phase 0: Discovery sub-phases
            new PhaseInfo
            {
                Id = "phase0-discover",
                Name = "Discover",
                Description = "Analyze the legacy codebase to extract business requirements, map processes, and build a comprehensive understanding of the system.",
                AgentFile = "phase0-discover.agent.md",
                Route = "/phase0/discover",
                PhaseNumber = 0,
                SubPhase = "discover"
            },
            new PhaseInfo
            {
                Id = "phase0-specify",
                Name = "Specify",
                Description = "Define detailed specifications for the modernized application based on discovery findings.",
                AgentFile = "phase0-specify.agent.md",
                Route = "/phase0/specify",
                PhaseNumber = 0,
                SubPhase = "specify"
            },
            new PhaseInfo
            {
                Id = "phase0-validate",
                Name = "Validate",
                Description = "Validate the specifications and ensure completeness before migration planning begins.",
                AgentFile = "phase0-validate.agent.md",
                Route = "/phase0/validate",
                PhaseNumber = 0,
                SubPhase = "validate"
            },

            // Phase 1-6: Core migration phases
            new PhaseInfo
            {
                Id = "phase1-plan",
                Name = "Plan Migration",
                Description = "Create a comprehensive migration plan including architecture decisions, risk assessment, and timeline.",
                AgentFile = "phase1-plan-migration.agent.md",
                Route = "/phase1/plan",
                PhaseNumber = 1
            },
            new PhaseInfo
            {
                Id = "phase2-assess",
                Name = "Assess Project",
                Description = "Perform deep technical assessment of the source project including dependencies, complexity, and compatibility analysis.",
                AgentFile = "phase2-assess-project.agent.md",
                Route = "/phase2/assess",
                PhaseNumber = 2
            },
            new PhaseInfo
            {
                Id = "phase3-migrate",
                Name = "Migrate Code",
                Description = "Execute the code migration, converting legacy patterns to modern equivalents while preserving business logic.",
                AgentFile = "phase3-migrate-code.agent.md",
                Route = "/phase3/migrate",
                PhaseNumber = 3
            },
            new PhaseInfo
            {
                Id = "phase4-infra",
                Name = "Generate Infrastructure",
                Description = "Generate Infrastructure as Code (Bicep/Terraform) for the target Azure environment.",
                AgentFile = "phase4-generate-infra.agent.md",
                Route = "/phase4/infra",
                PhaseNumber = 4
            },
            new PhaseInfo
            {
                Id = "phase5-deploy",
                Name = "Deploy to Azure",
                Description = "Deploy the migrated application and infrastructure to Azure.",
                AgentFile = "phase5-deploy-to-azure.agent.md",
                Route = "/phase5/deploy",
                PhaseNumber = 5
            },
            new PhaseInfo
            {
                Id = "phase6-cicd",
                Name = "Setup CI/CD",
                Description = "Configure continuous integration and deployment pipelines with GitHub Actions.",
                AgentFile = "phase6-setup-cicd.agent.md",
                Route = "/phase6/cicd",
                PhaseNumber = 6
            }
        };
    }
}
