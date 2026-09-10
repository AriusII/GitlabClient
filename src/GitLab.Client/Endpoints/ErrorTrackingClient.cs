using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ErrorTrackingClient(IGitLabApiConnection connection) : IErrorTrackingClient
{
    public IAsyncEnumerable<GitLabErrorTrackingClientKey> ListClientKeysAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("error_tracking")
                .Literal("client_keys").Build(),
            GitLabJsonContext.Default.GitLabErrorTrackingClientKeyArray,
            cancellationToken);
    }

    public Task<GitLabErrorTrackingClientKey> CreateClientKeyAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("error_tracking")
                .Literal("client_keys").Build(),
            GitLabJsonContext.Default.GitLabErrorTrackingClientKey,
            cancellationToken);
    }

    public Task<GitLabErrorTrackingClientKey> DeleteClientKeyAsync(ProjectId projectId, long keyId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("error_tracking")
                .Literal("client_keys").Segment(keyId).Build(),
            GitLabJsonContext.Default.GitLabErrorTrackingClientKey,
            cancellationToken);
    }

    public Task<GitLabErrorTrackingSettings> GetSettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("error_tracking").Literal("settings")
                .Build(),
            GitLabJsonContext.Default.GitLabErrorTrackingSettings,
            cancellationToken);
    }

    public Task<GitLabErrorTrackingSettings> UpdateSettingsAsync(ProjectId projectId,
        UpdateErrorTrackingSettingsRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("error_tracking").Literal("settings")
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateErrorTrackingSettingsRequest,
            GitLabJsonContext.Default.GitLabErrorTrackingSettings,
            cancellationToken);
    }

    public Task<GitLabErrorTrackingSettings> CreateSettingsAsync(ProjectId projectId,
        CreateErrorTrackingSettingsRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("error_tracking").Literal("settings")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateErrorTrackingSettingsRequest,
            GitLabJsonContext.Default.GitLabErrorTrackingSettings,
            cancellationToken);
    }
}