using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class OrganizationsRepository(IGitLabApiConnection connection) : IOrganizationsRepository
{
    public Task<GitLabOrganization> CreateAsync(CreateOrganizationRequest request, GitLabFileUpload? avatar = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Uri route = GitLabRouteBuilder.Create("organizations").Build();

        // GitLab's spec documents this endpoint as multipart/form-data only (it is the one place an
        // avatar can be attached - Organizations has no separate PUT .../avatar route the way groups and
        // projects do), but the avatar itself is optional. When one is supplied it travels through
        // PostFileAsync exactly like every other binary upload in this library; when it is not, there is
        // no file part to send, so the same fields go as a plain JSON body instead.
        if (avatar is null)
        {
            return connection.PostAsync(
                route,
                request,
                GitLabJsonContext.Default.CreateOrganizationRequest,
                GitLabJsonContext.Default.GitLabOrganization,
                cancellationToken);
        }

        Dictionary<string, string> formFields = new(StringComparer.Ordinal)
        {
            ["name"] = request.Name, ["path"] = request.Path
        };

        if (request.Description is not null)
        {
            formFields["description"] = request.Description;
        }

        if (request.Visibility is { } visibility)
        {
            formFields["visibility"] = visibility switch
            {
                GitLabOrganizationVisibility.Private => "private",
                GitLabOrganizationVisibility.Public => "public",
                _ => throw new ArgumentOutOfRangeException(nameof(request), visibility,
                    "Unknown GitLabOrganizationVisibility member.")
            };
        }

        return connection.PostFileAsync(
            route,
            avatar with { FieldName = "avatar" },
            formFields,
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