namespace AppModernization.Web.Models;

public enum PhaseStatus { NotStarted, InProgress, Completed, Skipped }

public class PhaseInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string AgentFile { get; set; }  // e.g., "phase0-discover.agent.md"
    public required string Route { get; set; }       // e.g., "/phase0/discover"
    public PhaseStatus Status { get; set; } = PhaseStatus.NotStarted;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int PhaseNumber { get; set; }
    public string? SubPhase { get; set; }  // e.g., "discover", "specify", "validate" for Phase 0
}
