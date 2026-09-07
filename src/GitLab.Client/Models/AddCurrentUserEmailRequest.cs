namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /user/emails</c>.
///     <para>
///         Deliberately separate from the administrator form of the same operation
///         (<c>POST /users/:id/emails</c>): that one also takes <c>skip_confirmation</c>, which a user
///         adding an address to their own account is not allowed to set. GitLab always sends a confirmation
///         mail here, so the new address comes back with a null <see cref="GitLabEmail.ConfirmedAt" />.
///     </para>
/// </summary>
public sealed record AddCurrentUserEmailRequest
{
    /// <summary>The address to add. Must not already be in use anywhere on the instance.</summary>
    public required string Email { get; init; }
}