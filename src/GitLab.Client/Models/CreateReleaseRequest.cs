namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/releases</c>.</summary>
public sealed record CreateReleaseRequest
{
    public required string TagName { get; init; }

    public string? Ref { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }
}