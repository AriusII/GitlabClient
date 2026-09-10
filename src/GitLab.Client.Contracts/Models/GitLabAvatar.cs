namespace GitLab.Client.Models;

/// <summary>
///     An avatar location, as returned by <c>GET /avatar</c> (look up a user's avatar by public email) and
///     by <c>PUT /user/avatar</c> (upload the authenticated user's avatar).
///     <para>
///         For a user with no GitLab account - or none whose public email matches - GitLab still answers
///         <c>200</c>, with the Gravatar (or configured avatar-service) URL derived from the email address,
///         so a non-null <see cref="AvatarUrl" /> is not evidence that the user exists.
///     </para>
/// </summary>
public sealed record GitLabAvatar
{
    /// <summary>Where the avatar image can be fetched from.</summary>
    public Uri? AvatarUrl { get; init; }
}