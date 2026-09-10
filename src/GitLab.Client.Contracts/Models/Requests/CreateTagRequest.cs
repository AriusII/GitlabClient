namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/repository/tags</c>.</summary>
public sealed record CreateTagRequest
{
    public required string TagName { get; init; }

    public required string Ref { get; init; }

    public string? Message { get; init; }
}