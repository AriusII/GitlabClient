using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ApplicationsClient(IGitLabApiConnection connection) : IApplicationsClient
{
    public IAsyncEnumerable<GitLabApplication> ListAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("applications").Build(),
            GitLabJsonContext.Default.GitLabApplicationArray,
            cancellationToken);
    }

    public Task<GitLabApplicationWithSecret> CreateAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        // The response body carries the client secret exactly once; never log this request or its response.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("applications").Build(),
            request,
            GitLabJsonContext.Default.CreateApplicationRequest,
            GitLabJsonContext.Default.GitLabApplicationWithSecret,
            cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("applications").Segment(id).Build(),
            cancellationToken);
    }

    public Task<GitLabApplicationWithSecret> RenewSecretAsync(long id, CancellationToken cancellationToken = default)
    {
        // Empty-bodied POST that returns the application with a freshly issued secret.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("applications").Segment(id).Literal("renew-secret").Build(),
            GitLabJsonContext.Default.GitLabApplicationWithSecret,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabApplication> ListForCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("user").Literal("applications").Build(),
            GitLabJsonContext.Default.GitLabApplicationArray,
            cancellationToken);
    }

    public Task<GitLabApplicationWithSecret> CreateForCurrentUserAsync(CreateApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        // The response body carries the client secret exactly once; never log this request or its response.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("user").Literal("applications").Build(),
            request,
            GitLabJsonContext.Default.CreateApplicationRequest,
            GitLabJsonContext.Default.GitLabApplicationWithSecret,
            cancellationToken);
    }

    public Task<GitLabApplication> GetForCurrentUserAsync(long id, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("user").Literal("applications").Segment(id).Build(),
            GitLabJsonContext.Default.GitLabApplication,
            cancellationToken);
    }

    public Task<GitLabApplication> UpdateForCurrentUserAsync(long id, UpdateApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("user").Literal("applications").Segment(id).Build(),
            request,
            GitLabJsonContext.Default.UpdateApplicationRequest,
            GitLabJsonContext.Default.GitLabApplication,
            cancellationToken);
    }

    public Task DeleteForCurrentUserAsync(long id, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("user").Literal("applications").Segment(id).Build(),
            cancellationToken);
    }

    public Task<JsonElement> GetWorkspacesHttpServerConfigAsync(CancellationToken cancellationToken = default)
    {
        // Same undocumented-schema shape as IWorkspacesClient's two agentw endpoints: the spec
        // declares no response schema, so this answers with a raw JsonElement rather than an invented DTO.
        return connection.GetAsync(
            GitLabRouteBuilder.Create("internal").Literal("agents").Literal("agentw").Literal("server_config")
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }
}