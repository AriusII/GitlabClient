using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class ProjectsRepository(IGitLabApiConnection connection) : IProjectsRepository
{
    public Task<GitLabProject> GetAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Build(),
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListAsync(ProjectListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public Task<GitLabProject> CreateAsync(CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Build(),
            request,
            GitLabJsonContext.Default.CreateProjectRequest,
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task<GitLabProject> CreateForUserAsync(long userId, CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Literal("user").Segment(userId).Build(),
            request,
            GitLabJsonContext.Default.CreateProjectRequest,
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task<GitLabProject> UpdateAsync(ProjectId projectId, UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Build(),
            request,
            GitLabJsonContext.Default.UpdateProjectRequest,
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task<GitLabProject> SetAvatarAsync(ProjectId projectId, GitLabFileUpload avatar,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(avatar);

        // The avatar is the one field of the update body that cannot travel as JSON, and GitLab expects it
        // under the form field name "avatar" rather than the usual "file" - pinned here so a caller cannot
        // get a silent no-op by leaving GitLabFileUpload.FieldName at its default.
        return connection.PutFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Build(),
            avatar with { FieldName = "avatar" },
            null,
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Build(),
            cancellationToken);
    }

    public Task<GitLabProject> RestoreAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("restore").Build(),
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task<GitLabProject> ArchiveAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("archive").Build(),
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task<GitLabProject> UnarchiveAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("unarchive").Build(),
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task<GitLabProject> StarAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("star").Build(),
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task<GitLabProject> UnstarAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("unstar").Build(),
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProjectStarrer> ListStarrersAsync(ProjectId projectId,
        ProjectStarrerListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("starrers")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectStarrerArray,
            cancellationToken);
    }

    public Task<GitLabProject> ForkAsync(ProjectId projectId, ForkProjectRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        // Every member of the body is optional and forking with none of them set is the common case, so a
        // null request is sent as an empty object rather than rejected.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("fork").Build(),
            request ?? new ForkProjectRequest(),
            GitLabJsonContext.Default.ForkProjectRequest,
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListForksAsync(ProjectId projectId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("forks")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public Task<GitLabProject> CreateForkRelationshipAsync(ProjectId projectId, ProjectId forkedFromId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("fork").Segment(forkedFromId).Build(),
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public Task DeleteForkRelationshipAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("fork").Build(),
            cancellationToken);
    }

    public Task<GitLabProject> TransferAsync(ProjectId projectId, TransferProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("transfer").Build(),
            request,
            GitLabJsonContext.Default.TransferProjectRequest,
            GitLabJsonContext.Default.GitLabProject,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPublicGroupDetails> ListTransferLocationsAsync(ProjectId projectId,
        ProjectTransferLocationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("transfer_locations")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabPublicGroupDetailsArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabPublicGroupDetails> ListAncestorGroupsAsync(ProjectId projectId,
        ProjectAncestorGroupListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("groups")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabPublicGroupDetailsArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroup> ListInvitedGroupsAsync(ProjectId projectId,
        ProjectInvitedGroupListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("invited_groups")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroup> ListShareLocationsAsync(ProjectId projectId,
        ProjectShareLocationListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("share_locations")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupArray,
            cancellationToken);
    }

    public Task<GitLabProjectGroupLink> ShareAsync(ProjectId projectId, ShareProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("share").Build(),
            request,
            GitLabJsonContext.Default.ShareProjectRequest,
            GitLabJsonContext.Default.GitLabProjectGroupLink,
            cancellationToken);
    }

    public Task UnshareAsync(ProjectId projectId, long groupId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("share").Segment(groupId).Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabUser> ListUsersAsync(ProjectId projectId, ProjectUserListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("users")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabUserArray,
            cancellationToken);
    }

    public Task ImportMembersAsync(ProjectId projectId, long sourceProjectId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId)
                .Literal("import_project_members").Segment(sourceProjectId)
                .Build(),
            cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, double>> GetLanguagesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("languages").Build(),
            GitLabJsonContext.Default.ProjectLanguages,
            cancellationToken);
    }

    public Task<GitLabProjectDailyStatistics> GetStatisticsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("statistics").Build(),
            GitLabJsonContext.Default.GitLabProjectDailyStatistics,
            cancellationToken);
    }

    public Task<GitLabProjectStorage> GetStorageAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("storage").Build(),
            GitLabJsonContext.Default.GitLabProjectStorage,
            cancellationToken);
    }

    public Task StartHousekeepingAsync(ProjectId projectId, ProjectHousekeepingRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        // An empty body leaves the task unset, which is how GitLab is asked for whichever housekeeping run
        // the project is due.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("housekeeping").Build(),
            request ?? new ProjectHousekeepingRequest(),
            GitLabJsonContext.Default.ProjectHousekeepingRequest,
            cancellationToken);
    }

    public Task RecalculateRepositorySizeAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("repository_size").Build(),
            cancellationToken);
    }

    public Task<GitLabProjectSecuritySettings> GetSecuritySettingsAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("security_settings").Build(),
            GitLabJsonContext.Default.GitLabProjectSecuritySettings,
            cancellationToken);
    }

    public Task<GitLabProjectSecuritySettings> UpdateSecuritySettingsAsync(ProjectId projectId,
        UpdateProjectSecuritySettingsRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("security_settings").Build(),
            request,
            GitLabJsonContext.Default.UpdateProjectSecuritySettingsRequest,
            GitLabJsonContext.Default.GitLabProjectSecuritySettings,
            cancellationToken);
    }

    public Task<GitLabMergeRequest> CreateCiConfigMergeRequestAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("create_ci_config").Build(),
            GitLabJsonContext.Default.GitLabMergeRequest,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProjectAuditEvent> ListAuditEventsAsync(ProjectId projectId,
        ProjectAuditEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("audit_events")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectAuditEventArray,
            cancellationToken);
    }

    public Task<GitLabProjectAuditEvent> GetAuditEventAsync(ProjectId projectId, long auditEventId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("audit_events").Segment(auditEventId)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectAuditEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListForUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users").Escaped(userId).Literal("projects")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListStarredByUserAsync(string userId, ProjectListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users").Escaped(userId).Literal("starred_projects")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabProject> ListContributedByUserAsync(string userId,
        ProjectListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("users").Escaped(userId).Literal("contributed_projects")
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabProjectArray,
            cancellationToken);
    }
}