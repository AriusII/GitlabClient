using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

/// <summary>
///     Project export/import and project templates, mocked at the <see cref="HttpMessageHandler" /> level.
///     What is verified is the bytes on the wire: route construction and percent-encoding, query strings,
///     the multipart form fields of an archive upload, and deserialization of realistic payloads.
/// </summary>
public sealed class ProjectImportRepositoryTests
{
    private const string ExportStatusJson = """
                                            {
                                              "id": 1,
                                              "description": "Itaque perspiciatis minima",
                                              "name": "Gitlab Test",
                                              "name_with_namespace": "Gitlab Org / Gitlab Test",
                                              "path": "gitlab-test",
                                              "path_with_namespace": "gitlab-org/gitlab-test",
                                              "created_at": "2017-08-29T04:36:44.383Z",
                                              "export_status": "finished",
                                              "_links": {
                                                "api_url": "https://gitlab.example/api/v4/projects/1/export/download",
                                                "web_url": "https://gitlab.example/gitlab-org/gitlab-test/download_export"
                                              }
                                            }
                                            """;

    private const string ImportStatusJson = """
                                            {
                                              "id": 1,
                                              "description": "Itaque perspiciatis minima",
                                              "name": "Gitlab Test",
                                              "name_with_namespace": "Gitlab Org / Gitlab Test",
                                              "path": "gitlab-test",
                                              "path_with_namespace": "gitlab-org/gitlab-test",
                                              "created_at": "2017-08-29T04:36:44.383Z",
                                              "import_status": "started",
                                              "import_type": "gitlab_project",
                                              "correlation_id": "dfcf583058ed4508e4c7c617bd7f0edd",
                                              "failed_relations": [
                                                {
                                                  "id": 42,
                                                  "created_at": "2012-05-28T04:42:42-07:00",
                                                  "exception_class": "StandardError",
                                                  "exception_message": "Error importing issue",
                                                  "source": "ImportRepositoryWorker",
                                                  "relation_name": "issues",
                                                  "line_number": 7
                                                }
                                              ],
                                              "stats": {
                                                "fetched": { "note": 2 },
                                                "imported": { "note": 1 }
                                              }
                                            }
                                            """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetExportStatusAsync_BuildsExportRoute_AndDeserializesTheStatusVocabulary()
    {
        using StubHttpMessageHandler handler = Json(ExportStatusJson);
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabProjectExportStatus status =
            await repository.GetExportStatusAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/export",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(1, status.Id);
        Assert.Equal("gitlab-org/gitlab-test", status.PathWithNamespace);
        Assert.Equal(GitLabProjectExportState.Finished, status.ExportStatus);
        Assert.NotNull(status.Links);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/export/download", status.Links!.ApiUrl?.AbsoluteUri);
    }

    [Fact]
    public async Task GetExportStatusAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = Json(ExportStatusJson);
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.GetExportStatusAsync("gitlab-org/sub group/gitlab-test",
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fsub%20group%2Fgitlab-test/export",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ExportAsync_WithoutARequest_SendsNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.ExportAsync(7, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/export",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task ExportAsync_SerializesTheUploadDestinationAndExcludedRelations()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        ExportProjectRequest request = new()
        {
            Description = "nightly",
            Upload = new GitLabProjectExportUpload
            {
                Url = new Uri("https://example.invalid/bucket/export.tar.gz"),
                HttpMethod = GitLabProjectExportUploadMethod.Post
            },
            ExcludedRelations = ["merge_requests", "issues"]
        };

        await repository.ExportAsync(7, request, TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        Assert.Contains("\"description\":\"nightly\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"http_method\":\"POST\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"excluded_relations\":[\"merge_requests\",\"issues\"]", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DownloadExportAsync_StreamsTheArchive_AndSurfacesTheContentDispositionFileName()
    {
        byte[] archive = [0x1F, 0x8B, 0x08, 0x00, 0x11, 0x22];

        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(archive) };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/gzip");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "\"2024-01-01_gitlab-org_gitlab-test_export.tar.gz\""
            };

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabFileResponse file =
            await repository.DownloadExportAsync(1, TestContext.Current.CancellationToken);

        await using (file.ConfigureAwait(true))
        {
            Assert.Equal("https://gitlab.example/api/v4/projects/1/export/download",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("application/gzip", file.ContentType);
            Assert.Equal("2024-01-01_gitlab-org_gitlab-test_export.tar.gz", file.FileName);

            using MemoryStream buffer = new();
            await file.Content.CopyToAsync(buffer, TestContext.Current.CancellationToken).ConfigureAwait(true);
            Assert.Equal(archive, buffer.ToArray());
        }
    }

    [Fact]
    public async Task DownloadExportAsync_MapsNotFoundToTheTypedException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Not found"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        await Assert.ThrowsAsync<GitLabNotFoundException>(async () => await repository
                .DownloadExportAsync(1, TestContext.Current.CancellationToken)
                .ConfigureAwait(true))
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task DownloadRelationsExportAsync_BuildsTheBatchQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([0x00])
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabFileResponse file = await repository.DownloadRelationsExportAsync(
            "gitlab-org/gitlab-test", "merge_requests", true, 3, TestContext.Current.CancellationToken);

        await using (file.ConfigureAwait(true))
        {
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab-test/export_relations/download"
                + "?relation=merge_requests&batched=true&batch_number=3",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task ListRelationExportStatusesAsync_DeserializesBatchesAndTheStatusEnum()
    {
        const string json = """
                            [
                              {
                                "relation": "issues",
                                "status": "finished",
                                "error": null,
                                "updated_at": "2021-05-04T11:25:20.423Z",
                                "batched": true,
                                "batches_count": 2,
                                "total_objects_count": 100,
                                "batches": [
                                  {
                                    "status": "finished",
                                    "batch_number": 1,
                                    "objects_count": 50,
                                    "error": null,
                                    "updated_at": "2021-05-04T11:25:20.423Z"
                                  },
                                  {
                                    "status": "failed",
                                    "batch_number": 2,
                                    "objects_count": 50,
                                    "error": "Error message",
                                    "updated_at": "2021-05-04T11:25:21.423Z"
                                  }
                                ]
                              },
                              {
                                "relation": "milestones",
                                "status": "pending",
                                "batched": false
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = Json(json);
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabProjectRelationExportStatus> statuses = [];
        await foreach (GitLabProjectRelationExportStatus status in
                       repository.ListRelationExportStatusesAsync(1, TestContext.Current.CancellationToken))
        {
            statuses.Add(status);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/1/export_relations/status",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(2, statuses.Count);
        Assert.Equal(GitLabProjectRelationExportState.Finished, statuses[0].Status);
        Assert.Equal(2, statuses[0].BatchesCount);
        Assert.NotNull(statuses[0].Batches);
        Assert.Equal(2, statuses[0].Batches!.Count);
        Assert.Equal(GitLabProjectRelationExportBatchState.Failed, statuses[0].Batches![1].Status);
        Assert.Equal("Error message", statuses[0].Batches![1].Error);
        Assert.Equal(GitLabProjectRelationExportState.Pending, statuses[1].Status);
        Assert.Null(statuses[1].Batches);
    }

    [Fact]
    public async Task GetRelationExportStatusAsync_FiltersByRelation()
    {
        const string json = """{"relation":"issues","status":"started","batched":false}""";

        using StubHttpMessageHandler handler = Json(json);
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabProjectRelationExportStatus status =
            await repository.GetRelationExportStatusAsync(1, "issues", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/export_relations/status?relation=issues",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabProjectRelationExportState.Started, status.Status);
    }

    [Fact]
    public async Task ExportRelationsAsync_SendsTheBatchedFlag()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.ExportRelationsAsync(1, new ExportProjectRelationsRequest { Batched = true },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/export_relations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"batched":true}""", sentBody);
    }

    [Fact]
    public async Task GetImportStatusAsync_DeserializesFailedRelationsAndTheUntypedStats()
    {
        using StubHttpMessageHandler handler = Json(ImportStatusJson);
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabProjectImportStatus status =
            await repository.GetImportStatusAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/import",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The spec types import_status as a bare string, so the library keeps it a string: a value GitLab
        // adds later must not turn a healthy response into a JsonException.
        Assert.Equal("started", status.ImportStatus);
        Assert.Equal("gitlab_project", status.ImportType);

        GitLabProjectImportFailedRelation failure = Assert.Single(status.FailedRelations!);
        Assert.Equal(42, failure.Id);
        Assert.Equal("StandardError", failure.ExceptionClass);
        Assert.Equal("issues", failure.RelationName);
        Assert.Equal(7, failure.LineNumber);

        Assert.NotNull(status.Stats);
        Assert.Equal(2, status.Stats!.Value.GetProperty("fetched").GetProperty("note").GetInt32());
    }

    [Fact]
    public async Task GetRelationImportStatusAsync_BuildsTheRelationImportsRoute()
    {
        using StubHttpMessageHandler handler = Json(ImportStatusJson);
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.GetRelationImportStatusAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/relation-imports",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ImportFromGitAsync_SendsGitLabsOwnWireNamesForTheCredentials()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ImportStatusJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        ImportProjectFromGitRequest request = new()
        {
            ImportUrl = new Uri("https://example.invalid/mirror.git"),
            ImportUsername = "svc",
            ImportPassword = "s3cret"
        };

        GitLabProjectImportStatus status =
            await repository.ImportFromGitAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/import/git",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("\"import_url_user\":\"svc\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"import_url_password\":\"s3cret\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(1, status.Id);
    }

    [Fact]
    public async Task ImportArchiveAsync_PostsAMultipartBodyWithBracketedOverrideParams()
    {
        using CapturingHttpMessageHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(ImportStatusJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream archive = new("ARCHIVE-BYTES"u8.ToArray());

        GitLabFileUpload upload = new()
        {
            Content = archive, FileName = "export.tar.gz", ContentType = "application/gzip"
        };

        ImportProjectArchiveRequest request = new()
        {
            Path = "restored",
            Name = "Restored",
            NamespacePath = "gitlab-org/sub",
            Overwrite = true,
            OverrideParams = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["description"] = "restored from backup"
            }
        };

        GitLabProjectImportStatus status =
            await repository.ImportArchiveAsync(upload, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/import", handler.RequestUri?.AbsoluteUri);
        Assert.StartsWith("multipart/form-data", handler.RequestContentType ?? string.Empty,
            StringComparison.Ordinal);

        string body = handler.RequestBody.Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=path", body, StringComparison.Ordinal);
        Assert.Contains("restored", body, StringComparison.Ordinal);
        Assert.Contains("name=namespace_path", body, StringComparison.Ordinal);
        Assert.Contains("name=overwrite", body, StringComparison.Ordinal);
        Assert.Contains("name=override_params[description]", body, StringComparison.Ordinal);
        Assert.Contains("restored from backup", body, StringComparison.Ordinal);
        Assert.Contains("name=file", body, StringComparison.Ordinal);
        Assert.Contains("filename=export.tar.gz", body, StringComparison.Ordinal);
        Assert.Contains("ARCHIVE-BYTES", body, StringComparison.Ordinal);

        // The upload stream is borrowed, never owned by the transport.
        Assert.True(archive.CanRead);
        Assert.Equal(1, status.Id);
    }

    [Fact]
    public async Task ImportRelationAsync_PostsPathAndRelationAlongsideTheArchive()
    {
        const string trackerJson = """
                                   {
                                     "id": 9,
                                     "project_path": "gitlab-org/gitlab-test",
                                     "relation": "issues",
                                     "status": "created",
                                     "created_at": "2022-01-31T15:10:45.080Z",
                                     "updated_at": "2022-01-31T15:10:45.080Z"
                                   }
                                   """;

        using CapturingHttpMessageHandler handler = new(() => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(trackerJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream archive = new("ARCHIVE"u8.ToArray());

        GitLabProjectRelationImport tracker = await repository.ImportRelationAsync(
            new GitLabFileUpload { Content = archive, FileName = "export.tar.gz" },
            new ImportProjectRelationRequest { Path = "gitlab-org/gitlab-test", Relation = "issues" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/import-relation", handler.RequestUri?.AbsoluteUri);

        string body = handler.RequestBody.Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=relation", body, StringComparison.Ordinal);
        Assert.Contains("gitlab-org/gitlab-test", body, StringComparison.Ordinal);

        Assert.Equal(9, tracker.Id);
        Assert.Equal("created", tracker.Status);
    }

    [Fact]
    public async Task AuthorizeUploadEndpoints_BuildTheirWorkhorseRoutes()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.AuthorizeArchiveUploadAsync(TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/projects/import/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        await repository.AuthorizeRelationUploadAsync(TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/projects/import-relation/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ImportFromRemoteArchiveAsync_PostsTheRemoteImportRoute()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ImportStatusJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        using JsonDocument overrides = JsonDocument.Parse("""{"description":"from remote"}""");

        await repository.ImportFromRemoteArchiveAsync(
            new ImportProjectFromRemoteArchiveRequest
            {
                Url = new Uri("https://example.invalid/export.tar.gz"),
                Path = "restored",
                NamespaceId = 12,
                Overwrite = false,
                OverrideParams = overrides.RootElement
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/remote-import",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("\"url\":\"https://example.invalid/export.tar.gz\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"namespace_id\":12", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"override_params\":{\"description\":\"from remote\"}", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ImportFromS3Async_PostsTheS3RouteWithItsCredentials()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ImportStatusJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.ImportFromS3Async(
            new ImportProjectFromS3Request
            {
                Region = "eu-west-1",
                BucketName = "backups",
                FileKey = "exports/gitlab-test.tar.gz",
                AccessKeyId = "AKIAEXAMPLE",
                SecretAccessKey = "s3cret",
                Path = "restored"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/remote-import-s3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("\"bucket_name\":\"backups\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"access_key_id\":\"AKIAEXAMPLE\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"secret_access_key\":\"s3cret\"", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ImportFromGitHubAsync_PostsTheImportRoute_AndDeserializesTheThinProjectShape()
    {
        const string json = """
                            {
                              "id": 27,
                              "name": "my-repo",
                              "full_path": "/root/my-repo",
                              "full_name": "Administrator / my-repo",
                              "refs_url": "/root/my-repo/refs",
                              "forked": false
                            }
                            """;

        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabImportedProject project = await repository.ImportFromGitHubAsync(
            new ImportProjectFromGitHubRequest
            {
                PersonalAccessToken = "ghp_example",
                RepoId = 123456,
                TargetNamespace = "root",
                TimeoutStrategy = GitLabProjectImportTimeoutStrategy.Optimistic,
                PaginationLimit = 50
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/import/github", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("\"repo_id\":123456", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"timeout_strategy\":\"optimistic\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(27, project.Id);
        Assert.Equal("/root/my-repo/refs", project.RefsUrl);
        Assert.False(project.Forked);
    }

    [Fact]
    public async Task CancelGitHubImportAsync_PostsTheCancelRoute_AndDeserializesTheImportStateEnum()
    {
        const string json = """
                            {
                              "id": 160,
                              "name": "my-repo",
                              "full_path": "/root/my-repo",
                              "full_name": "Administrator / my-repo",
                              "import_source": "source/source-repo",
                              "import_status": "canceled",
                              "human_import_status_name": "canceled",
                              "provider_link": "/source/source-repo",
                              "relation_type": "owned"
                            }
                            """;

        using StubHttpMessageHandler handler = Json(json);
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabRemoteImportedProject project = await repository.CancelGitHubImportAsync(
            new CancelGitHubImportRequest { ProjectId = 160 }, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/import/github/cancel",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabRemoteImportState.Canceled, project.ImportStatus);
        Assert.Equal("source/source-repo", project.ImportSource);
        Assert.Equal("/source/source-repo", project.ProviderLink);
    }

    [Fact]
    public async Task ImportFromBitbucketAsync_PostsTheBitbucketCloudRoute()
    {
        const string json = """{"id": 5, "name": "repo", "import_status": "scheduled"}""";

        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabRemoteImportedProject project = await repository.ImportFromBitbucketAsync(
            new ImportProjectFromBitbucketRequest
            {
                BitbucketEmail = "user@example.invalid",
                BitbucketApiToken = "atl_example",
                RepoPath = "workspace/repo",
                TargetNamespace = "root"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/import/bitbucket",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("\"bitbucket_api_token\":\"atl_example\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"repo_path\":\"workspace/repo\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(GitLabRemoteImportState.Scheduled, project.ImportStatus);
    }

    [Fact]
    public async Task ImportFromBitbucketServerAsync_PostsTheBitbucketServerRoute()
    {
        const string json = """{"id": 6, "name": "repo", "forked": false}""";

        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabImportedProject project = await repository.ImportFromBitbucketServerAsync(
            new ImportProjectFromBitbucketServerRequest
            {
                BitbucketServerUrl = new Uri("https://bitbucket.example.invalid"),
                BitbucketServerUsername = "svc",
                PersonalAccessToken = "bb_example",
                BitbucketServerProject = "KEY",
                BitbucketServerRepo = "repo",
                TimeoutStrategy = GitLabProjectImportTimeoutStrategy.Pessimistic
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/import/bitbucket_server",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(sentBody);
        Assert.Contains("\"bitbucket_server_project\":\"KEY\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"timeout_strategy\":\"pessimistic\"", sentBody, StringComparison.Ordinal);
        Assert.Equal(6, project.Id);
    }

    [Theory]
    [InlineData(GitLabProjectTemplateType.Dockerfiles, "dockerfiles")]
    [InlineData(GitLabProjectTemplateType.Gitignores, "gitignores")]
    [InlineData(GitLabProjectTemplateType.GitlabCiYmls, "gitlab_ci_ymls")]
    [InlineData(GitLabProjectTemplateType.Licenses, "licenses")]
    [InlineData(GitLabProjectTemplateType.Issues, "issues")]
    [InlineData(GitLabProjectTemplateType.MergeRequests, "merge_requests")]
    public async Task ListTemplatesAsync_ProjectsEveryTemplateTypeOntoItsWireName(
        GitLabProjectTemplateType type, string wireName)
    {
        using StubHttpMessageHandler handler = Json("""[{"key":"mit","name":"MIT License"}]""");
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabProjectTemplate> templates = [];
        await foreach (GitLabProjectTemplate template in
                       repository.ListTemplatesAsync(1, type, TestContext.Current.CancellationToken))
        {
            templates.Add(template);
        }

        Assert.Equal(
            string.Create(CultureInfo.InvariantCulture,
                $"https://gitlab.example/api/v4/projects/1/templates/{wireName}"),
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabProjectTemplate only = Assert.Single(templates);
        Assert.Equal("mit", only.Key);
        Assert.Equal("MIT License", only.Name);
    }

    [Fact]
    public async Task GetTemplateAsync_EscapesASlashBearingName_AndBuildsThePlaceholderQuery()
    {
        const string json = """
                            {
                              "key": "mit",
                              "name": "MIT License",
                              "nickname": null,
                              "popular": true,
                              "html_url": "http://choosealicense.com/licenses/mit/",
                              "source_url": "https://opensource.org/licenses/MIT",
                              "description": "A short and simple permissive license",
                              "conditions": ["include-copyright"],
                              "permissions": ["commercial-use", "modifications"],
                              "limitations": ["liability", "warranty"],
                              "content": "MIT License\n\nCopyright (c) 2024 Example"
                            }
                            """;

        using StubHttpMessageHandler handler = Json(json);
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabProjectTemplateDetail template = await repository.GetTemplateAsync(
            "gitlab-org/gitlab-test",
            GitLabProjectTemplateType.MergeRequests,
            "team/Default Template",
            new ProjectTemplateOptions { SourceTemplateProjectId = 12, Project = "Example", Fullname = "Example B.V." },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab-test/templates/merge_requests"
            + "/team%2FDefault%20Template"
            + "?source_template_project_id=12&project=Example&fullname=Example%20B.V.",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("mit", template.Key);
        Assert.True(template.Popular);
        Assert.Equal(["include-copyright"], template.Conditions);
        Assert.Equal(2, template.Permissions?.Count);
        Assert.Contains("MIT License", template.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetTemplateAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = Json("""{"key":"mit","name":"MIT License"}""");
        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        ProjectImportRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.GetTemplateAsync(1, GitLabProjectTemplateType.Licenses, "mit",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/templates/licenses/mit",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    private static StubHttpMessageHandler Json(string payload)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        });
    }

    /// <summary>
    ///     Reads the request body before answering, which <see cref="StubHttpMessageHandler" /> cannot do
    ///     for a multipart upload: <see cref="HttpRequestMessage.Dispose()" /> disposes its content, so the
    ///     body is gone by the time the call returns.
    /// </summary>
    private sealed class CapturingHttpMessageHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? RequestContentType { get; private set; }

        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            RequestContentType = request.Content?.Headers.ContentType?.ToString();

            if (request.Content is not null)
            {
                RequestBody = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }

            HttpResponseMessage response = respond();
            response.RequestMessage ??= request;
            return response;
        }
    }
}