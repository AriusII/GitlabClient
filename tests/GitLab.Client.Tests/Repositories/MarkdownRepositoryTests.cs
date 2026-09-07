using System.Net;
using System.Text;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class MarkdownRepositoryTests
{
    private const string RenderedJson = """
                                        {
                                          "html": "<p dir=\"auto\">Hello <a href=\"/gitlab-org/gitlab/-/issues/42\">#42</a></p>"
                                        }
                                        """;

    [Fact]
    public async Task RenderAsync_PostsToTheMarkdownRoute_AndDeserializesTheHtml()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(RenderedJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MarkdownRepository repository = new(connection);

        GitLabRenderedMarkdown rendered = await repository.RenderAsync(
            new RenderMarkdownRequest { Text = "Hello #42" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/markdown", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("<p dir=\"auto\">Hello <a href=\"/gitlab-org/gitlab/-/issues/42\">#42</a></p>", rendered.Html);
    }

    [Fact]
    public async Task RenderAsync_SendsGfmAndProjectContext_UsingGitLabWireNames()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(RenderedJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MarkdownRepository repository = new(connection);

        await repository.RenderAsync(
            new RenderMarkdownRequest { Text = "Hello #42", Gfm = true, Project = "gitlab-org/gitlab" },
            TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        Assert.Contains("\"text\":\"Hello #42\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"gfm\":true", sentBody, StringComparison.Ordinal);

        // The project is a body parameter, not a route segment, so it must reach GitLab with its slash
        // intact - percent-encoding it here would make reference resolution silently fail.
        Assert.Contains("\"project\":\"gitlab-org/gitlab\"", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RenderAsync_OmitsUnsetMembers_RatherThanSendingNulls()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(RenderedJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MarkdownRepository repository = new(connection);

        await repository.RenderAsync(
            new RenderMarkdownRequest { Text = "plain" },
            TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        Assert.DoesNotContain("gfm", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("project", sentBody, StringComparison.Ordinal);
    }
}