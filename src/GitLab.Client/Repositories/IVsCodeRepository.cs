using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the VS Code Settings Sync resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IVsCodeService), typeof(IVsCodeClient))]
internal interface IVsCodeRepository
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