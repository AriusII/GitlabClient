namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/pipelines/:pipeline_id/metadata</c>. GitLab exposes exactly one
///     mutable piece of pipeline metadata, the display name.
/// </summary>
public sealed record UpdatePipelineMetadataRequest
{
    /// <summary>The name to show for the pipeline in the UI and in <c>GitLabPipeline.Name</c>.</summary>
    public required string Name { get; init; }
}