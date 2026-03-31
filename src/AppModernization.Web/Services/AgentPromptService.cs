namespace AppModernization.Web.Services;

/// <summary>
/// Singleton service that loads and caches agent .md files from the .github/agents/ directory.
/// Strips YAML frontmatter, resolves referenced skills, and returns the combined prompt.
/// </summary>
public class AgentPromptService
{
    private readonly string _agentsDirectory;
    private readonly string _skillsDirectory;
    private readonly Dictionary<string, string> _cache = new();
    private readonly Dictionary<string, string> _skillCache = new();
    private readonly object _lock = new();
    private readonly ILogger<AgentPromptService> _logger;

    public AgentPromptService(IWebHostEnvironment env, ILogger<AgentPromptService> logger)
    {
        _logger = logger;
        var repoRoot = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", ".."));
        _agentsDirectory = Path.Combine(repoRoot, ".github", "agents");
        _skillsDirectory = Path.Combine(repoRoot, ".github", "skills");
    }

    /// <summary>
    /// Returns the combined prompt: agent markdown body + referenced skill content.
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
        var frontmatter = ExtractFrontmatter(content);
        var body = StripYamlFrontmatter(content);

        // Parse skills from frontmatter
        var skillNames = ParseSkillReferences(frontmatter);

        // Load and append skill content
        var combinedPrompt = body;
        if (skillNames.Count > 0)
        {
            var skillSections = new List<string>();
            foreach (var skillName in skillNames)
            {
                var skillContent = LoadSkillContent(skillName);
                if (skillContent is not null)
                {
                    skillSections.Add($"### Skill: {skillName}\n\n{skillContent}");
                    _logger.LogDebug("Loaded skill '{SkillName}' for agent '{Agent}'", skillName, agentFileName);
                }
                else
                {
                    _logger.LogWarning("Skill '{SkillName}' referenced by agent '{Agent}' not found", skillName, agentFileName);
                }
            }

            if (skillSections.Count > 0)
            {
                combinedPrompt += "\n\n---\n\n## Referenced Skills\n\nUse the following skill knowledge when relevant to the task:\n\n"
                    + string.Join("\n\n---\n\n", skillSections);
            }
        }

        lock (_lock)
        {
            _cache[agentFileName] = combinedPrompt;
        }

        return combinedPrompt;
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
    /// Returns a list of available skill names.
    /// </summary>
    public List<string> GetAvailableSkills()
    {
        if (!Directory.Exists(_skillsDirectory))
            return new List<string>();

        return Directory.GetDirectories(_skillsDirectory)
            .Where(d => File.Exists(Path.Combine(d, "SKILL.md")))
            .Select(d => new DirectoryInfo(d).Name)
            .OrderBy(n => n)
            .ToList();
    }

    /// <summary>
    /// Loads a skill's SKILL.md content, stripping frontmatter. Returns null if not found.
    /// </summary>
    private string? LoadSkillContent(string skillName)
    {
        lock (_lock)
        {
            if (_skillCache.TryGetValue(skillName, out var cached))
                return cached;
        }

        var skillFile = Path.Combine(_skillsDirectory, skillName, "SKILL.md");
        if (!File.Exists(skillFile))
            return null;

        var content = File.ReadAllText(skillFile);
        var body = StripYamlFrontmatter(content);

        lock (_lock)
        {
            _skillCache[skillName] = body;
        }

        return body;
    }

    /// <summary>
    /// Extracts the YAML frontmatter string (content between --- markers).
    /// </summary>
    private static string ExtractFrontmatter(string content)
    {
        if (!content.StartsWith("---"))
            return "";

        var endIndex = content.IndexOf("---", 3, StringComparison.Ordinal);
        if (endIndex < 0)
            return "";

        return content[3..endIndex].Trim();
    }

    /// <summary>
    /// Parses skill references from YAML frontmatter.
    /// Supports inline: skills: ['skill-a', 'skill-b'] or skills: [skill-a, skill-b]
    /// and multi-line:
    ///   skills:
    ///     - skill-a
    ///     - skill-b
    /// </summary>
    private static List<string> ParseSkillReferences(string frontmatter)
    {
        var skills = new List<string>();

        foreach (var line in frontmatter.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("skills:"))
            {
                var value = trimmed["skills:".Length..].Trim();
                if (value.StartsWith('[') && value.EndsWith(']'))
                {
                    var inner = value[1..^1];
                    foreach (var item in inner.Split(','))
                    {
                        var name = item.Trim().Trim('\'', '"').Trim();
                        if (!string.IsNullOrWhiteSpace(name))
                            skills.Add(name);
                    }
                }
                break;
            }
        }

        // Handle multi-line YAML list format
        if (skills.Count == 0)
        {
            var inSkills = false;
            foreach (var line in frontmatter.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed == "skills:")
                {
                    inSkills = true;
                    continue;
                }
                if (inSkills)
                {
                    if (trimmed.StartsWith("- "))
                    {
                        var name = trimmed[2..].Trim().Trim('\'', '"').Trim();
                        if (!string.IsNullOrWhiteSpace(name))
                            skills.Add(name);
                    }
                    else if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        break;
                    }
                }
            }
        }

        return skills;
    }

    /// <summary>
    /// Strips YAML frontmatter delimited by --- from the beginning of a markdown file.
    /// </summary>
    private static string StripYamlFrontmatter(string content)
    {
        if (!content.StartsWith("---"))
            return content.Trim();

        var endIndex = content.IndexOf("---", 3, StringComparison.Ordinal);
        if (endIndex < 0)
            return content.Trim();

        var bodyStart = endIndex + 3;
        if (bodyStart < content.Length && content[bodyStart] == '\r') bodyStart++;
        if (bodyStart < content.Length && content[bodyStart] == '\n') bodyStart++;

        return content[bodyStart..].Trim();
    }
}
