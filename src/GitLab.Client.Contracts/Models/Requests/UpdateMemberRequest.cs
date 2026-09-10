namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>PUT /projects/:id/members/:userId</c>.</summary>
public sealed record UpdateMemberRequest
{
    public required int AccessLevel { get; init; }

    public DateOnly? ExpiresAt { get; init; }

    /// <summary>The ID of a custom member role to attach to the membership.</summary>
    public long? MemberRoleId { get; init; }
}