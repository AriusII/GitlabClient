using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Organizations resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         GitLab Organizations were introduced in 17.5 and are still experimental as of 19.4 (behind the
///         <c>org_stage_experimental</c> feature flag) - the spec exposes only creation and soft-delete
///         today, so that is all this resource wraps.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IOrganizationsService), typeof(IOrganizationsClient))]
internal interface IOrganizationsRepository
{
    Task<GitLabOrganization> CreateAsync(CreateOrganizationRequest request, GitLabFileUpload? avatar = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(long organizationId, CancellationToken cancellationToken = default);
}