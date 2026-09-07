namespace GitLab.Client.Models;

/// <summary>The body of <c>POST /projects/:id/merge_requests/:merge_request_iid/pipelines</c>.</summary>
public sealed record CreateMergeRequestPipelineRequest
{
    /// <summary>
    ///     Creates the pipeline in the background and answers immediately, with a pipeline whose jobs have
    ///     not been created yet.
    /// </summary>
    public bool? Async { get; init; }
}