namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /service_accounts</c>, <c>POST /groups/:id/service_accounts</c> and
///     <c>POST /projects/:id/service_accounts</c>. Every member is optional: GitLab generates a name, a
///     username and a no-reply email address for the ones the caller leaves out.
/// </summary>
public sealed record CreateServiceAccountRequest
{
    /// <summary>Display name of the service account.</summary>
    public string? Name { get; init; }

    /// <summary>Username of the service account. Must be unique across the instance.</summary>
    public string? Username { get; init; }

    /// <summary>Custom email address for the service account.</summary>
    public string? Email { get; init; }
}