using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class BadgesRepositoryTests
{
    private const string CoverageBadgeJson = """
                                             {
                                               "id": 3,
                                               "name": "Coverage",
                                               "link_url": "https://gitlab.example/%{project_path}/-/pipelines",
                                               "image_url": "https://gitlab.example/%{project_path}/badges/%{default_branch}/coverage.svg",
                                               "rendered_link_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines",
                                               "rendered_image_url": "https://gitlab.example/gitlab-org/gitlab/badges/main/coverage.svg",
                                               "kind": "project"
                                             }
                                             """;

    [Fact]
    public async Task ListForProjectAsync_BuildsBadgesRoute_WithNameFilter_AndDeserializesBadges()
    {
        const string Json = $"[{CoverageBadgeJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        List<GitLabBadge> badges = new();
        await foreach (GitLabBadge badge in repository.ListForProjectAsync(42, "Coverage",
                           TestContext.Current.CancellationToken))
        {
            badges.Add(badge);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/badges?name=Coverage",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBadge single = Assert.Single(badges);
        Assert.Equal(3, single.Id);
        Assert.Equal("Coverage", single.Name);

        // The unrendered forms keep GitLab's %{...} placeholders verbatim - the reason they are string, not Uri.
        Assert.Equal("https://gitlab.example/%{project_path}/-/pipelines", single.LinkUrl);
        Assert.Equal("https://gitlab.example/%{project_path}/badges/%{default_branch}/coverage.svg",
            single.ImageUrl);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab/-/pipelines"), single.RenderedLinkUrl);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab/badges/main/coverage.svg"),
            single.RenderedImageUrl);
        Assert.Equal("project", single.Kind);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesNamespacedProjectPath_AndOmitsNameWhenNotSupplied()
    {
        const string Json = $"[{CoverageBadgeJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        List<GitLabBadge> badges = new();
        await foreach (GitLabBadge badge in repository.ListForProjectAsync("gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            badges.Add(badge);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/badges",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(badges);
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsNumericBadgeIdRoute_AndDeserializesBadge()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(CoverageBadgeJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        GitLabBadge badge = await repository.GetForProjectAsync(42, 3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/badges/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3, badge.Id);
        Assert.Equal("Coverage", badge.Name);
        Assert.Equal("project", badge.Kind);
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsSerializedBody_WithPlaceholdersIntact()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(CoverageBadgeJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        CreateBadgeRequest request = new()
        {
            LinkUrl = "https://gitlab.example/%{project_path}/-/pipelines",
            ImageUrl = "https://gitlab.example/%{project_path}/badges/%{default_branch}/coverage.svg",
            Name = "Coverage"
        };

        GitLabBadge badge = await repository.CreateForProjectAsync(42, request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/badges",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        // A placeholder must reach GitLab unescaped in the JSON body; only query values get escaped.
        Assert.Contains("\"link_url\":\"https://gitlab.example/%{project_path}/-/pipelines\"", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"name\":\"Coverage\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(3, badge.Id);
    }

    [Fact]
    public async Task UpdateForProjectAsync_PutsOnlySuppliedMembers_ToNumericBadgeIdRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(CoverageBadgeJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        GitLabBadge badge = await repository.UpdateForProjectAsync(42, 3,
            new UpdateBadgeRequest { Name = "Coverage" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/badges/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"Coverage"}""", sentBody);
        Assert.Equal("Coverage", badge.Name);
    }

    [Fact]
    public async Task DeleteForProjectAsync_SendsDeleteToNumericBadgeIdRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        await repository.DeleteForProjectAsync("gitlab-org/gitlab", 3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/badges/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task PreviewForProjectAsync_BuildsRenderRoute_AndEscapesPlaceholderQueryValues()
    {
        const string Json = """
                            {
                              "name": "Coverage",
                              "link_url": "https://gitlab.example/%{project_path}/-/pipelines",
                              "image_url": "https://gitlab.example/%{project_path}/coverage.svg",
                              "rendered_link_url": "https://gitlab.example/gitlab-org/gitlab/-/pipelines",
                              "rendered_image_url": "https://gitlab.example/gitlab-org/gitlab/coverage.svg"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        GitLabBadgePreview preview = await repository.PreviewForProjectAsync(42,
            "https://gitlab.example/%{project_path}/-/pipelines",
            "https://gitlab.example/%{project_path}/coverage.svg",
            TestContext.Current.CancellationToken);

        // "render" is a literal segment sitting where a numeric badge_id would otherwise go, and the
        // placeholder-bearing query values are percent-escaped ('%{' becomes %25%7B).
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/badges/render"
            + "?link_url=https%3A%2F%2Fgitlab.example%2F%25%7Bproject_path%7D%2F-%2Fpipelines"
            + "&image_url=https%3A%2F%2Fgitlab.example%2F%25%7Bproject_path%7D%2Fcoverage.svg",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("Coverage", preview.Name);
        Assert.Equal("https://gitlab.example/%{project_path}/-/pipelines", preview.LinkUrl);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab/-/pipelines"), preview.RenderedLinkUrl);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab/coverage.svg"), preview.RenderedImageUrl);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupBadgesRoute_AndEncodesNamespacedGroupPath()
    {
        const string Json = """
                            [
                              {
                                "id": 8,
                                "name": "Docs",
                                "link_url": "https://docs.example",
                                "image_url": "https://docs.example/badge.svg",
                                "kind": "group"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        List<GitLabBadge> badges = new();
        await foreach (GitLabBadge badge in repository.ListForGroupAsync("gitlab-org/subgroup", "Docs",
                           TestContext.Current.CancellationToken))
        {
            badges.Add(badge);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/badges?name=Docs",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBadge single = Assert.Single(badges);
        Assert.Equal(8, single.Id);
        Assert.Equal("group", single.Kind);
        Assert.Null(single.RenderedLinkUrl);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsGroupBadgeRoute_AndDeserializesBadge()
    {
        const string Json = """
                            {
                              "id": 8,
                              "name": "Docs",
                              "link_url": "https://docs.example",
                              "image_url": "https://docs.example/badge.svg",
                              "kind": "group"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        GitLabBadge badge = await repository.GetForGroupAsync(9, 8, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/badges/8",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Docs", badge.Name);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsSerializedBodyToGroupBadgesRoute()
    {
        const string Json = """
                            {
                              "id": 8,
                              "name": "Docs",
                              "link_url": "https://docs.example",
                              "image_url": "https://docs.example/badge.svg",
                              "kind": "group"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        GitLabBadge badge = await repository.CreateForGroupAsync(9,
            new CreateBadgeRequest { LinkUrl = "https://docs.example", ImageUrl = "https://docs.example/badge.svg" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/badges", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"link_url":"https://docs.example","image_url":"https://docs.example/badge.svg"}""",
            sentBody);
        Assert.Equal(8, badge.Id);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsSerializedBodyToGroupBadgeRoute()
    {
        const string Json = """
                            {
                              "id": 8,
                              "name": "Handbook",
                              "link_url": "https://handbook.example",
                              "image_url": "https://docs.example/badge.svg",
                              "kind": "group"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        GitLabBadge badge = await repository.UpdateForGroupAsync(9, 8,
            new UpdateBadgeRequest { Name = "Handbook", LinkUrl = "https://handbook.example" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/badges/8",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"link_url":"https://handbook.example","name":"Handbook"}""", sentBody);
        Assert.Equal("Handbook", badge.Name);
    }

    [Fact]
    public async Task DeleteForGroupAsync_SendsDeleteToGroupBadgeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        await repository.DeleteForGroupAsync(9, 8, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/badges/8",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task PreviewForGroupAsync_BuildsGroupRenderRoute()
    {
        const string Json = """
                            {
                              "link_url": "https://docs.example",
                              "image_url": "https://docs.example/badge.svg",
                              "rendered_link_url": "https://docs.example",
                              "rendered_image_url": "https://docs.example/badge.svg"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        GitLabBadgePreview preview = await repository.PreviewForGroupAsync("gitlab-org/subgroup",
            "https://docs.example", "https://docs.example/badge.svg", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/badges/render"
            + "?link_url=https%3A%2F%2Fdocs.example"
            + "&image_url=https%3A%2F%2Fdocs.example%2Fbadge.svg",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Null(preview.Name);
        Assert.Equal(new Uri("https://docs.example"), preview.RenderedLinkUrl);
    }

    [Fact]
    public async Task GetForProjectAsync_OnMissingBadge_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Badge Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForProjectAsync(42, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Badge Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateForProjectAsync_OnValidationError_ThrowsGitLabValidationExceptionWithFieldErrors()
    {
        const string Json = """{ "message": { "link_url": ["is blocked: Only allowed schemes are http, https"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        BadgesRepository repository = new(connection);

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateForProjectAsync(42,
                new CreateBadgeRequest { LinkUrl = "ftp://example", ImageUrl = "ftp://example/b.svg" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("is blocked: Only allowed schemes are http, https",
            Assert.Single(exception.Errors["link_url"]));
    }
}