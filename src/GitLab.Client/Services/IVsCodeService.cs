using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for VS Code Settings Sync, sitting between the public
///     <c>IVsCodeClient</c> controller and <c>IVsCodeRepository</c>'s raw GitLab access. Mirrors the
///     repository's method shapes 1:1 today (its implementation is generated); this is the seam where
///     request validation, caching, or cross-resource composition would go once the resource needs more
///     than pass-through.
/// </summary>
internal interface IVsCodeService
{
    Task<GitLabVsCodeSettingsManifest> GetManifestAsync(string? settingsContextHash = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabVsCodeSettingReference> ListReferencesAsync(GitLabVsCodeSettingResource resourceName,
        string? settingsContextHash = null, CancellationToken cancellationToken = default);

    Task<GitLabVsCodeSetting> GetSettingAsync(GitLabVsCodeSettingResource resourceName, string id,
        string? settingsContextHash = null, CancellationToken cancellationToken = default);

    Task CreateOrUpdateAsync(GitLabVsCodeSettingResource resourceName, string? settingsContextHash = null,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(string? settingsContextHash = null, CancellationToken cancellationToken = default);
}