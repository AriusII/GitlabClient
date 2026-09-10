using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class VsCodeClient(IGitLabApiConnection connection) : IVsCodeClient
{
    public Task<GitLabVsCodeSettingsManifest> GetManifestAsync(string? settingsContextHash = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            SettingsSync(settingsContextHash).Literal("manifest").Build(),
            GitLabJsonContext.Default.GitLabVsCodeSettingsManifest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabVsCodeSettingReference> ListReferencesAsync(
        GitLabVsCodeSettingResource resourceName, string? settingsContextHash = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            Resource(settingsContextHash, resourceName).Build(),
            GitLabJsonContext.Default.GitLabVsCodeSettingReferenceArray,
            cancellationToken);
    }

    public Task<GitLabVsCodeSetting> GetSettingAsync(GitLabVsCodeSettingResource resourceName, string id,
        string? settingsContextHash = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            Resource(settingsContextHash, resourceName).Escaped(id).Build(),
            GitLabJsonContext.Default.GitLabVsCodeSetting,
            cancellationToken);
    }

    public Task CreateOrUpdateAsync(GitLabVsCodeSettingResource resourceName, string? settingsContextHash = null,
        CancellationToken cancellationToken = default)
    {
        // The spec declares no request body for this operation, so none is sent. See IVsCodeClient.
        return connection.PostAsync(
            Resource(settingsContextHash, resourceName).Build(),
            cancellationToken);
    }

    public Task DeleteCollectionAsync(string? settingsContextHash = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            SettingsSync(settingsContextHash).Literal("collection").Build(),
            cancellationToken);
    }

    /// <summary>
    ///     Every Settings Sync route exists twice in the spec - once user-global and once with a
    ///     <c>{settings_context_hash}</c> segment between <c>settings_sync</c> and <c>v1</c>. The two
    ///     families are otherwise identical, so they share this builder instead of being written out twice.
    /// </summary>
    private static GitLabRouteBuilder SettingsSync(string? settingsContextHash)
    {
        GitLabRouteBuilder route = GitLabRouteBuilder.Create("vscode").Literal("settings_sync");

        if (!string.IsNullOrWhiteSpace(settingsContextHash))
        {
            route = route.Escaped(settingsContextHash);
        }

        return route.Literal("v1");
    }

    private static GitLabRouteBuilder Resource(string? settingsContextHash,
        GitLabVsCodeSettingResource resourceName)
    {
        return SettingsSync(settingsContextHash).Literal("resource").Literal(resourceName.ToRouteValue());
    }
}