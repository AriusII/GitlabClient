using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Feature flags, sitting between the public
///     <c>IFeatureFlagsClient</c> controller and <c>IFeatureFlagsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IFeatureFlagsService
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