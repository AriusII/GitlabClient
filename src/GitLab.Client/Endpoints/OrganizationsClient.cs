using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class OrganizationsClient(IGitLabApiConnection connection) : IOrganizationsClient
{
    public Task<GitLabOrganization> CreateAsync(CreateOrganizationRequest request, GitLabFileUpload? avatar = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Uri route = GitLabRouteBuilder.Create("organizations").Build();

        // GitLab's spec documents this endpoint as multipart/form-data only (it is the one place an
        // avatar can be attached - Organizations has no separate PUT .../avatar route the way groups and
        // projects do). The typed form extension retains that representation when no file is supplied.
        if (avatar is null)
        {
            return connection.PostMultipartFormAsync(
                route,
                request,
                GitLabJsonContext.Default.CreateOrganizationRequest,
                GitLabJsonContext.Default.GitLabOrganization,
                cancellationToken);
        }

        return connection.PostFileAsync(
            route,
            avatar with { FieldName = "avatar" },
            GitLabFormFields.FromRequest(request, GitLabJsonContext.Default.CreateOrganizationRequest),
            GitLabJsonContext.Default.GitLabOrganization,
            cancellationToken);
    }

    public Task DeleteAsync(long organizationId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("organizations").Segment(organizationId).Build(),
            cancellationToken);
    }
}