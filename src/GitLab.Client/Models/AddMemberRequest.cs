namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/members</c>.</summary>
public sealed record AddMemberRequest
{
    public required long UserId { get; init; }

    public required int AccessLevel { get; init; }

    public DateOnly? ExpiresAt { get; init; }
}