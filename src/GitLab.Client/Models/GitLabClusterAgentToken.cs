namespace GitLab.Client.Models;

/// <summary>
///     A registration token for a cluster agent (<c>/projects/:id/cluster_agents/:agent_id/tokens</c>),
///     the credential the in-cluster agent presents to authenticate back to GitLab.
///     <para>
///         Covers both shapes GitLab returns from this area: the listing
///         (<c>APIEntitiesClustersAgentTokenBasic</c>), which carries no <see cref="LastUsedAt" />, and
///         the single-token read (<c>APIEntitiesClustersAgentToken</c>), which does. Neither carries the
///         secret itself - GitLab discloses that exactly once, on creation, as
///         <see cref="GitLabClusterAgentTokenWithSecret" />.
///     </para>
/// </summary>
public sealed record GitLabClusterAgentToken
{
    public required long Id { get; init; }

    /// <summary>The human-readable name given to the token when it was created.</summary>
    public string? Name { get; init; }

    public string? Description { get; init; }

    /// <summary>The agent this token authenticates.</summary>
    public long? AgentId { get; init; }

    /// <summary>The token's lifecycle state, for example <c>active</c> or <c>revoked</c>.</summary>
    public string? Status { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The id of the user who created this token.</summary>
    public long? CreatedByUserId { get; init; }

    /// <summary>
    ///     When the token was last presented, or <see langword="null" /> if it never has been. Absent
    ///     entirely from the listing shape, which this type also models - so a listed token always reads
    ///     as <see langword="null" /> here regardless of its real last-used time.
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; init; }
}