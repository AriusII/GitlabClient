using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class WorkspacesClient(IGitLabApiConnection connection) : IWorkspacesClient
{
    public Task<JsonElement> GetAgentInfoAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("internal").Literal("agents").Literal("agentw").Literal("agent_info").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> AuthorizeUserAccessAsync(string workspaceHost, long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("internal").Literal("agents").Literal("agentw")
                .Literal("authorize_user_access")
                .Query("workspace_host", workspaceHost)
                .Query("user_id", userId)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }
}