namespace GitLab.Client.Models;

/// <summary>
///     A newly created cluster agent token together with its plaintext secret, as returned by
///     <c>POST /projects/:id/cluster_agents/:agent_id/tokens</c> (the wire's
///     <c>APIEntitiesClustersAgentTokenWithToken</c>) - the only moment GitLab ever discloses it.
///     <para>
///         <see cref="Token" /> cannot be retrieved afterwards: a value read here must be persisted
///         immediately or the token is useless and has to be recreated. Every other read of a token
///         returns the secret-free <see cref="GitLabClusterAgentToken" /> instead.
///     </para>
///     <para>
///         Treat an instance of this type as a credential - do not log it, do not put it in an exception
///         message, and do not hand it to a generic object dumper. <see cref="ToString" /> is overridden
///         here for exactly that reason.
///     </para>
/// </summary>
public sealed record GitLabClusterAgentTokenWithSecret
{
    public required long Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    public long? AgentId { get; init; }

    public string? Status { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public long? CreatedByUserId { get; init; }

    public DateTimeOffset? LastUsedAt { get; init; }

    /// <summary>The freshly minted secret. Store it now; GitLab cannot show it again.</summary>
    public required string Token { get; init; }

    /// <summary>
    ///     Renders the token without its secret. Record types print every member from the
    ///     compiler-generated <c>ToString()</c>, which is exactly how a credential ends up in a log line;
    ///     this override keeps <see cref="Token" /> out of it.
    /// </summary>
    public override string ToString()
    {
        return
            $"GitLabClusterAgentTokenWithSecret {{ Id = {Id}, Name = {Name}, AgentId = {AgentId}, Token = <redacted> }}";
    }
}