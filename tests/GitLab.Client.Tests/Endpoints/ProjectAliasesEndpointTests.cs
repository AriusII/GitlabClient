using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ProjectAliasesEndpointTests
{
    private const string AliasJson = """
                                     {
                                       "id": 1,
                                       "project_id": 12,
                                       "name": "gitlab"
                                     }
                                     """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task ListAsync_BuildsTheAliasesRoute_AndDeserializesThePayload()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{AliasJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectAliasesClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabProjectAlias> aliases = [];
        await foreach (GitLabProjectAlias alias in repository.ListAsync(cancellationToken: TestContext.Current
                           .CancellationToken))
        {
            aliases.Add(alias);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/project_aliases", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabProjectAlias only = Assert.Single(aliases);
        Assert.Equal(1, only.Id);
        Assert.Equal(12, only.ProjectId);
        Assert.Equal("gitlab", only.Name);
    }

    [Fact]
    public async Task ListAsync_ProjectsPerPageOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectAliasesClient repository = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabProjectAlias _ in repository.ListAsync(new ProjectAliasListOptions { PerPage = 100 },
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/project_aliases?per_page=100",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     An alias is caller-supplied free text and is routinely a project path, so it must be
    ///     percent-encoded. Unescaped, "gitlab-org/gitlab" would address <c>/project_aliases/gitlab-org/gitlab</c>,
    ///     a route GitLab does not serve.
    /// </summary>
    [Fact]
    public async Task GetAsync_PercentEncodesAnAliasContainingASlash()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AliasJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectAliasesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabProjectAlias alias =
            await repository.GetAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/project_aliases/gitlab-org%2Fgitlab",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("gitlab", alias.Name);
    }

    [Fact]
    public async Task CreateAsync_PostsTheProjectPathUnencodedInTheBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(AliasJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectAliasesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabProjectAlias alias = await repository.CreateAsync(
            new CreateProjectAliasRequest { ProjectId = "gitlab-org/gitlab", Name = "gitlab" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/project_aliases", handler.LastRequest?.RequestUri?.AbsoluteUri);

        // A JSON body value, not a route segment: the path must NOT be percent-encoded here.
        Assert.Equal("""{"project_id":"gitlab-org/gitlab","name":"gitlab"}""", sentBody);
        Assert.Equal(12, alias.ProjectId);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheAliasRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectAliasesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteAsync("gitlab", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/project_aliases/gitlab",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     The whole area is administrator-only on Premium/Ultimate, so 403 is the response a caller is most
    ///     likely to meet. It must surface as the typed exception, not as a bare GitLabApiException.
    /// </summary>
    [Fact]
    public async Task GetAsync_MapsForbiddenToTheTypedException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"message":"403 Forbidden"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectAliasesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.GetAsync("gitlab", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}