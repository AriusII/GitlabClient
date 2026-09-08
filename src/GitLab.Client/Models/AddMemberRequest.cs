namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/members</c>. Exactly one of <see cref="UserId" /> and
///     <see cref="Username" /> must be set - GitLab rejects the call when both or neither are present.
/// </summary>
public sealed record AddMemberRequest
{
    /// <summary>
    ///     The numeric ID of the user to add, or several separated by commas. Mutually exclusive with
    ///     <see cref="Username" />.
    /// </summary>
    public long? UserId { get; init; }

    public required int AccessLevel { get; init; }

    /// <summary>
    ///     The username of the user to add, or several separated by commas. Mutually exclusive with
    ///     <see cref="UserId" />.
    /// </summary>
    public string? Username { get; init; }

    public DateOnly? ExpiresAt { get; init; }

    /// <summary>Free-text label for whatever triggered the addition; GitLab records it for analytics.</summary>
    public string? InviteSource { get; init; }
}