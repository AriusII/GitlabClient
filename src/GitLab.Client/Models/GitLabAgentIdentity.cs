namespace GitLab.Client.Models;

/// <summary>
///     An external agent identity (<c>/projects/:id/ai_agent/identities</c>) - the registration that ties
///     one agent running on one machine to one user and project.
/// </summary>
public sealed record GitLabAgentIdentity
{
    public required long Id { get; init; }

    /// <summary>
    ///     The agent this identity was registered for. A string because GitLab types the response field as
    ///     free text; the request side is the typed <see cref="GitLabAgentType" />.
    /// </summary>
    public string? AgentType { get; init; }

    /// <summary>
    ///     When the identity was revoked, or null while it is still usable. Registering against a revoked
    ///     identity answers <c>403</c>.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}