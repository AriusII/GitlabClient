using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed partial class IntegrationsRepository(IGitLabApiConnection connection) : IIntegrationsRepository
{
    /// <summary>
    ///     The one fixed path word this whole resource is built on. GitLab also serves every project
    ///     route below under the older <c>/services</c> spelling - byte-identical slugs, verbs and
    ///     schemas - and that alias is deliberately not wrapped: it is the same API under an
    ///     abandoned name, and wrapping it would double the surface for nothing.
    /// </summary>
    private const string Integrations = "integrations";

    public IAsyncEnumerable<GitLabIntegration> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Integrations).Build(),
            GitLabJsonContext.Default.GitLabIntegrationArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIntegration> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(Integrations).Build(),
            GitLabJsonContext.Default.GitLabIntegrationArray,
            cancellationToken);
    }

    public Task<GitLabIntegration> GetAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectIntegrationRoute(projectId, slug),
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> GetForGroupAsync(GroupId groupId, string slug,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupIntegrationRoute(groupId, slug),
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetAsync(ProjectId projectId, string slug,
        IReadOnlyDictionary<string, JsonElement> settings, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectIntegrationRoute(projectId, slug),
            settings,
            GitLabJsonContext.Default.IntegrationSettings,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetForGroupAsync(GroupId groupId, string slug,
        IReadOnlyDictionary<string, JsonElement> settings, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupIntegrationRoute(groupId, slug),
            settings,
            GitLabJsonContext.Default.IntegrationSettings,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetAsync<TSettings>(ProjectId projectId, string slug, TSettings settings,
        JsonTypeInfo<TSettings> settingsTypeInfo, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectIntegrationRoute(projectId, slug),
            settings,
            settingsTypeInfo,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetForGroupAsync<TSettings>(GroupId groupId, string slug, TSettings settings,
        JsonTypeInfo<TSettings> settingsTypeInfo, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupIntegrationRoute(groupId, slug),
            settings,
            settingsTypeInfo,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task DisableAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(ProjectIntegrationRoute(projectId, slug), cancellationToken);
    }

    public Task DisableForGroupAsync(GroupId groupId, string slug, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(GroupIntegrationRoute(groupId, slug), cancellationToken);
    }

    /// <summary>
    ///     The slug is caller-supplied text, so it goes through <c>Escaped</c> rather than <c>Literal</c>.
    ///     Every slug GitLab ships today is made of unreserved characters and survives unchanged, which is
    ///     exactly why this is easy to get wrong: <c>Literal</c> would look correct for all 51 of them and
    ///     then silently emit a broken path for the first slug that is not, or for the typo'd string a
    ///     caller passes by hand.
    /// </summary>
    private static Uri ProjectIntegrationRoute(ProjectId projectId, string slug)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Integrations).Escaped(slug).Build();
    }

    private static Uri GroupIntegrationRoute(GroupId groupId, string slug)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(Integrations).Escaped(slug).Build();
    }
}