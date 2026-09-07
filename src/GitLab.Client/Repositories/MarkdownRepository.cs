using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class MarkdownRepository(IGitLabApiConnection connection) : IMarkdownRepository
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