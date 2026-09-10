using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class GroupImportEndpointTests
{
    private const string RelationStatusJson = """
                                              {
                                                "relation": "issues",
                                                "status": "started",
                                                "error": null,
                                                "updated_at": "2012-05-28T04:42:42-07:00",
                                                "batched": true,
                                                "batches_count": 2,
                                                "total_objects_count": 100,
                                                "batches": [
                                                  {
                                                    "status": "finished",
                                                    "batch_number": 1,
                                                    "objects_count": 60,
                                                    "error": null,
                                                    "updated_at": "2012-05-28T04:42:42-07:00"
                                                  },
                                                  {
                                                    "status": "failed",
                                                    "batch_number": 2,
                                                    "objects_count": 40,
                                                    "error": "Error message",
                                                    "updated_at": "2012-05-28T04:43:42-07:00"
                                                  }
                                                ]
                                              }
                                              """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly byte[] ArchiveBytes = [0x1F, 0x8B, 0x08, 0x00, 0x42, 0x13];

    [Fact]
    public async Task ImportAsync_PostsMultipartFormData_WithTheGroupFieldsAlongsideTheArchive()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        using MemoryStream content = new(ArchiveBytes);
        GitLabFileUpload file = new()
        {
            Content = content, FileName = "group-export.tar.gz", ContentType = "application/gzip"
        };

        await repository.ImportAsync(file, "imported-group", "Imported Group", 42, 7,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/import",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("group-export.tar.gz", sentBody, StringComparison.Ordinal);
        Assert.Contains("application/gzip", sentBody, StringComparison.Ordinal);
        Assert.Contains("imported-group", sentBody, StringComparison.Ordinal);
        Assert.Contains("Imported Group", sentBody, StringComparison.Ordinal);
        Assert.Contains("parent_id", sentBody, StringComparison.Ordinal);
        Assert.Contains("organization_id", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task ImportAsync_OmitsTheOptionalFormFields_WhenTheyAreNotSupplied()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        using MemoryStream content = new(ArchiveBytes);
        GitLabFileUpload file = new() { Content = content, FileName = "group-export.tar.gz" };

        await repository.ImportAsync(file, "imported-group", "Imported Group",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        Assert.DoesNotContain("parent_id", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("organization_id", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AuthorizeImportAsync_PostsToTheWorkhorseAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        await repository.AuthorizeImportAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/import/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateExportAsync_EncodesANamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        await repository.CreateExportAsync("parent-group/subgroup", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/export",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadExportAsync_StreamsTheArchive_AndSurfacesTheContentDispositionFileName()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(ArchiveBytes) };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/gzip");
            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment") { FileName = "\"2024-01-01_group_export.tar.gz\"" };

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        using GitLabFileResponse archive =
            await repository.DownloadExportAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/export/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("application/gzip", archive.ContentType);
        Assert.Equal("2024-01-01_group_export.tar.gz", archive.FileName);

        using MemoryStream copy = new();
        await archive.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(ArchiveBytes, copy.ToArray());
    }

    [Fact]
    public async Task ScheduleRelationsExportAsync_PostsTheBatchedFlag()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        await repository.ScheduleRelationsExportAsync("gitlab-org/charts",
            new ScheduleGroupRelationsExportRequest { Batched = true }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fcharts/export_relations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"batched":true}""", sentBody);
    }

    [Fact]
    public async Task ScheduleRelationsExportAsync_OmitsBatched_WhenItIsNotSet()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        await repository.ScheduleRelationsExportAsync(9970, new ScheduleGroupRelationsExportRequest(),
            TestContext.Current.CancellationToken);

        Assert.Equal("{}", sentBody);
    }

    [Fact]
    public async Task DownloadRelationsExportAsync_BuildsTheBatchQuery()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(ArchiveBytes) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/gzip");
            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        using GitLabFileResponse relation = await repository.DownloadRelationsExportAsync(9970, "issues", true,
            2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/export_relations/download?relation=issues&batched=true&batch_number=2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/gzip", relation.ContentType);
    }

    [Fact]
    public async Task DownloadRelationsExportAsync_SendsOnlyTheRelation_WhenTheExportIsNotBatched()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(ArchiveBytes)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        using GitLabFileResponse relation = await repository.DownloadRelationsExportAsync(
            "parent-group/subgroup", "merge_requests", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/export_relations/download?relation=merge_requests",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(relation.Content);
    }

    [Fact]
    public async Task GetRelationsExportStatusAsync_FiltersByRelation_AndDeserializesTheBatches()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RelationStatusJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        GitLabGroupRelationsExportStatus status =
            await repository.GetRelationsExportStatusAsync(9970, "issues", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/export_relations/status?relation=issues",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("issues", status.Relation);
        Assert.Equal(GitLabGroupRelationExportState.Started, status.Status);
        Assert.Null(status.Error);
        Assert.True(status.Batched);
        Assert.Equal(2, status.BatchesCount);
        Assert.Equal(100, status.TotalObjectsCount);
        Assert.Equal(new DateTimeOffset(2012, 5, 28, 4, 42, 42, TimeSpan.FromHours(-7)), status.UpdatedAt);

        Assert.NotNull(status.Batches);
        Assert.Equal(2, status.Batches!.Count);
        Assert.Equal(GitLabGroupRelationExportBatchState.Finished, status.Batches[0].Status);
        Assert.Equal(1, status.Batches[0].BatchNumber);
        Assert.Equal(60, status.Batches[0].ObjectsCount);
        Assert.Equal(GitLabGroupRelationExportBatchState.Failed, status.Batches[1].Status);
        Assert.Equal("Error message", status.Batches[1].Error);
    }

    [Fact]
    public async Task ListRelationsExportStatusesAsync_SendsNoRelationFilter_AndReadsTheArrayForm()
    {
        string json = $"[{RelationStatusJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        List<GitLabGroupRelationsExportStatus> statuses = [];
        await foreach (GitLabGroupRelationsExportStatus item in
                       repository.ListRelationsExportStatusesAsync("parent-group/subgroup",
                           TestContext.Current.CancellationToken))
        {
            statuses.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/export_relations/status",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabGroupRelationsExportStatus status = Assert.Single(statuses);
        Assert.Equal("issues", status.Relation);
        Assert.Equal(GitLabGroupRelationExportState.Started, status.Status);
    }

    [Fact]
    public async Task DownloadExportAsync_BeforeTheArchiveExists_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DownloadExportAsync(9970, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task CreateExportAsync_WhenRateLimited_ThrowsGitLabRateLimitExceededException()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new((HttpStatusCode)429)
            {
                Content = new StringContent("""{ "message": "429 Too Many Requests" }""", Encoding.UTF8,
                    "application/json")
            };

            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(60));

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        GroupImportClient repository = new(connection);

        GitLabRateLimitExceededException exception =
            await Assert.ThrowsAsync<GitLabRateLimitExceededException>(() =>
                repository.CreateExportAsync(9970, TestContext.Current.CancellationToken));

        Assert.Equal((HttpStatusCode)429, exception.StatusCode);
    }
}