namespace GitLab.Client.Models;

/// <summary>
///     One external agent session (<c>/projects/:id/ai_agent/sessions</c>) - a single run of a coding agent
///     against a project.
/// </summary>
public sealed record GitLabAgentSession
{
    public required long Id { get; init; }

    /// <summary>
    ///     The agent that ran. A string because GitLab types the response field as free text; the request
    ///     side is the typed <see cref="GitLabAgentType" />.
    /// </summary>
    public string? AgentType { get; init; }

    /// <summary>The <see cref="GitLabAgentIdentity.Id" /> this session was opened under.</summary>
    public long? AgentIdentityId { get; init; }

    /// <summary>The GitLab user the session belongs to.</summary>
    public long? UserId { get; init; }

    /// <summary>How the session reached GitLab - see <see cref="GitLabAgentSyncType" /> for the vocabulary.</summary>
    public string? SyncType { get; init; }

    /// <summary>Where the session is in its lifecycle - see <see cref="GitLabAgentSessionStatus" />.</summary>
    public string? Status { get; init; }

    /// <summary>The user-provided description of what the run was for.</summary>
    public string? Goal { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}