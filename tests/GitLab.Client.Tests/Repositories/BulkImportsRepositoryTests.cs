using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class BulkImportsRepositoryTests
{
    private const string BulkImportJson = """
                                          {
                                            "id": 1,
                                            "status": "finished",
                                            "source_type": "gitlab",
                                            "source_url": "https://source.gitlab.com/",
                                            "created_at": "2012-05-28T04:42:42-07:00",
                                            "updated_at": "2012-05-28T04:44:42-07:00",
                                            "has_failures": false
                                          }
                                          """;

    private const string EntityJson = """
                                      {
                                        "id": 7,
                                        "bulk_import_id": 1,
                                        "status": "created",
                                        "entity_type": "project",
                                        "source_full_path": "source_group/source_project",
                                        "destination_full_path": "some_group/source_project",
                                        "destination_name": "destination_slug",
                                        "destination_slug": "destination_slug",
                                        "destination_namespace": "destination_path",
                                        "parent_id": 3,
                                        "namespace_id": 4,
                                        "project_id": 5,
                                        "created_at": "2012-05-28T04:42:42-07:00",
                                        "updated_at": "2012-05-28T04:44:42-07:00",
                                        "failures": [
                                          {
                                            "relation": "label",
                                            "exception_message": "error message",
                                            "exception_class": "Exception",
                                            "correlation_id_value": "dfcf583058ed4508e4c7c617bd7f0edd",
                                            "source_url": "https://source.gitlab.com/group/-/epics/1",
                                            "source_title": "title"
                                          }
                                        ],
                                        "migrate_projects": true,
                                        "migrate_memberships": false,
                                        "has_failures": true,
                                        "stats": { "labels": { "source": 10, "imported": 9 } }
                                      }
                                      """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task ListAsync_BuildsTheMigrationRoute_AndDeserializesTheMigration()
    {
        string json = $"[{BulkImportJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        List<GitLabBulkImport> imports = [];
        await foreach (GitLabBulkImport item in repository.ListAsync(
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            imports.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/bulk_imports",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBulkImport import = Assert.Single(imports);
        Assert.Equal(1, import.Id);
        Assert.Equal(GitLabBulkImportStatus.Finished, import.Status);
        Assert.Equal("gitlab", import.SourceType);
        Assert.Equal("https://source.gitlab.com/", import.SourceUrl?.OriginalString);
        Assert.False(import.HasFailures);
        Assert.Equal(new DateTimeOffset(2012, 5, 28, 4, 42, 42, TimeSpan.FromHours(-7)), import.CreatedAt);
    }

    [Fact]
    public async Task ListAsync_ProjectsTheOptionsOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        BulkImportListOptions options = new()
        {
            Sort = GitLabBulkImportSort.Asc, Status = GitLabBulkImportStatus.Timeout, Page = 2, PerPage = 50
        };

        await foreach (GitLabBulkImport _ in repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/bulk_imports?sort=asc&status=timeout&page=2&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAsync_CanFilterByCanceled_WhichTheResponseSchemaOmitsButTheCancelEndpointProduces()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""[{"id":1,"status":"canceled"}]""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        List<GitLabBulkImport> imports = [];
        await foreach (GitLabBulkImport item in repository.ListAsync(
                           new BulkImportListOptions { Status = GitLabBulkImportStatus.Canceled },
                           TestContext.Current.CancellationToken))
        {
            imports.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/bulk_imports?status=canceled",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabBulkImportStatus.Canceled, Assert.Single(imports).Status);
    }

    [Fact]
    public async Task CreateAsync_PostsTheSourceConfigurationAndEntities()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(BulkImportJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        CreateBulkImportRequest request = new()
        {
            Configuration = new BulkImportConfiguration
            {
                Url = new Uri("https://source.gitlab.example"), AccessToken = "glpat-source-token"
            },
            Entities =
            [
                new BulkImportEntityRequest
                {
                    SourceType = GitLabBulkImportEntitySourceType.GroupEntity,
                    SourceFullPath = "source/full/path",
                    DestinationNamespace = "destination/namespace",
                    DestinationSlug = "destination-slug",
                    MigrateProjects = false,
                    MigrateMemberships = true
                }
            ]
        };

        GitLabBulkImport import = await repository.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/bulk_imports", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        Assert.NotNull(sentBody);
        using JsonDocument document = JsonDocument.Parse(sentBody!);
        JsonElement configuration = document.RootElement.GetProperty("configuration");
        Assert.Equal("https://source.gitlab.example", configuration.GetProperty("url").GetString());
        Assert.Equal("glpat-source-token", configuration.GetProperty("access_token").GetString());

        JsonElement entity = Assert.Single(document.RootElement.GetProperty("entities").EnumerateArray());

        // The request vocabulary is "group_entity"/"project_entity", not the response's "group"/"project".
        Assert.Equal("group_entity", entity.GetProperty("source_type").GetString());
        Assert.Equal("source/full/path", entity.GetProperty("source_full_path").GetString());
        Assert.Equal("destination/namespace", entity.GetProperty("destination_namespace").GetString());
        Assert.Equal("destination-slug", entity.GetProperty("destination_slug").GetString());
        Assert.False(entity.GetProperty("migrate_projects").GetBoolean());
        Assert.True(entity.GetProperty("migrate_memberships").GetBoolean());

        Assert.Equal(1, import.Id);
    }

    [Fact]
    public void BulkImportConfiguration_ToString_DoesNotPrintTheSourceAccessToken()
    {
        BulkImportConfiguration configuration = new()
        {
            Url = new Uri("https://source.gitlab.example"), AccessToken = "glpat-source-token"
        };

        string text = configuration.ToString();

        Assert.DoesNotContain("glpat-source-token", text, StringComparison.Ordinal);
        Assert.Contains("[redacted]", text, StringComparison.Ordinal);
        Assert.Contains("https://source.gitlab.example", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ImportGitHubGistsRequest_ToString_DoesNotPrintTheGitHubToken()
    {
        ImportGitHubGistsRequest request = new() { PersonalAccessToken = "ghp-secret" };

        string text = request.ToString();

        Assert.DoesNotContain("ghp-secret", text, StringComparison.Ordinal);
        Assert.Contains("[redacted]", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAsync_BuildsTheMigrationRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(BulkImportJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        GitLabBulkImport import = await repository.GetAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/bulk_imports/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabBulkImportStatus.Finished, import.Status);
    }

    [Fact]
    public async Task CancelAsync_PostsAnEmptyBody_AndReturnsTheUpdatedMigration()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":1,"status":"canceled"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        GitLabBulkImport import = await repository.CancelAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/bulk_imports/1/cancel",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabBulkImportStatus.Canceled, import.Status);
    }

    [Fact]
    public async Task ListEntitiesAsync_BuildsTheInstanceWideEntityRoute_AndDeserializesTheEntity()
    {
        string json = $"[{EntityJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        List<GitLabBulkImportEntity> entities = [];
        await foreach (GitLabBulkImportEntity item in repository.ListEntitiesAsync(
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            entities.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/bulk_imports/entities",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBulkImportEntity entity = Assert.Single(entities);
        Assert.Equal(7, entity.Id);
        Assert.Equal(1, entity.BulkImportId);
        Assert.Equal(GitLabBulkImportStatus.Created, entity.Status);
        Assert.Equal(GitLabBulkImportEntityType.Project, entity.EntityType);
        Assert.Equal("source_group/source_project", entity.SourceFullPath);
        Assert.Equal("some_group/source_project", entity.DestinationFullPath);
        Assert.Equal("destination_path", entity.DestinationNamespace);
        Assert.Equal(3, entity.ParentId);
        Assert.Equal(4, entity.NamespaceId);
        Assert.Equal(5, entity.ProjectId);
        Assert.True(entity.MigrateProjects);
        Assert.False(entity.MigrateMemberships);
        Assert.True(entity.HasFailures);

        GitLabBulkImportEntityFailure failure = Assert.Single(entity.Failures!);
        Assert.Equal("label", failure.Relation);
        Assert.Equal("Exception", failure.ExceptionClass);

        // "stats" is a bare object in the spec, so it stays raw JSON rather than a modelled shape.
        Assert.NotNull(entity.Stats);
        Assert.Equal(9, entity.Stats!.Value.GetProperty("labels").GetProperty("imported").GetInt32());
    }

    [Fact]
    public async Task ListEntitiesForImportAsync_ScopesToOneMigration_AndFiltersByStatus()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        BulkImportEntityListOptions options = new() { Status = GitLabBulkImportStatus.Failed, Page = 3, PerPage = 20 };

        await foreach (GitLabBulkImportEntity _ in
                       repository.ListEntitiesForImportAsync(1, options, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/bulk_imports/1/entities?status=failed&page=3&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetEntityAsync_BuildsTheNestedEntityRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(EntityJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        GitLabBulkImportEntity entity = await repository.GetEntityAsync(1, 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/bulk_imports/1/entities/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(7, entity.Id);
    }

    [Fact]
    public async Task ListEntityFailuresAsync_BuildsTheFailuresRoute_AndDeserializesTheFailure()
    {
        const string Json = """
                            [
                              {
                                "relation": "label",
                                "exception_message": "error message",
                                "exception_class": "Exception",
                                "correlation_id_value": "dfcf583058ed4508e4c7c617bd7f0edd",
                                "source_url": "https://source.gitlab.com/group/-/epics/1",
                                "source_title": "title"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        List<GitLabBulkImportEntityFailure> failures = [];
        await foreach (GitLabBulkImportEntityFailure item in
                       repository.ListEntityFailuresAsync(1, 7, TestContext.Current.CancellationToken))
        {
            failures.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/bulk_imports/1/entities/7/failures",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBulkImportEntityFailure failure = Assert.Single(failures);
        Assert.Equal("label", failure.Relation);
        Assert.Equal("error message", failure.ExceptionMessage);
        Assert.Equal("dfcf583058ed4508e4c7c617bd7f0edd", failure.CorrelationIdValue);
        Assert.Equal("https://source.gitlab.com/group/-/epics/1", failure.SourceUrl?.OriginalString);
        Assert.Equal("title", failure.SourceTitle);
    }

    [Fact]
    public async Task ImportGitHubGistsAsync_PostsTheTokenToTheGistImportRoute()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        await repository.ImportGitHubGistsAsync(new ImportGitHubGistsRequest { PersonalAccessToken = "ghp-secret" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/import/github/gists",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"personal_access_token":"ghp-secret"}""", sentBody);
    }

    [Fact]
    public async Task ListOfflineExportsAsync_BuildsTheOfflineExportRoute_AndFiltersByStatus()
    {
        const string Json = """
                            [
                              {
                                "id": 3,
                                "status": "finished",
                                "bucket": "gitlab-offline-transfer-acme",
                                "export_prefix": "2026-09-01",
                                "created_at": "2012-05-28T04:42:42-07:00",
                                "updated_at": "2012-05-28T04:44:42-07:00",
                                "has_failures": false
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        OfflineExportListOptions options = new()
        {
            Sort = GitLabBulkImportSort.Desc, Status = GitLabOfflineExportStatus.Finished, Page = 4, PerPage = 10
        };

        List<GitLabOfflineExport> exports = [];
        await foreach (GitLabOfflineExport item in
                       repository.ListOfflineExportsAsync(options, TestContext.Current.CancellationToken))
        {
            exports.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/offline_exports?sort=desc&status=finished&page=4&per_page=10",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabOfflineExport export = Assert.Single(exports);
        Assert.Equal(3, export.Id);

        // The spec declares no response entity for this route, so Status stays a bare string.
        Assert.Equal("finished", export.Status);
        Assert.Equal("gitlab-offline-transfer-acme", export.Bucket);
        Assert.Equal("2026-09-01", export.ExportPrefix);
    }

    [Fact]
    public async Task ListOfflineExportsAsync_ToleratesAStatusTheFilterVocabularyDoesNotName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""[{"id":3,"status":"some_future_state"}]""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        List<GitLabOfflineExport> exports = [];
        await foreach (GitLabOfflineExport item in
                       repository.ListOfflineExportsAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            exports.Add(item);
        }

        Assert.Equal("some_future_state", Assert.Single(exports).Status);
    }

    [Fact]
    public async Task CreateOfflineExportAsync_PostsTheBucketConfigurationAndEntities()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        CreateOfflineExportRequest request = new()
        {
            Bucket = "gitlab-offline-transfer-acme",
            Entities = [new OfflineExportEntityRequest { FullPath = "source/full/path" }],
            AwsS3Configuration = new OfflineTransferAwsS3Configuration
            {
                AwsAccessKeyId = "AKIA", AwsSecretAccessKey = "s3cr3t", Region = "eu-west-1", PathStyle = true
            }
        };

        await repository.CreateOfflineExportAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/offline_exports",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        using JsonDocument document = JsonDocument.Parse(sentBody!);
        Assert.Equal("gitlab-offline-transfer-acme", document.RootElement.GetProperty("bucket").GetString());

        JsonElement s3 = document.RootElement.GetProperty("aws_s3_configuration");
        Assert.Equal("AKIA", s3.GetProperty("aws_access_key_id").GetString());
        Assert.Equal("s3cr3t", s3.GetProperty("aws_secret_access_key").GetString());
        Assert.Equal("eu-west-1", s3.GetProperty("region").GetString());
        Assert.True(s3.GetProperty("path_style").GetBoolean());

        JsonElement entity = Assert.Single(document.RootElement.GetProperty("entities").EnumerateArray());
        Assert.Equal("source/full/path", entity.GetProperty("full_path").GetString());

        // The unset alternatives must be omitted, not sent as null: GitLab rejects more than one.
        Assert.False(document.RootElement.TryGetProperty("gcs_configuration", out _));
        Assert.False(document.RootElement.TryGetProperty("s3_compatible_configuration", out _));
    }

    [Fact]
    public void OfflineTransferAwsS3Configuration_ToString_DoesNotPrintTheSecretAccessKey()
    {
        OfflineTransferAwsS3Configuration configuration = new()
        {
            AwsAccessKeyId = "AKIA", AwsSecretAccessKey = "s3cr3t", Region = "eu-west-1"
        };

        string text = configuration.ToString();

        Assert.DoesNotContain("s3cr3t", text, StringComparison.Ordinal);
        Assert.Contains("[redacted]", text, StringComparison.Ordinal);
        Assert.Contains("AKIA", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetOfflineExportAsync_BuildsTheOfflineExportRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":3,"status":"started"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        GitLabOfflineExport export = await repository.GetOfflineExportAsync(3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/offline_exports/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3, export.Id);
        Assert.Equal("started", export.Status);
    }

    [Fact]
    public async Task CreateOfflineImportAsync_PostsTheExportPrefix_AndReturnsAMigration()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(BulkImportJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        CreateOfflineImportRequest request = new()
        {
            Bucket = "gitlab-offline-transfer-acme",
            ExportPrefix = "2026-09-01",
            Entities =
            [
                new OfflineImportEntityRequest
                {
                    SourceType = GitLabBulkImportEntitySourceType.ProjectEntity,
                    SourceFullPath = "source/full/path",
                    DestinationNamespace = "destination/namespace",
                    DestinationSlug = "destination-slug"
                }
            ],
            GcsAdcConfiguration = new OfflineTransferGcsAdcConfiguration { GoogleProject = "acme-prod" }
        };

        GitLabBulkImport import =
            await repository.CreateOfflineImportAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/offline_imports",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(sentBody);
        using JsonDocument document = JsonDocument.Parse(sentBody!);
        Assert.Equal("2026-09-01", document.RootElement.GetProperty("export_prefix").GetString());
        Assert.Equal("acme-prod",
            document.RootElement.GetProperty("gcs_adc_configuration").GetProperty("google_project").GetString());

        JsonElement entity = Assert.Single(document.RootElement.GetProperty("entities").EnumerateArray());
        Assert.Equal("project_entity", entity.GetProperty("source_type").GetString());

        Assert.Equal(1, import.Id);
    }

    [Fact]
    public async Task GetAsync_OnAMissingMigration_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(404, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_OnUnprocessableEntity_ThrowsGitLabValidationException_WithoutEchoingTheToken()
    {
        const string Json = """{ "message": { "configuration.url": ["must be a valid URL"] } }""";

        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        BulkImportsRepository repository = new(connection);

        CreateBulkImportRequest request = new()
        {
            Configuration = new BulkImportConfiguration
            {
                Url = new Uri("https://source.gitlab.example"), AccessToken = "glpat-source-token"
            },
            Entities =
            [
                new BulkImportEntityRequest
                {
                    SourceType = GitLabBulkImportEntitySourceType.GroupEntity,
                    SourceFullPath = "source/full/path",
                    DestinationNamespace = "destination"
                }
            ]
        };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Contains("must be a valid URL", exception.Message, StringComparison.Ordinal);

        // The transport must never fold the request body - which carries the source credential - into the
        // exception it raises.
        Assert.DoesNotContain("glpat-source-token", exception.ToString(), StringComparison.Ordinal);
    }
}