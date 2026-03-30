using System.Collections.Concurrent;
using AppModernization.Web.Models;
using GitHub.Copilot.SDK;

namespace AppModernization.Web.Services;

/// <summary>
/// Singleton service wrapping the GitHub Copilot SDK.
/// Manages CopilotClient lifecycle and sessions.
/// </summary>
public class CopilotService : IAsyncDisposable
{
    private readonly AgentPromptService _agentPromptService;
    private readonly ILogger<CopilotService> _logger;
    private CopilotClient? _client;
    private readonly ConcurrentDictionary<string, CopilotSession> _sessions = new();
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    /// <summary>
    /// Event fired when a chat message (or streaming delta) is received.
    /// The string key is the session ID.
    /// </summary>
    public event Action<string, ChatMessage>? OnMessageReceived;

    /// <summary>
    /// Event fired when a session becomes idle (all processing complete).
    /// </summary>
    public event Action<string>? OnSessionIdle;

    public CopilotService(AgentPromptService agentPromptService, ILogger<CopilotService> logger)
    {
        _agentPromptService = agentPromptService;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the CopilotClient. Safe to call multiple times.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized) return;

            _client = new CopilotClient(new CopilotClientOptions
            {
                AutoStart = true,
                UseStdio = true
            });

            await _client.StartAsync();
            _initialized = true;
            _logger.LogInformation("CopilotClient started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start CopilotClient");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Creates a new CopilotSession for the given migration phase.
    /// The agent prompt is loaded from the .md file and injected as a system message in Append mode.
    /// </summary>
    public async Task<string> CreateSessionForPhaseAsync(PhaseInfo phase, string? workingDirectory = null, CancellationToken cancellationToken = default)
    {
        if (_client is null)
            throw new InvalidOperationException("CopilotClient not initialized. Call InitializeAsync first.");

        var agentPrompt = _agentPromptService.GetAgentPrompt(phase.AgentFile);

        // If a working directory is specified, recreate the client with that CWD
        // so the agent's file operations target the correct codebase
        if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
        {
            _logger.LogInformation("Setting Copilot working directory to: {Cwd}", workingDirectory);
            // Stop existing client and restart with new CWD
            if (_initialized)
            {
                try { await _client.StopAsync(); } catch { /* ignore */ }
                _initialized = false;
            }

            _client = new CopilotClient(new CopilotClientOptions
            {
                AutoStart = true,
                UseStdio = true,
                Cwd = workingDirectory
            });
            await _client.StartAsync();
            _initialized = true;
        }

        var session = await _client.CreateSessionAsync(new SessionConfig
        {
            OnPermissionRequest = PermissionHandler.ApproveAll,
            Streaming = true,
            SystemMessage = new SystemMessageConfig
            {
                Mode = SystemMessageMode.Append,
                Content = agentPrompt
            }
        });

        var sessionId = session.SessionId;
        _sessions[sessionId] = session;

        // Wire up event handlers
        session.On(evt =>
        {
            switch (evt)
            {
                case AssistantMessageDeltaEvent delta:
                    OnMessageReceived?.Invoke(sessionId, new ChatMessage
                    {
                        Role = "assistant",
                        Content = delta.Data.DeltaContent ?? "",
                        IsStreaming = true
                    });
                    break;

                case AssistantMessageEvent msg:
                    OnMessageReceived?.Invoke(sessionId, new ChatMessage
                    {
                        Role = "assistant",
                        Content = msg.Data.Content ?? "",
                        IsStreaming = false
                    });
                    break;

                case SessionIdleEvent:
                    _logger.LogDebug("Session {SessionId} is idle", sessionId);
                    OnSessionIdle?.Invoke(sessionId);
                    break;

                case SessionErrorEvent error:
                    _logger.LogError("Session {SessionId} error: {Message}", sessionId, error.Data.Message);
                    OnMessageReceived?.Invoke(sessionId, new ChatMessage
                    {
                        Role = "system",
                        Content = $"Error: {error.Data.Message}",
                        IsStreaming = false
                    });
                    break;
            }
        });

        _logger.LogInformation("Created session {SessionId} for phase {PhaseId}", sessionId, phase.Id);
        return sessionId;
    }

    /// <summary>
    /// Sends a user message to the specified session.
    /// Responses arrive via the OnMessageReceived event.
    /// </summary>
    public async Task SendMessageAsync(string sessionId, string message, CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"Session {sessionId} not found.");

        _logger.LogDebug("Sending message to session {SessionId}: {Message}", sessionId, message[..Math.Min(message.Length, 100)]);

        await session.SendAsync(new MessageOptions
        {
            Prompt = message
        }, cancellationToken);
    }

    /// <summary>
    /// Sends a message and waits for the complete response.
    /// </summary>
    public async Task SendAndWaitAsync(string sessionId, string message, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"Session {sessionId} not found.");

        _logger.LogDebug("SendAndWait to session {SessionId}: {Message}", sessionId, message[..Math.Min(message.Length, 100)]);

        await session.SendAndWaitAsync(new MessageOptions
        {
            Prompt = message
        }, timeout, cancellationToken);
    }

    /// <summary>
    /// Checks if a session exists and is active.
    /// </summary>
    public bool HasSession(string sessionId) => _sessions.ContainsKey(sessionId);

    public async ValueTask DisposeAsync()
    {
        foreach (var (id, session) in _sessions)
        {
            try
            {
                await session.DisposeAsync();
                _logger.LogDebug("Disposed session {SessionId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing session {SessionId}", id);
            }
        }
        _sessions.Clear();

        if (_client is not null)
        {
            try
            {
                await _client.StopAsync();
                await _client.DisposeAsync();
                _logger.LogInformation("CopilotClient stopped and disposed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing CopilotClient");
            }
            _client = null;
        }

        _initialized = false;
        _initLock.Dispose();
    }
}
