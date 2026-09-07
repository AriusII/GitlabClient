using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Feature flags" API area - a project's own feature flags
///     (<c>/projects/:id/feature_flags</c>), the minimum role allowed to change them
///     (<c>/projects/:id/feature_flags_settings</c>) and the user lists a <c>gitlabUserList</c> strategy
///     targets (<c>/projects/:id/feature_flags_user_lists</c>).
///     <para>
///         These are the flags a project rolls out to its own users and serves to Unleash clients. They
///         are unrelated to <see cref="IFeaturesClient" />, which is the instance-wide Flipper surface
///         GitLab develops itself behind.
///     </para>
///     <para>
///         Every endpoint here needs at least the Developer role, and the project's own
///         <see cref="GitLabFeatureFlagSettings.MinimumRole" /> can raise that bar further for the write
///         verbs.
///     </para>
/// </summary>
public interface IFeatureFlagsClient
{
    /// <summary>Streams every feature flag on a project, newest first.</summary>
    IAsyncEnumerable<GitLabFeatureFlag> ListAsync(ProjectId projectId, FeatureFlagListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one feature flag by name. Flag names may legally contain <c>.</c> and <c>/</c>; the route
    ///     builder percent-encodes the name, so pass it raw.
    /// </summary>
    Task<GitLabFeatureFlag> GetAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>Creates a feature flag, optionally with its strategies already attached.</summary>
    Task<GitLabFeatureFlag> CreateAsync(ProjectId projectId, CreateFeatureFlagRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a feature flag - renames it, toggles it, or adds, edits and deletes strategies. The route
    ///     takes the flag's current name even when <see cref="UpdateFeatureFlagRequest.Name" /> renames it.
    /// </summary>
    Task<GitLabFeatureFlag> UpdateAsync(ProjectId projectId, string name, UpdateFeatureFlagRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a feature flag and returns the flag that was removed - this endpoint answers <c>200</c>
    ///     with a body rather than <c>204</c>.
    /// </summary>
    Task<GitLabFeatureFlag> DeleteAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the minimum role allowed to change the project's feature flags.</summary>
    Task<GitLabFeatureFlagSettings> GetSettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets the minimum role allowed to change the project's feature flags. Raising it above your own
    ///     role locks you out of the write verbs above.
    /// </summary>
    Task<GitLabFeatureFlagSettings> UpdateSettingsAsync(ProjectId projectId,
        UpdateFeatureFlagSettingsRequest request, CancellationToken cancellationToken = default);

    /// <summary>Streams every feature flag user list on a project.</summary>
    IAsyncEnumerable<GitLabFeatureFlagUserList> ListUserListsAsync(ProjectId projectId,
        FeatureFlagUserListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one user list by its internal ID. That is
    ///     <see cref="GitLabFeatureFlagUserList.Iid" />, not <see cref="GitLabFeatureFlagUserList.Id" />.
    /// </summary>
    Task<GitLabFeatureFlagUserList> GetUserListAsync(ProjectId projectId, long iid,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a user list from a comma-separated set of external user IDs.</summary>
    Task<GitLabFeatureFlagUserList> CreateUserListAsync(ProjectId projectId,
        CreateFeatureFlagUserListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Renames a user list or replaces its members wholesale.
    ///     <see cref="UpdateFeatureFlagUserListRequest.UserXids" /> is not an append.
    /// </summary>
    Task<GitLabFeatureFlagUserList> UpdateUserListAsync(ProjectId projectId, long iid,
        UpdateFeatureFlagUserListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a user list. GitLab refuses with <c>409</c> while a <c>gitlabUserList</c> strategy still
    ///     references it.
    /// </summary>
    Task DeleteUserListAsync(ProjectId projectId, long iid, CancellationToken cancellationToken = default);
}