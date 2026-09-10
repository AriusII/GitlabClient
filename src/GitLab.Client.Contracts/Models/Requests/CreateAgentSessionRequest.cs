namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /projects/:id/ai_agent/sessions</c>.</summary>
public sealed record CreateAgentSessionRequest
{
    /// <summary>The agent that is running.</summary>
    public required GitLabAgentType AgentType { get; init; }

    /// <summary>The <see cref="GitLabAgentIdentity.Id" /> returned by identity registration.</summary>
    public required long AgentIdentityId { get; init; }

    /// <summary>How this session is being reported.</summary>
    public required GitLabAgentSyncType SyncType { get; init; }

    /// <summary>A human description of what the run is for.</summary>
    public string? Goal { get; init; }

    /// <summary>When the run actually began. Set it to backfill a session that finished before it was reported.</summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>
    ///     A client-generated UUID that makes a retry safe: GitLab returns the existing session instead of
    ///     opening a second one when this matches a session it already has.
    /// </summary>
    public string? IdempotencyKey { get; init; }
}