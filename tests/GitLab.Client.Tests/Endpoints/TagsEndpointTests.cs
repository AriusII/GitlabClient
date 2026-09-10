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

public sealed class TagsEndpointTests
{
    [Fact]
    public async Task ListAsync_BuildsTagsRoute_WithQueryOptions_AndDeserializesTags()
    {
        const string Json = """
                            [
                              {
                                "name": "v1.0.0",
                                "message": "Release 1.0.0",
                                "target": "2695effb5807a22ff3d138d593fd856244e155e7",
                                "commit": {
                                  "id": "2695effb5807a22ff3d138d593fd856244e155e7",
                                  "short_id": "2695effb",
                                  "title": "Initial commit",
                                  "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/2695effb5807a22ff3d138d593fd856244e155e7"
                                },
                                "release": {
                                  "tag_name": "v1.0.0",
                                  "description": "Release notes"
                                },
                                "protected": false,
                                "created_at": "2023-10-12T02:16:52.000Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TagsClient repository = new(connection);

        TagListOptions options = new()
        {
            OrderBy = "updated",
            Sort = "desc",
            Search = "^v",
            Page = 2,
            PerPage = 20
        };

        List<GitLabTag> tags = new();
        await foreach (GitLabTag item in repository.ListAsync(1, options, TestContext.Current.CancellationToken))
        {
            tags.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/repository/tags", requestUri);
        Assert.Contains("order_by=updated", requestUri);
        Assert.Contains("sort=desc", requestUri);
        Assert.Contains("search=%5Ev", requestUri);
        Assert.Contains("page=2", requestUri);
        Assert.Contains("per_page=20", requestUri);

        GitLabTag tag = Assert.Single(tags);
        Assert.Equal("v1.0.0", tag.Name);
        Assert.Equal("Release 1.0.0", tag.Message);
        Assert.Equal("2695effb5807a22ff3d138d593fd856244e155e7", tag.Target);
        Assert.False(tag.Protected);
        Assert.NotNull(tag.Commit);
        Assert.Equal("2695effb", tag.Commit!.ShortId);
        Assert.NotNull(tag.Release);
        Assert.Equal("v1.0.0", tag.Release!.TagName);
        Assert.Equal("Release notes", tag.Release.Description);
        Assert.Equal(new DateTimeOffset(2023, 10, 12, 2, 16, 52, TimeSpan.Zero), tag.CreatedAt);
    }

    [Fact]
    public async Task ListAsync_UsesTagNameAsTheKeysetPageToken()
    {
        const string Json = """
                            [
                              {
                                "name": "v2.0.0",
                                "target": "2695effb5807a22ff3d138d593fd856244e155e7",
                                "protected": false
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TagsClient repository = new(connection);

        await foreach (GitLabTag _ in repository.ListAsync(1, new TagListOptions { PageToken = "v1.0.0" },
                           TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/1/repository/tags?page_token=v1.0.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_EscapesSlashInTagName_AndDeserializesTag()
    {
        const string Json = """
                            {
                              "name": "release/1.0",
                              "message": null,
                              "target": "7b5c3cc8be40ee161ae89a06bba6229da1032a0",
                              "commit": {
                                "id": "7b5c3cc8be40ee161ae89a06bba6229da1032a0",
                                "short_id": "7b5c3cc8",
                                "title": "Cut release",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/7b5c3cc8be40ee161ae89a06bba6229da1032a0"
                              },
                              "protected": true
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TagsClient repository = new(connection);

        GitLabTag tag = await repository.GetAsync(1, "release/1.0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/repository/tags/release%2F1.0", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("release/1.0", tag.Name);
        Assert.Null(tag.Message);
        Assert.True(tag.Protected);
        Assert.Equal("7b5c3cc8be40ee161ae89a06bba6229da1032a0", tag.Target);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        const string Json = """
                            {
                              "name": "v1.0.0",
                              "target": "2695effb5807a22ff3d138d593fd856244e155e7",
                              "protected": false
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TagsClient repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", "v1.0.0", TestContext.Current.CancellationToken);

        Assert.Contains("/projects/gitlab-org%2Fgitlab/repository/tags/v1.0.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Tag Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TagsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, "missing-tag", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Tag Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_PostsToTagsRoute_WithSerializedBody_AndDeserializesCreatedTag()
    {
        const string Json = """
                            {
                              "name": "v2.0.0",
                              "message": "Release 2.0.0",
                              "target": "9a3f2b1c4d5e6f7089a1b2c3d4e5f60718293a4b",
                              "commit": {
                                "id": "9a3f2b1c4d5e6f7089a1b2c3d4e5f60718293a4b",
                                "short_id": "9a3f2b1c",
                                "title": "Cut 2.0.0",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/9a3f2b1c4d5e6f7089a1b2c3d4e5f60718293a4b"
                              },
                              "protected": false
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
        TagsClient repository = new(connection);

        CreateTagRequest request = new() { TagName = "v2.0.0", Ref = "main", Message = "Release 2.0.0" };

        GitLabTag tag = await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/tags",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"tag_name\":\"v2.0.0\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"ref\":\"main\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("v2.0.0", tag.Name);
        Assert.Equal("Release 2.0.0", tag.Message);
    }

    [Fact]
    public async Task DeleteAsync_EscapesSlashInTagName_AndSendsDeleteRequest()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TagsClient repository = new(connection);

        await repository.DeleteAsync(1, "release/1.0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/repository/tags/release%2F1.0", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Tag Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TagsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DeleteAsync(1, "missing-tag", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Tag Not Found", exception.Message);
    }

    [Fact]
    public async Task GetSignatureAsync_EscapesSlashInTagName_AndRetainsTheRawSignatureObject()
    {
        const string Json = """
                            {
                              "signature_type": "PGP",
                              "signature": {
                                "verified": true,
                                "signer": {
                                  "email": "gitlab@example.org"
                                }
                              }
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TagsClient repository = new(connection);

        GitLabTagSignature signature =
            await repository.GetSignatureAsync(1, "release/1.0", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/repository/tags/release%2F1.0/signature",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("PGP", signature.SignatureType);
        Assert.True(signature.Signature.HasValue);
        Assert.True(signature.Signature.Value.GetProperty("verified").GetBoolean());
        Assert.Equal("gitlab@example.org", signature.Signature.Value.GetProperty("signer").GetProperty("email")
            .GetString());
    }

    [Fact]
    public async Task GetSignatureAsync_OnUnsignedTag_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Signature Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TagsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetSignatureAsync(1, "v1.0.0", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}