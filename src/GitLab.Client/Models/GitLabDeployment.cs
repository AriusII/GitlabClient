namespace GitLab.Client.Models;

/// <summary>
///     A deployment of a ref to an environment, as returned by the GitLab Deployments API
///     (<c>/projects/:id/deployments</c>).
///     <para>
///         Models both <c>APIEntitiesDeployment</c> (the list shape) and
///         <c>APIEntitiesDeploymentExtended</c> (get/create/update), which is why
///         <see cref="PendingApprovalCount" /> - present only on the extended shape - is nullable.
///     </para>
/// </summary>
public sealed record GitLabDeployment
{
    public required long Id { get; init; }

    public long? Iid { get; init; }

    /// <summary>The branch or tag that was deployed.</summary>
    public string? Ref { get; init; }

    public string? Sha { get; init; }

    /// <summary>One of "created", "running", "success", "failed", "canceled", "skipped" or "blocked".</summary>
    public required string Status { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The user who triggered the deployment.</summary>
    public GitLabUser? User { get; init; }

    /// <summary>
    ///     The environment that was deployed to. GitLab embeds only the basic environment shape here, so
    ///     <see cref="GitLabEnvironment.State" /> is absent on a deployment.
    /// </summary>
    public GitLabEnvironment? Environment { get; init; }

    /// <summary>The CI job that performed the deployment, when one exists.</summary>
    public GitLabJob? Deployable { get; init; }

    /// <summary>
    ///     How many approvals a protected environment is still waiting for. Returned only by the
    ///     single-deployment endpoints (get, create, update), never by the list endpoint.
    /// </summary>
    public int? PendingApprovalCount { get; init; }
}