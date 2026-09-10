using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class CurrentUserClient(IGitLabApiConnection connection) : ICurrentUserClient
{
    /// <summary>
    ///     The multipart field name <c>PUT /user/avatar</c> expects. Every other GitLab upload endpoint
    ///     calls its part <c>file</c>, which is why <see cref="GitLabFileUpload.FieldName" /> defaults to
    ///     that - sending it here is answered with a validation error, so the caller's value is overridden
    ///     rather than trusted.
    /// </summary>
    private const string AvatarFieldName = "avatar";

    public IAsyncEnumerable<GitLabUserActivity> ListActivitiesAsync(UserActivityListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("user").Literal("activities").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabUserActivityArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabEmail> ListEmailsAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("user").Literal("emails").Build(),
            GitLabJsonContext.Default.GitLabEmailArray,
            cancellationToken);
    }

    public Task<GitLabEmail> GetEmailAsync(long emailId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("user").Literal("emails").Segment(emailId).Build(),
            GitLabJsonContext.Default.GitLabEmail,
            cancellationToken);
    }

    public Task<GitLabEmail> AddEmailAsync(AddCurrentUserEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("user").Literal("emails").Build(),
            request,
            GitLabJsonContext.Default.AddCurrentUserEmailRequest,
            GitLabJsonContext.Default.GitLabEmail,
            cancellationToken);
    }

    public Task DeleteEmailAsync(long emailId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("user").Literal("emails").Segment(emailId).Build(),
            cancellationToken);
    }

    public Task<GitLabUserPreferences> GetPreferencesAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("user").Literal("preferences").Build(),
            GitLabJsonContext.Default.GitLabUserPreferences,
            cancellationToken);
    }

    public Task<GitLabUserPreferences> UpdatePreferencesAsync(UpdateUserPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("user").Literal("preferences").Build(),
            request,
            GitLabJsonContext.Default.UpdateUserPreferencesRequest,
            GitLabJsonContext.Default.GitLabUserPreferences,
            cancellationToken);
    }

    public Task<GitLabUserStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("user").Literal("status").Build(),
            GitLabJsonContext.Default.GitLabUserStatus,
            cancellationToken);
    }

    public Task<GitLabUserStatus> SetStatusAsync(SetUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("user").Literal("status").Build(),
            request,
            GitLabJsonContext.Default.SetUserStatusRequest,
            GitLabJsonContext.Default.GitLabUserStatus,
            cancellationToken);
    }

    public Task<GitLabUserStatus> UpdateStatusAsync(SetUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        // PATCH, not PUT: GitLab exposes both verbs on this one route with deliberately different
        // semantics - PUT nullifies every field the body omits, PATCH leaves them alone.
        return connection.PatchAsync(
            GitLabRouteBuilder.Create("user").Literal("status").Build(),
            request,
            GitLabJsonContext.Default.SetUserStatusRequest,
            GitLabJsonContext.Default.GitLabUserStatus,
            cancellationToken);
    }

    public Task<GitLabSupportPin> GetSupportPinAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("user").Literal("support_pin").Build(),
            GitLabJsonContext.Default.GitLabSupportPin,
            cancellationToken);
    }

    public Task<GitLabSupportPin> CreateSupportPinAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("user").Literal("support_pin").Build(),
            GitLabJsonContext.Default.GitLabSupportPin,
            cancellationToken);
    }

    public Task<GitLabRunnerRegistration> CreateRunnerAsync(CreateUserRunnerRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("user").Literal("runners").Build(),
            request,
            GitLabJsonContext.Default.CreateUserRunnerRequest,
            GitLabJsonContext.Default.GitLabRunnerRegistration,
            cancellationToken);
    }

    public Task<GitLabAvatar> SetAvatarAsync(GitLabFileUpload avatar,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(avatar);

        return connection.PutFileAsync(
            GitLabRouteBuilder.Create("user").Literal("avatar").Build(),
            avatar with { FieldName = AvatarFieldName },
            null,
            GitLabJsonContext.Default.GitLabAvatar,
            cancellationToken);
    }
}