using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Integrations, sitting between the public
///     <c>IIntegrationsClient</c> controller and <c>IIntegrationsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
///     <para>
///         Declared <c>partial</c> for the same reason as the repository interface: the per-slug typed
///         setters arrive in sibling <c>IIntegrationsService.&lt;Area&gt;.cs</c> files and must stay in
///         lockstep with the other three declarations of the resource.
///     </para>
/// </summary>
internal partial interface IIntegrationsService
{
    IAsyncEnumerable<GitLabIntegration> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabIntegration> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> GetAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> GetForGroupAsync(GroupId groupId, string slug,
        CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetAsync(ProjectId projectId, string slug,
        IReadOnlyDictionary<string, JsonElement> settings, CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetForGroupAsync(GroupId groupId, string slug,
        IReadOnlyDictionary<string, JsonElement> settings, CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetAsync<TSettings>(ProjectId projectId, string slug, TSettings settings,
        JsonTypeInfo<TSettings> settingsTypeInfo, CancellationToken cancellationToken = default);

    Task<GitLabIntegration> SetForGroupAsync<TSettings>(GroupId groupId, string slug, TSettings settings,
        JsonTypeInfo<TSettings> settingsTypeInfo, CancellationToken cancellationToken = default);

    Task DisableAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);

    Task DisableForGroupAsync(GroupId groupId, string slug, CancellationToken cancellationToken = default);
}