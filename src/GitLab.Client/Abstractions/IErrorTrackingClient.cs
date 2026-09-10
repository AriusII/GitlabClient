using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Error tracking" API area (<c>/projects/:id/error_tracking</c>) - a project's
///     Sentry (or Sentry-compatible) integration settings, and the client keys used to authenticate
///     error submissions into it.
/// </summary>
public interface IErrorTrackingClient
{
    /// <summary>Streams every client key configured for integrated error tracking on the project.</summary>
    IAsyncEnumerable<GitLabErrorTrackingClientKey> ListClientKeysAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a client key. GitLab generates <see cref="GitLabErrorTrackingClientKey.PublicKey" /> automatically;
    ///     the request body is empty.
    /// </summary>
    Task<GitLabErrorTrackingClientKey> CreateClientKeyAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a client key, returning it as it was just before deletion.</summary>
    Task<GitLabErrorTrackingClientKey> DeleteClientKeyAsync(ProjectId projectId, long keyId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the project's Error Tracking settings.</summary>
    Task<GitLabErrorTrackingSettings> GetSettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Partially updates the project's Error Tracking settings. Requires the Maintainer or Owner role.
    ///     Unlike <see cref="CreateSettingsAsync" />, <see cref="UpdateErrorTrackingSettingsRequest.Integrated" />
    ///     is optional here.
    /// </summary>
    Task<GitLabErrorTrackingSettings> UpdateSettingsAsync(ProjectId projectId,
        UpdateErrorTrackingSettingsRequest request, CancellationToken cancellationToken = default);

    /// <summary>Creates the project's Error Tracking settings. Requires the Maintainer or Owner role.</summary>
    Task<GitLabErrorTrackingSettings> CreateSettingsAsync(ProjectId projectId,
        CreateErrorTrackingSettingsRequest request, CancellationToken cancellationToken = default);
}