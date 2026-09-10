using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

/// <remarks>
///     Three parents, one endpoint shape. The twelve public methods are deliberately thin wrappers over
///     four verb helpers plus one route helper per parent, so the encoding rule that actually matters -
///     the attribute key is caller-supplied free text and therefore <c>Escaped</c>, never <c>Literal</c> -
///     is written down exactly once instead of twelve times.
/// </remarks>
internal sealed class CustomAttributesClient(IGitLabApiConnection connection) : ICustomAttributesClient
{
    public IAsyncEnumerable<GitLabCustomAttribute> ListForUserAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        return List(UserAttributes(userId), cancellationToken);
    }

    public IAsyncEnumerable<GitLabCustomAttribute> ListForUserAsync(string userIdOrUsername,
        CancellationToken cancellationToken = default)
    {
        return List(UserAttributes(userIdOrUsername), cancellationToken);
    }

    public Task<GitLabCustomAttribute> GetForUserAsync(long userId, string key,
        CancellationToken cancellationToken = default)
    {
        return Get(UserAttributes(userId), key, cancellationToken);
    }

    public Task<GitLabCustomAttribute> GetForUserAsync(string userIdOrUsername, string key,
        CancellationToken cancellationToken = default)
    {
        return Get(UserAttributes(userIdOrUsername), key, cancellationToken);
    }

    public Task<GitLabCustomAttribute> SetForUserAsync(long userId, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken = default)
    {
        return Set(UserAttributes(userId), key, request, cancellationToken);
    }

    public Task<GitLabCustomAttribute> SetForUserAsync(string userIdOrUsername, string key,
        SetCustomAttributeRequest request, CancellationToken cancellationToken = default)
    {
        return Set(UserAttributes(userIdOrUsername), key, request, cancellationToken);
    }

    public Task DeleteForUserAsync(long userId, string key, CancellationToken cancellationToken = default)
    {
        return Delete(UserAttributes(userId), key, cancellationToken);
    }

    public Task DeleteForUserAsync(string userIdOrUsername, string key,
        CancellationToken cancellationToken = default)
    {
        return Delete(UserAttributes(userIdOrUsername), key, cancellationToken);
    }

    public IAsyncEnumerable<GitLabCustomAttribute> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return List(GroupAttributes(groupId), cancellationToken);
    }

    public Task<GitLabCustomAttribute> GetForGroupAsync(GroupId groupId, string key,
        CancellationToken cancellationToken = default)
    {
        return Get(GroupAttributes(groupId), key, cancellationToken);
    }

    public Task<GitLabCustomAttribute> SetForGroupAsync(GroupId groupId, string key,
        SetCustomAttributeRequest request, CancellationToken cancellationToken = default)
    {
        return Set(GroupAttributes(groupId), key, request, cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, string key, CancellationToken cancellationToken = default)
    {
        return Delete(GroupAttributes(groupId), key, cancellationToken);
    }

    public IAsyncEnumerable<GitLabCustomAttribute> ListForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return List(ProjectAttributes(projectId), cancellationToken);
    }

    public Task<GitLabCustomAttribute> GetForProjectAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default)
    {
        return Get(ProjectAttributes(projectId), key, cancellationToken);
    }

    public Task<GitLabCustomAttribute> SetForProjectAsync(ProjectId projectId, string key,
        SetCustomAttributeRequest request, CancellationToken cancellationToken = default)
    {
        return Set(ProjectAttributes(projectId), key, request, cancellationToken);
    }

    public Task DeleteForProjectAsync(ProjectId projectId, string key,
        CancellationToken cancellationToken = default)
    {
        return Delete(ProjectAttributes(projectId), key, cancellationToken);
    }

    private static GitLabRouteBuilder UserAttributes(long userId)
    {
        return GitLabRouteBuilder.Create("users")
            .Segment(userId)
            .Literal("custom_attributes");
    }

    private static GitLabRouteBuilder UserAttributes(string userIdOrUsername)
    {
        return GitLabRouteBuilder.Create("users")
            .Escaped(userIdOrUsername)
            .Literal("custom_attributes");
    }

    private static GitLabRouteBuilder GroupAttributes(GroupId groupId)
    {
        return GitLabRouteBuilder.Create("groups")
            .Segment(groupId)
            .Literal("custom_attributes");
    }

    private static GitLabRouteBuilder ProjectAttributes(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("custom_attributes");
    }

    private IAsyncEnumerable<GitLabCustomAttribute> List(GitLabRouteBuilder route,
        CancellationToken cancellationToken)
    {
        return connection.GetPagedAsync(
            route.Build(),
            GitLabJsonContext.Default.GitLabCustomAttributeArray,
            cancellationToken);
    }

    // The key is whatever an administrator typed - it legally contains "/", spaces and "%" - so it is
    // Escaped, not Literal. An unescaped "team/owner" would silently address a route that does not exist
    // and come back as a 404 that reads like a missing attribute.
    private Task<GitLabCustomAttribute> Get(GitLabRouteBuilder route, string key,
        CancellationToken cancellationToken)
    {
        return connection.GetAsync(
            route.Escaped(key).Build(),
            GitLabJsonContext.Default.GitLabCustomAttribute,
            cancellationToken);
    }

    private Task<GitLabCustomAttribute> Set(GitLabRouteBuilder route, string key, SetCustomAttributeRequest request,
        CancellationToken cancellationToken)
    {
        return connection.PutAsync(
            route.Escaped(key).Build(),
            request,
            GitLabJsonContext.Default.SetCustomAttributeRequest,
            GitLabJsonContext.Default.GitLabCustomAttribute,
            cancellationToken);
    }

    private Task Delete(GitLabRouteBuilder route, string key, CancellationToken cancellationToken)
    {
        return connection.DeleteAsync(
            route.Escaped(key).Build(),
            cancellationToken);
    }
}