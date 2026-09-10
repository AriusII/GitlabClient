using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     The answer to <c>PUT /license/:id/refresh_billable_users</c>, which GitLab models with its generic
///     <c>APIEntitiesBasicSuccess</c> envelope.
/// </summary>
public sealed record GitLabLicenseRefreshResult
{
    /// <summary>
    ///     Whether the recalculation was scheduled. GitLab declares the member as a bare object rather than
    ///     a boolean, so it is kept as a raw <see cref="JsonElement" />; in practice it is <c>true</c>.
    /// </summary>
    public JsonElement? Success { get; init; }
}