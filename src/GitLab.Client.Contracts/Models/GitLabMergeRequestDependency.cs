using GitLab.Client.Models.Responses;

namespace GitLab.Client.Models;

/// <summary>
///     A merge request dependency: the record saying one merge request blocks another
///     (<c>APIEntitiesMergeRequestDependency</c>, the <c>blocks</c>/<c>blockees</c> endpoints).
/// </summary>
/// <remarks>
///     Both sides are embedded as the full <c>MergeRequestBasic</c> entity, so
///     <see cref="GitLabMergeRequest" /> is reused rather than a leaner embedding type. Either side can be
///     absent when the caller cannot see the merge request on the other end of the dependency.
/// </remarks>
public sealed record GitLabMergeRequestDependency
{
    /// <summary>The dependency's own id - this is what <c>blocks/:block_id</c> addresses, not a merge request iid.</summary>
    public required long Id { get; init; }

    /// <summary>The merge request that must be merged first.</summary>
    public GitLabMergeRequest? BlockingMergeRequest { get; init; }

    /// <summary>The merge request being held back.</summary>
    public GitLabMergeRequest? BlockedMergeRequest { get; init; }

    public long? ProjectId { get; init; }
}