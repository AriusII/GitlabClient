namespace GitLab.Client.Models;

/// <summary>
///     The lean merge request GitLab embeds in a <see cref="GitLabMergeTrainCar" />
///     (<c>APIEntitiesMergeRequestSimple</c> in the spec).
///     <para>
///         This is deliberately not <see cref="GitLabMergeRequest" />: the embedded shape omits
///         <c>source_branch</c>, <c>target_branch</c> and the merge status, all of which
///         <see cref="GitLabMergeRequest" /> marks <c>required</c>, so reusing that type here would fail
///         deserialization on every merge train response. Fetch the full merge request through
///         <c>IMergeRequestsClient</c> when more than these fields is needed.
///     </para>
/// </summary>
public sealed record GitLabMergeTrainMergeRequest
{
    public required long Id { get; init; }

    /// <summary>The project-scoped number the merge train endpoints address a merge request by.</summary>
    public required long Iid { get; init; }

    public long? ProjectId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public string? State { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public Uri? WebUrl { get; init; }
}