using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Body of <c>PATCH /projects/:id/ai_agent/sessions/:session_id</c>.</summary>
public sealed record CompleteAgentSessionRequest
{
    /// <summary>The terminal status to move the session to.</summary>
    public required GitLabAgentSessionOutcome Status { get; init; }

    /// <summary>
    ///     SHA-256 of the session transcript. Sending the value GitLab already stored is a no-op that
    ///     returns the session unchanged, so this doubles as the retry key for the completion call.
    /// </summary>
    [JsonPropertyName("jsonl_sha256")]
    public string? JsonlSha256 { get; init; }
}