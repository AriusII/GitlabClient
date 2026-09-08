using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ContainerRegistryRepositoryTests
{
    private const string RepositoryJson = """
                                          {
                                            "id": 1,
                                            "name": "",
                                            "path": "group/project",
                                            "project_id": 9,
                                            "location": "registry.example.com:5000/group/project",
                                            "created_at": "2019-01-10T13:38:57.391Z",
                                            "cleanup_policy_started_at": "2020-01-10T15:40:57.391Z",
                                            "tags_count": 3,
                                            "tags": [
                                              {
                                                "name": "0.0.1",
                                                "path": "group/project:0.0.1",
                                                "location": "registry.example.com:5000/group/project:0.0.1"
                                              }
                                            ],
                                            "delete_api_path": "/api/v4/projects/9/registry/repositories/1",
                                            "size": 12345,
                                            "status": null
                                          }
                                          """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task IngestEventsAsync_PostsToTheEventsRoute_WithTheRawPayload()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        using JsonDocument payloadDoc = JsonDocument.Parse("""{ "events": [ { "action": "push" } ] }""");

        await repository.IngestEventsAsync(payloadDoc.RootElement.Clone(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/container_registry_event/events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"events":[{"action":"push"}]}""", sentBody);
    }

    [Fact]
    public async Task IngestEventsAsync_OnUnauthorized_ThrowsGitLabAuthenticationException()
    {
        const string Json = """{ "message": "Invalid Token" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        using JsonDocument payloadDoc = JsonDocument.Parse("""{ "events": [] }""");

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabAuthenticationException>(() =>
            repository.IngestEventsAsync(payloadDoc.RootElement.Clone(), TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupRoute_AndDeserializesEntries()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{RepositoryJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        List<GitLabRegistryRepository> repositories = [];
        await foreach (GitLabRegistryRepository item in
                       repository.ListForGroupAsync(9970, cancellationToken: TestContext.Current.CancellationToken))
        {
            repositories.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/registry/repositories",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabRegistryRepository only = Assert.Single(repositories);
        Assert.Equal(1, only.Id);
        Assert.Equal("group/project", only.Path);
        Assert.Equal(9, only.ProjectId);
        Assert.Equal("registry.example.com:5000/group/project", only.Location);
        Assert.Equal(3, only.TagsCount);
        Assert.Equal("0.0.1", Assert.Single(only.Tags!).Name);
        Assert.Equal(12345, only.Size);
        Assert.Null(only.Status);
    }

    [Fact]
    public async Task ListForGroupAsync_EncodesNamespacedGroupPath_AndAppliesPagingQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        await foreach (GitLabRegistryRepository _ in repository.ListForGroupAsync("parent-group/subgroup",
                           new GroupRegistryRepositoryListOptions { Page = 2, PerPage = 50 },
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/registry/repositories?page=2&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForProjectAsync_BuildsProjectRoute_AndAppliesTagsQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{RepositoryJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        List<GitLabRegistryRepository> repositories = [];
        await foreach (GitLabRegistryRepository item in repository.ListForProjectAsync(9,
                           new RegistryRepositoryListOptions { Tags = true, TagsCount = true },
                           TestContext.Current.CancellationToken))
        {
            repositories.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/9/registry/repositories?tags=true&tags_count=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("group/project", Assert.Single(repositories).Path);
    }

    [Fact]
    public async Task DeleteRepositoryAsync_EncodesNamespacedProjectPath_AndSendsDelete()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        await repository.DeleteRepositoryAsync("gitlab-org/gitlab", 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/registry/repositories/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteTagsAsync_AppliesEveryRegexAndRetentionFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        DeleteRegistryRepositoryTagsOptions options = new()
        {
            NameRegexDelete = ".*", NameRegexKeep = "release-.*", KeepN = 10, OlderThan = "1month"
        };

        await repository.DeleteTagsAsync(9, 1, options, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/9/registry/repositories/1/tags"
            + "?name_regex_delete=.%2A&name_regex_keep=release-.%2A&keep_n=10&older_than=1month",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     <see cref="DeleteRegistryRepositoryTagsOptions.NameRegex" /> is the route's older alias for
    ///     <see cref="DeleteRegistryRepositoryTagsOptions.NameRegexDelete" />, kept because the spec still
    ///     declares both - this exercises it on its own so a wire-name regression on the alias isn't hidden
    ///     behind the other test's exclusive use of the newer name.
    /// </summary>
    [Fact]
    public async Task DeleteTagsAsync_AppliesTheOlderNameRegexAlias()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        DeleteRegistryRepositoryTagsOptions options = new() { NameRegex = ".*" };

        await repository.DeleteTagsAsync(9, 1, options, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/9/registry/repositories/1/tags?name_regex=.%2A",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListTagsAsync_BuildsTagsRoute_AndDeserializesEntries()
    {
        const string TagsJson = """
                                [
                                  {
                                    "name": "0.0.1",
                                    "path": "group/project:0.0.1",
                                    "location": "registry.example.com:5000/group/project:0.0.1"
                                  },
                                  {
                                    "name": "latest",
                                    "path": "group/project:latest",
                                    "location": "registry.example.com:5000/group/project:latest"
                                  }
                                ]
                                """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TagsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        List<GitLabRegistryRepositoryTag> tags = [];
        await foreach (GitLabRegistryRepositoryTag tag in
                       repository.ListTagsAsync(9, 1, cancellationToken: TestContext.Current.CancellationToken))
        {
            tags.Add(tag);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/9/registry/repositories/1/tags",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, tags.Count);
        Assert.Equal("latest", tags[1].Name);
        Assert.Equal("group/project:latest", tags[1].Path);
    }

    [Fact]
    public async Task ListTagsAsync_AppliesThePagingQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        await foreach (GitLabRegistryRepositoryTag _ in repository.ListTagsAsync(9, 1,
                           new RegistryRepositoryTagListOptions { Page = 2, PerPage = 50 },
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/9/registry/repositories/1/tags?page=2&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteTagAsync_EscapesTheTagName_AndSendsDelete()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        await repository.DeleteTagAsync(9, 1, "release/1.0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/9/registry/repositories/1/tags/release%2F1.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetTagAsync_ReturnsTagDetails()
    {
        const string TagDetailsJson = """
                                      {
                                        "name": "0.0.1",
                                        "path": "group/project:0.0.1",
                                        "location": "registry.example.com:5000/group/project:0.0.1",
                                        "revision": "e15afa4858b1d3e878e1d7c9dd1e04c1f8b1a5f9c1e9d1a5",
                                        "short_revision": "e15afa4858",
                                        "digest": "sha256:4b6d9c3a...",
                                        "created_at": "2019-08-08T14:03:39.000Z",
                                        "total_size": 1234567
                                      }
                                      """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TagDetailsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        GitLabRegistryRepositoryTagDetails tag =
            await repository.GetTagAsync(9, 1, "0.0.1", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/9/registry/repositories/1/tags/0.0.1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("sha256:4b6d9c3a...", tag.Digest);
        Assert.Equal("e15afa4858", tag.ShortRevision);
        Assert.Equal(1234567, tag.TotalSize);
    }

    [Fact]
    public async Task GetTagAsync_OnMissingTag_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Tag Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetTagAsync(9, 1, "missing", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetAsync_BuildsTopLevelRepositoryRoute_WithInclusionFlags()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RepositoryJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        ContainerRegistryRepository repository = new(connection);

        GitLabRegistryRepository result = await repository.GetAsync(1,
            new RegistryRepositoryGetOptions { Tags = true, TagsCount = true, Size = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/registry/repositories/1?tags=true&tags_count=true&size=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, result.Id);
        Assert.Equal(9, result.ProjectId);
    }
}