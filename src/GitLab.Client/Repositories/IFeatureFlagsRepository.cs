using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Feature flags resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IFeatureFlagsService), typeof(IFeatureFlagsClient))]
internal interface IFeatureFlagsRepository
{
    IAsyncEnumerable<GitLabFeatureFlag> ListAsync(ProjectId projectId, FeatureFlagListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabFeatureFlag> GetAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    Task<GitLabFeatureFlag> CreateAsync(ProjectId projectId, CreateFeatureFlagRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabFeatureFlag> UpdateAsync(ProjectId projectId, string name, UpdateFeatureFlagRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabFeatureFlag> DeleteAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    Task<GitLabFeatureFlagSettings> GetSettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabFeatureFlagSettings> UpdateSettingsAsync(ProjectId projectId,
        UpdateFeatureFlagSettingsRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabFeatureFlagUserList> ListUserListsAsync(ProjectId projectId,
        FeatureFlagUserListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabFeatureFlagUserList> GetUserListAsync(ProjectId projectId, long iid,
        CancellationToken cancellationToken = default);

    Task<GitLabFeatureFlagUserList> CreateUserListAsync(ProjectId projectId,
        CreateFeatureFlagUserListRequest request, CancellationToken cancellationToken = default);

    Task<GitLabFeatureFlagUserList> UpdateUserListAsync(ProjectId projectId, long iid,
        UpdateFeatureFlagUserListRequest request, CancellationToken cancellationToken = default);

    Task DeleteUserListAsync(ProjectId projectId, long iid, CancellationToken cancellationToken = default);
}