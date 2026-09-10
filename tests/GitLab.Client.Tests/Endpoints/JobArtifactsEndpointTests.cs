using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class JobArtifactsEndpointTests
{
    private static readonly byte[] ArchiveBytes = [0x50, 0x4B, 0x03, 0x04, 0x0A, 0x00];

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task DownloadAsync_BuildsTheArtifactsRoute_StreamsTheBody_AndParsesTheFileName()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(ArchiveBytes) };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment") { FileName = "\"artifacts.zip\"" };

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        using GitLabFileResponse artifacts =
            await repository.DownloadAsync(1, 8, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/jobs/8/artifacts",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/zip", artifacts.ContentType);
        Assert.Equal("artifacts.zip", artifacts.FileName);

        using MemoryStream copy = new();
        await artifacts.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);

        Assert.Equal(ArchiveBytes, copy.ToArray());
    }

    [Fact]
    public async Task DownloadAsync_ProjectsTheFileTypeEnumOntoItsWireValue()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(ArchiveBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        JobArtifactDownloadOptions options = new()
        {
            FileType = GitLabJobArtifactFileType.DependencyScanning, JobToken = "abc123"
        };

        using GitLabFileResponse artifacts =
            await repository.DownloadAsync(1, 8, options, TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;

        // The query value is the [JsonStringEnumMemberName], not the C# member name, so it provably cannot
        // drift from what the serializer would write.
        Assert.Contains("file_type=dependency_scanning", requestUri, StringComparison.Ordinal);
        Assert.Contains("job_token=abc123", requestUri, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, artifacts.StatusCode);
    }

    [Fact]
    public async Task DownloadFileAsync_PercentEncodesTheSlashBearingArtifactPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html/>", Encoding.UTF8, "text/html")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadFileAsync("gitlab-org/gitlab", 8,
            "coverage/report/index.html", cancellationToken: TestContext.Current.CancellationToken);

        // An artifact path contains slashes by definition, so it must stay one route parameter - and so must
        // the namespaced project path in front of it.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/jobs/8/artifacts/"
            + "coverage%2Freport%2Findex.html",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("text/html", file.ContentType);
    }

    [Fact]
    public async Task DownloadFileAsync_AppendsTheJobTokenQueryWhenGiven()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(ArchiveBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadFileAsync(1, 8, "build/app.tar.gz",
            "abc123", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/jobs/8/artifacts/build%2Fapp.tar.gz?job_token=abc123",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
    }

    [Fact]
    public async Task DownloadForRefAsync_PercentEncodesASlashBearingRefName_AndSendsTheRequiredJobQuery()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(ArchiveBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        JobArtifactRefDownloadOptions options = new() { SearchRecentSuccessfulPipelines = true };

        using GitLabFileResponse artifacts = await repository.DownloadForRefAsync(1, "release/1.0", "test",
            options, TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;

        // "release/1.0" unescaped would address /jobs/artifacts/release/1.0/download, a path that does not
        // exist - a 404 that reads like a missing artifact rather than a client bug.
        Assert.StartsWith("https://gitlab.example/api/v4/projects/1/jobs/artifacts/release%2F1.0/download?",
            requestUri, StringComparison.Ordinal);
        Assert.Contains("job=test", requestUri, StringComparison.Ordinal);
        Assert.Contains("search_recent_successful_pipelines=true", requestUri, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, artifacts.StatusCode);
    }

    [Fact]
    public async Task DownloadFileForRefAsync_PercentEncodesBothTheRefNameAndTheArtifactPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("coverage", Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadFileForRefAsync(1, "release/1.0",
            "coverage/index.html", "test", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/jobs/artifacts/release%2F1.0/raw/"
            + "coverage%2Findex.html?job=test",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("text/plain", file.ContentType);
    }

    [Fact]
    public async Task ListAsync_BuildsTheTreeRoute_AppliesQueryOptions_AndDeserializesEntries()
    {
        const string Json = """
                            [
                              {
                                "name": "coverage",
                                "path": "coverage",
                                "type": "directory",
                                "size": 0,
                                "mode": "40755"
                              },
                              {
                                "name": "index.html",
                                "path": "coverage/index.html",
                                "type": "file",
                                "size": 10240,
                                "mode": "100644"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        JobArtifactTreeListOptions options = new()
        {
            Path = "coverage", Recursive = true, JobToken = "abc123", PerPage = 100
        };

        List<GitLabJobArtifactEntry> entries = [];
        await foreach (GitLabJobArtifactEntry entry in repository.ListAsync(1, 8, options,
                           TestContext.Current.CancellationToken))
        {
            entries.Add(entry);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/projects/1/jobs/8/artifacts/tree?", requestUri,
            StringComparison.Ordinal);
        Assert.Contains("path=coverage", requestUri, StringComparison.Ordinal);
        Assert.Contains("recursive=true", requestUri, StringComparison.Ordinal);
        Assert.Contains("job_token=abc123", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=100", requestUri, StringComparison.Ordinal);

        Assert.Equal(2, entries.Count);
        Assert.Equal(GitLabJobArtifactEntryType.Directory, entries[0].Type);
        Assert.Equal(0, entries[0].Size);
        Assert.Equal(GitLabJobArtifactEntryType.File, entries[1].Type);
        Assert.Equal("coverage/index.html", entries[1].Path);
        Assert.Equal(10240, entries[1].Size);
        Assert.Equal("100644", entries[1].Mode);
    }

    [Fact]
    public async Task KeepAsync_PostsToTheKeepRoute_WithNoBody_AndReturnsTheJob()
    {
        const string Json = """
                            {
                              "id": 8,
                              "status": "success",
                              "name": "rspec",
                              "artifacts_expire_at": null,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/jobs/8"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        GitLabJob job = await repository.KeepAsync(1, 8, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/jobs/8/artifacts/keep",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(8, job.Id);
        Assert.Null(job.ArtifactsExpireAt);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheJobArtifactsRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        await repository.DeleteAsync(1, 8, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/jobs/8/artifacts",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAllAsync_SendsDeleteToTheProjectArtifactsRoute_AndAcceptsA202()
    {
        // GitLab deletes in the background here, so the success code is 202 rather than 204.
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        await repository.DeleteAllAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/artifacts",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadAsync_OnMissingArtifacts_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Artifacts Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        JobArtifactsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DownloadAsync(1, 8, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Artifacts Not Found", exception.Message);
    }
}