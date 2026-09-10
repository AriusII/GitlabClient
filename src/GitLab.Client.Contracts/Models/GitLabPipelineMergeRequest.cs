namespace GitLab.Client.Models;

/// <summary>
///     The merge-request projection GitLab includes in an instance-wide pipeline listing when the merge request is
///     visible to the caller.
/// </summary>
public sealed record GitLabPipelineMergeRequest
{
    public long? Iid { get; init; }

    public string? Title { get; init; }

    public Uri? WebUrl { get; init; }
}