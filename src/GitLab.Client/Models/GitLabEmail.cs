namespace GitLab.Client.Models;

/// <summary>
///     A secondary email address on a user account, as returned by the GitLab User emails API
///     (<c>/user/emails</c> and <c>/users/:id/emails</c>).
///     <para>
///         The primary email address is not part of this collection: it is a property of the user, is
///         returned by <c>GET /user</c>, and cannot be deleted here.
///     </para>
/// </summary>
public sealed record GitLabEmail
{
    /// <summary>The email's own id, which is what the <c>/user/emails/:email_id</c> routes take - not the user's id.</summary>
    public required long Id { get; init; }

    public required string Email { get; init; }

    /// <summary>
    ///     When the address was confirmed, or <see langword="null" /> while confirmation is still pending.
    ///     An unconfirmed address receives no notifications and cannot be used to sign in.
    /// </summary>
    public DateTimeOffset? ConfirmedAt { get; init; }
}