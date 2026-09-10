namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>PATCH /service_accounts/:user_id</c>,
///     <c>PATCH /groups/:id/service_accounts/:user_id</c> and
///     <c>PATCH /projects/:id/service_accounts/:user_id</c>. Only the members that are set are sent, so an
///     omitted member leaves that attribute untouched.
///     <para>
///         Changing the email does not take effect immediately: GitLab records the new address as
///         <see cref="GitLabServiceAccount.UnconfirmedEmail" /> until it is confirmed.
///     </para>
/// </summary>
public sealed record UpdateServiceAccountRequest
{
    /// <summary>New display name for the service account.</summary>
    public string? Name { get; init; }

    /// <summary>New username for the service account. Must be unique across the instance.</summary>
    public string? Username { get; init; }

    /// <summary>New email address for the service account.</summary>
    public string? Email { get; init; }
}