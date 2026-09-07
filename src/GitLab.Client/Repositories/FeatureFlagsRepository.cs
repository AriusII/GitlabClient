using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class FeatureFlagsRepository(IGitLabApiConnection connection) : IFeatureFlagsRepository
{
    public IAsyncEnumerable<GitLabFeatureFlag> ListAsync(ProjectId projectId, FeatureFlagListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags").QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabFeatureFlagArray,
            cancellationToken);
    }

    public Task<GitLabFeatureFlag> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags").Escaped(name).Build(),
            GitLabJsonContext.Default.GitLabFeatureFlag,
            cancellationToken);
    }

    public Task<GitLabFeatureFlag> CreateAsync(ProjectId projectId, CreateFeatureFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags").Build(),
            request,
            GitLabJsonContext.Default.CreateFeatureFlagRequest,
            GitLabJsonContext.Default.GitLabFeatureFlag,
            cancellationToken);
    }

    public Task<GitLabFeatureFlag> UpdateAsync(ProjectId projectId, string name, UpdateFeatureFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags").Escaped(name).Build(),
            request,
            GitLabJsonContext.Default.UpdateFeatureFlagRequest,
            GitLabJsonContext.Default.GitLabFeatureFlag,
            cancellationToken);
    }

    // Deleting a flag answers 200 with the flag that was removed rather than 204, so the deserializing
    // DELETE overload is the right one here.
    public Task<GitLabFeatureFlag> DeleteAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags").Escaped(name).Build(),
            GitLabJsonContext.Default.GitLabFeatureFlag,
            cancellationToken);
    }

    public Task<GitLabFeatureFlagSettings> GetSettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags_settings").Build(),
            GitLabJsonContext.Default.GitLabFeatureFlagSettings,
            cancellationToken);
    }

    public Task<GitLabFeatureFlagSettings> UpdateSettingsAsync(ProjectId projectId,
        UpdateFeatureFlagSettingsRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags_settings").Build(),
            request,
            GitLabJsonContext.Default.UpdateFeatureFlagSettingsRequest,
            GitLabJsonContext.Default.GitLabFeatureFlagSettings,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabFeatureFlagUserList> ListUserListsAsync(ProjectId projectId,
        FeatureFlagUserListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags_user_lists")
                .QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabFeatureFlagUserListArray,
            cancellationToken);
    }

    public Task<GitLabFeatureFlagUserList> GetUserListAsync(ProjectId projectId, long iid,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags_user_lists").Segment(iid)
                .Build(),
            GitLabJsonContext.Default.GitLabFeatureFlagUserList,
            cancellationToken);
    }

    public Task<GitLabFeatureFlagUserList> CreateUserListAsync(ProjectId projectId,
        CreateFeatureFlagUserListRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags_user_lists").Build(),
            request,
            GitLabJsonContext.Default.CreateFeatureFlagUserListRequest,
            GitLabJsonContext.Default.GitLabFeatureFlagUserList,
            cancellationToken);
    }

    public Task<GitLabFeatureFlagUserList> UpdateUserListAsync(ProjectId projectId, long iid,
        UpdateFeatureFlagUserListRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags_user_lists").Segment(iid)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateFeatureFlagUserListRequest,
            GitLabJsonContext.Default.GitLabFeatureFlagUserList,
            cancellationToken);
    }

    public Task DeleteUserListAsync(ProjectId projectId, long iid, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("feature_flags_user_lists").Segment(iid)
                .Build(),
            cancellationToken);
    }
}