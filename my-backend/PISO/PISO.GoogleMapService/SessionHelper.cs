using System.Text.Json;
using PISO.Contracts;

namespace PISO.GoogleMapService;

public class SessionHelper
{
    private readonly GoogleSession[] _sessions;

    public SessionHelper(string sessionsFilePath, ILoggerManager logger)
    {
        if (string.IsNullOrWhiteSpace(sessionsFilePath) || !File.Exists(sessionsFilePath))
            throw new FileNotFoundException($"Google sessions file not found: {sessionsFilePath}");

        var json = File.ReadAllText(sessionsFilePath);
        _sessions = JsonSerializer.Deserialize<GoogleSession[]>(json)
                    ?? throw new InvalidOperationException($"Failed to parse sessions file: {sessionsFilePath}");

        if (_sessions.Length == 0)
            throw new InvalidOperationException($"No sessions found in {sessionsFilePath}");

        logger.LogInfo($"Loaded {_sessions.Length} sessions from {sessionsFilePath}");
    }

    public GoogleSession GetRandomSession() =>
        _sessions[Random.Shared.Next(_sessions.Length)];
}
