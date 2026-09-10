namespace GitLab.Client.Models;

/// <summary>
///     A service account, as returned by the GitLab Service accounts API (<c>/service_accounts</c>,
///     <c>/groups/:id/service_accounts</c>, <c>/projects/:id/service_accounts</c>).
///     <para>
///         A service account is a machine user, so it shares the user id space: the <see cref="Id" /> here is
///         the <c>user_id</c> the service-account token routes on <c>IPersonalAccessTokensClient</c> take.
///         GitLab returns a narrower shape than a full user though - no <c>state</c>, <c>web_url</c> or
///         avatar - which is why this is its own record rather than <see cref="GitLabUser" />.
///     </para>
/// </summary>
public sealed record GitLabServiceAccount
{
    /// <summary>The service account's user id.</summary>
    public required long Id { get; init; }

    /// <summary>
    ///     The generated or supplied username. GitLab derives one of the form
    ///     <c>service_account_group_&lt;id&gt;_&lt;hash&gt;</c> when the create request omits it.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>The account's public email address, if one is set.</summary>
    public string? PublicEmail { get; init; }

    /// <summary>The display name. GitLab defaults it to "Service account user" when the create request omits it.</summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The account's confirmed email address. GitLab generates a <c>service_account_...@noreply....</c>
    ///     address when the create request omits one.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    ///     An email address that has been requested but not yet confirmed. An update that changes the email sets
    ///     this and leaves <see cref="Email" /> on the old address until the new one is confirmed.
    /// </summary>
    public string? UnconfirmedEmail { get; init; }
}