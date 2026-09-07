namespace GitLab.Client.Models;

/// <summary>Request body for <c>PUT /projects/:id/members/:userId</c>.</summary>
public sealed record UpdateMemberRequest
{
    public required int AccessLevel { get; init; }

    public DateOnly? ExpiresAt { get; init; }
}