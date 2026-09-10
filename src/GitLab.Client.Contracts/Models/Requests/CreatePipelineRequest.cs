namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/pipeline</c>.</summary>
public sealed record CreatePipelineRequest
{
    public required string Ref { get; init; }

    /// <summary>
    ///     Variables made available to the pipeline. This is an array on this endpoint, unlike the object
    ///     form used by trigger-pipeline endpoints.
    /// </summary>
    public IReadOnlyList<GitLabPipelineVariableRequest>? Variables { get; init; }

    /// <summary>
    ///     CI/CD inputs to use when creating the pipeline, keyed by the input names declared in the CI/CD
    ///     configuration. Each value retains its declared JSON scalar or array shape.
    /// </summary>
    public IReadOnlyDictionary<string, GitLabPipelineInputValue>? Inputs { get; init; }
}