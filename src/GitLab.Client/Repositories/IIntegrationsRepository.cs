using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Integrations resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         <b>Deliberately <c>partial</c>, and split by area.</b> The Integrations tag is the largest in
///         the spec, and the bulk of it is one <c>PUT</c> per integration slug. The generic core lives
///         here; typed, per-slug setters live in sibling partial files named
///         <c>IIntegrationsRepository.&lt;Area&gt;.cs</c> (see <c>IIntegrationsRepository.Slack.cs</c>
///         for the shape). All four declarations of the resource - this interface,
///         <see cref="IntegrationsRepository" />, <see cref="IIntegrationsService" /> and
///         <see cref="IIntegrationsClient" /> - are partial and must be extended in lockstep, or the
///         layer generator reports GLC0003 naming the member that drifted.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IIntegrationsService), typeof(IIntegrationsClient))]
internal partial interface IIntegrationsRepository
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