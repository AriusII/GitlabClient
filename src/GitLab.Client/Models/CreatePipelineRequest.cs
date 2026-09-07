namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/pipeline</c>.</summary>
public sealed record CreatePipelineRequest
{
    public required string Ref { get; init; }
}