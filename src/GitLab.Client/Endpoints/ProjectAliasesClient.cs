using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ProjectAliasesClient(IGitLabApiConnection connection) : IProjectAliasesClient
{
    public IAsyncEnumerable<GitLabProjectAlias> ListAsync(ProjectAliasListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("project_aliases").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabProjectAliasArray,
            cancellationToken);
    }

    // The alias is caller-supplied free text, so Escaped: an alias is allowed to look like a project path
    // and carry a "/", which unescaped would address a route that does not exist.
    public Task<GitLabProjectAlias> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("project_aliases").Escaped(name).Build(),
            GitLabJsonContext.Default.GitLabProjectAlias,
            cancellationToken);
    }

    public Task<GitLabProjectAlias> CreateAsync(CreateProjectAliasRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("project_aliases").Build(),
            request,
            GitLabJsonContext.Default.CreateProjectAliasRequest,
            GitLabJsonContext.Default.GitLabProjectAlias,
            cancellationToken);
    }

    public Task DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("project_aliases").Escaped(name).Build(),
            cancellationToken);
    }
}