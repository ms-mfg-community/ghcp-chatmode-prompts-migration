namespace AppModernization.Web.Models;

public class MigrationProject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "New Migration Project";
    public string? SourcePath { get; set; }
    public string? HostingPlatform { get; set; }   // "App Service", "AKS", "Container Apps"
    public string? IaCType { get; set; }            // "Bicep", "Terraform"
    public string? DatabaseType { get; set; }       // "Azure SQL", "Cosmos DB", etc.
    public string? TargetFramework { get; set; }    // ".NET 10", "Java 21", etc.
    public string? ApplicationType { get; set; }    // "WinForms", "WebForms", "MVC", "WCF", "Java EE"
    public bool UsePhase0 { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<PhaseInfo> Phases { get; set; } = new();
}
