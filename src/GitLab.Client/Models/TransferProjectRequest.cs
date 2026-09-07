namespace GitLab.Client.Models;

/// <summary>Request body for <c>PUT /projects/:id/transfer</c>.</summary>
public sealed record TransferProjectRequest
{
    /// <summary>The ID or path of the namespace to move the project into.</summary>
    public required string Namespace { get; init; }
}