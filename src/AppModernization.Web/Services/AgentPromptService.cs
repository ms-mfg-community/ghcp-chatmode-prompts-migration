namespace AppModernization.Web.Services;

/// <summary>
/// Singleton service that loads and caches agent .md files from the .github/agents/ directory.
/// Strips YAML frontmatter and returns just the markdown body.
/// </summary>
public class AgentPromptService
{
    private readonly string _agentsDirectory;
    private readonly Dictionary<string, string> _cache = new();
    private readonly object _lock = new();

    public AgentPromptService(IWebHostEnvironment env)
    {
        _agentsDirectory = Path.GetFullPath(
            Path.Combine(env.ContentRootPath, "..", "..", ".github", "agents"));
    }

    /// <summary>
    /// Returns the markdown body of an agent file, stripping YAML frontmatter.
    /// </summary>
    public string GetAgentPrompt(string agentFileName)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(agentFileName, out var cached))
                return cached;
        }

        var filePath = Path.Combine(_agentsDirectory, agentFileName);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Agent file not found: {agentFileName}", filePath);

        var content = File.ReadAllText(filePath);
        var body = StripYamlFrontmatter(content);

        lock (_lock)
        {
            _cache[agentFileName] = body;
        }

        return body;
    }

    /// <summary>
    /// Returns a list of available agent file names.
    /// </summary>
    public List<string> GetAvailableAgents()
    {
        if (!Directory.Exists(_agentsDirectory))
            return new List<string>();

        return Directory.GetFiles(_agentsDirectory, "*.agent.md")
            .Select(Path.GetFileName)
            .Where(f => f is not null)
            .Cast<string>()
            .OrderBy(f => f)
            .ToList();
    }

    /// <summary>
    /// Strips YAML frontmatter delimited by --- from the beginning of a markdown file.
    /// </summary>
    private static string StripYamlFrontmatter(string content)
    {
        if (!content.StartsWith("---"))
            return content.Trim();

        // Find the closing ---
        var endIndex = content.IndexOf("---", 3, StringComparison.Ordinal);
        if (endIndex < 0)
            return content.Trim();

        // Skip past the closing --- and any immediate newline
        var bodyStart = endIndex + 3;
        if (bodyStart < content.Length && content[bodyStart] == '\r') bodyStart++;
        if (bodyStart < content.Length && content[bodyStart] == '\n') bodyStart++;

        return content[bodyStart..].Trim();
    }
}
