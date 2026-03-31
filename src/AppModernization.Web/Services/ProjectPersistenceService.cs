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

    public string GetProjectReportsPath(string projectId)
    {
        var path = Path.Combine(_projectsDirectory, projectId, "reports");
        Directory.CreateDirectory(path);
        return path;
    }

    public async Task SaveProjectAsync(MigrationProject project)
    {
        var projectDir = Path.Combine(_projectsDirectory, project.Id);
        Directory.CreateDirectory(projectDir);
        Directory.CreateDirectory(Path.Combine(projectDir, "reports"));
        var filePath = Path.Combine(projectDir, "project.json");
        var json = JsonSerializer.Serialize(project, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        _logger.LogInformation("Saved project {ProjectId} to {Path}", project.Id, filePath);
    }

    public async Task<MigrationProject?> LoadProjectAsync(string projectId)
    {
        // New layout: {id}/project.json
        var filePath = Path.Combine(_projectsDirectory, projectId, "project.json");
        if (!File.Exists(filePath))
        {
            // Backward compat: try legacy {id}.json
            filePath = Path.Combine(_projectsDirectory, $"{projectId}.json");
            if (!File.Exists(filePath)) return null;
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<MigrationProject>(json, _jsonOptions);
    }

    public async Task<List<ProjectSummary>> ListProjectsAsync()
    {
        var summaries = new List<ProjectSummary>();
        if (!Directory.Exists(_projectsDirectory)) return summaries;

        var projectFiles = new List<string>();

        // New layout: scan for */project.json directories
        foreach (var dir in Directory.GetDirectories(_projectsDirectory))
        {
            var projectFile = Path.Combine(dir, "project.json");
            if (File.Exists(projectFile))
                projectFiles.Add(projectFile);
        }

        // Backward compat: also scan for legacy *.json files in root
        foreach (var file in Directory.GetFiles(_projectsDirectory, "*.json"))
        {
            // Skip if this project already found via directory layout
            var fileId = Path.GetFileNameWithoutExtension(file);
            var dirProjectFile = Path.Combine(_projectsDirectory, fileId, "project.json");
            if (!File.Exists(dirProjectFile))
                projectFiles.Add(file);
        }

        foreach (var file in projectFiles)
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
        // New layout: delete the project directory
        var projectDir = Path.Combine(_projectsDirectory, projectId);
        if (Directory.Exists(projectDir))
        {
            Directory.Delete(projectDir, recursive: true);
            _logger.LogInformation("Deleted project directory {ProjectId}", projectId);
        }

        // Backward compat: also delete legacy file if it exists
        var legacyFile = Path.Combine(_projectsDirectory, $"{projectId}.json");
        if (File.Exists(legacyFile))
        {
            File.Delete(legacyFile);
            _logger.LogInformation("Deleted legacy project file {ProjectId}", projectId);
        }

        return Task.CompletedTask;
    }

    private string GetProjectPath(string projectId) =>
        Path.Combine(_projectsDirectory, projectId, "project.json");
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
