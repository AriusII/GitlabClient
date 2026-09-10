using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class MarkdownClient(IGitLabApiConnection connection) : IMarkdownClient
{
    public Task<GitLabRenderedMarkdown> RenderAsync(RenderMarkdownRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("markdown").Build(),
            request,
            GitLabJsonContext.Default.RenderMarkdownRequest,
            GitLabJsonContext.Default.GitLabRenderedMarkdown,
            cancellationToken);
    }
}