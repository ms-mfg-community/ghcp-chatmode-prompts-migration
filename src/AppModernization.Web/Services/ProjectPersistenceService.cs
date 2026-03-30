using System.Text.Json;
using AppModernization.Web.Models;

namespace AppModernization.Web.Services;

public class ProjectPersistenceService
{
    private readonly string _projectsDirectory;
    private readonly ILogger<ProjectPersistenceService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ProjectPersistenceService(ILogger<ProjectPersistenceService> logger)
    {
        _logger = logger;
        _projectsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".appmod", "projects");
        Directory.CreateDirectory(_projectsDirectory);
    }

    public async Task SaveProjectAsync(MigrationProject project)
    {
        var filePath = GetProjectPath(project.Id);
        var json = JsonSerializer.Serialize(project, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        _logger.LogInformation("Saved project {ProjectId} to {Path}", project.Id, filePath);
    }

    public async Task<MigrationProject?> LoadProjectAsync(string projectId)
    {
        var filePath = GetProjectPath(projectId);
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<MigrationProject>(json, _jsonOptions);
    }

    public async Task<List<ProjectSummary>> ListProjectsAsync()
    {
        var summaries = new List<ProjectSummary>();
        if (!Directory.Exists(_projectsDirectory)) return summaries;

        foreach (var file in Directory.GetFiles(_projectsDirectory, "*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var project = JsonSerializer.Deserialize<MigrationProject>(json, _jsonOptions);
                if (project is not null)
                {
                    var completedPhases = project.Phases.Count(p => p.Status == PhaseStatus.Completed || p.Status == PhaseStatus.Skipped);
                    var progress = project.Phases.Count > 0 ? (double)completedPhases / project.Phases.Count * 100 : 0;
                    summaries.Add(new ProjectSummary
                    {
                        Id = project.Id,
                        Name = project.Name,
                        CreatedAt = project.CreatedAt,
                        Progress = progress,
                        SourcePath = project.SourcePath,
                        CurrentPhase = project.Phases.FirstOrDefault(p => p.Status == PhaseStatus.InProgress)?.Name
                                       ?? project.Phases.FirstOrDefault(p => p.Status == PhaseStatus.NotStarted)?.Name
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read project file {File}", file);
            }
        }

        return summaries.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public Task DeleteProjectAsync(string projectId)
    {
        var filePath = GetProjectPath(projectId);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Deleted project {ProjectId}", projectId);
        }
        return Task.CompletedTask;
    }

    private string GetProjectPath(string projectId) =>
        Path.Combine(_projectsDirectory, $"{projectId}.json");
}

public class ProjectSummary
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public double Progress { get; set; }
    public string? CurrentPhase { get; set; }
    public string? SourcePath { get; set; }
}
