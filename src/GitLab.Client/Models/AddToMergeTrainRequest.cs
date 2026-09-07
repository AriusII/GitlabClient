namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/merge_trains/merge_requests/:merge_request_iid</c>.
///     <para>
///         Every member is optional; sending an empty body queues the merge request as-is. The spec's
///         fourth parameter, <c>when_pipeline_succeeds</c>, is not exposed: GitLab documents it as
///         deprecated in favour of <see cref="AutoMerge" />, which does the same thing.
///     </para>
/// </summary>
public sealed record AddToMergeTrainRequest
{
    /// <summary>
    ///     If given, must match the current HEAD of the source branch or GitLab refuses the merge. The guard
    ///     against queueing a revision someone has since pushed over.
    /// </summary>
    public string? Sha { get; init; }

    /// <summary>Squashes the merge request's commits into one when the car merges.</summary>
    public bool? Squash { get; init; }

    /// <summary>Sets the merge request to auto-merge once its pipeline succeeds.</summary>
    public bool? AutoMerge { get; init; }
}