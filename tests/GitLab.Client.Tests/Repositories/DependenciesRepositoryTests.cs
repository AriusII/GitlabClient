using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class DependenciesRepositoryTests
{
    private const string DependencyJson = """
                                          {
                                            "name": "@scope/package",
                                            "version": "5.0.1",
                                            "package_manager": "npm",
                                            "dependency_file_path": "package-lock.json",
                                            "vulnerabilities": [
                                              {
                                                "name": "DDoS",
                                                "severity": "unknown",
                                                "id": 144827,
                                                "url": "https://gitlab.example/group/project/-/security/vulnerabilities/144827"
                                              }
                                            ],
                                            "licenses": [
                                              {
                                                "spdx_identifier": "MIT",
                                                "name": "MIT License",
                                                "url": "https://opensource.org/licenses/MIT"
                                              }
                                            ],
                                            "malware": false
                                          }
                                          """;

    private const string ExportJson = """
                                      {
                                        "id": 2,
                                        "has_finished": false,
                                        "self": "https://gitlab.example/api/v4/dependency_list_exports/2",
                                        "download": "https://gitlab.example/api/v4/dependency_list_exports/2/download"
                                      }
                                      """;

    private const string SbomScanJson = """
                                        {
                                          "id": 9,
                                          "download_url": "https://gitlab.example/scan/9",
                                          "throttled": false,
                                          "project_throttling_resets_in": 30,
                                          "advisory_db_state": "up_to_date"
                                        }
                                        """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task ListAsync_BuildsTheDependenciesRoute_AndDeserializesTheNestedEntries()
    {
        string json = $"[{DependencyJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        List<GitLabDependency> dependencies = [];
        await foreach (GitLabDependency item in
                       repository.ListAsync(1, cancellationToken: TestContext.Current.CancellationToken))
        {
            dependencies.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/dependencies",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabDependency dependency = Assert.Single(dependencies);
        Assert.Equal("@scope/package", dependency.Name);
        Assert.Equal("5.0.1", dependency.Version);
        Assert.Equal("npm", dependency.PackageManager);
        Assert.Equal("package-lock.json", dependency.DependencyFilePath);
        Assert.False(dependency.Malware);

        GitLabDependencyLicense license = Assert.Single(dependency.Licenses!);
        Assert.Equal("MIT", license.SpdxIdentifier);
        Assert.Equal("MIT License", license.Name);
        Assert.Equal("https://opensource.org/licenses/MIT", license.Url?.AbsoluteUri);

        GitLabDependencyVulnerability vulnerability = Assert.Single(dependency.Vulnerabilities!);
        Assert.Equal(144827, vulnerability.Id);
        Assert.Equal("DDoS", vulnerability.Name);
        Assert.Equal("unknown", vulnerability.Severity);
    }

    [Fact]
    public async Task ListAsync_EncodesTheNamespacedProjectPath_AndRepeatsThePackageManagerFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        DependencyListOptions options = new() { PackageManager = ["npm", "maven"], PerPage = 20 };

        await foreach (GitLabDependency _ in repository.ListAsync("gitlab-org/gitlab", options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        // GitLab types package_manager as an array, and Grape wants the repeated form.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/dependencies"
            + "?package_manager[]=npm&package_manager[]=maven&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAsync_ToleratesAPackageManagerGitLabAddedLater()
    {
        // package_manager is a bare string in the spec, so a new ecosystem must not fail the response.
        const string Json = """
                            [
                              {
                                "name": "left-pad",
                                "version": "1.0.0",
                                "package_manager": "a-brand-new-ecosystem"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        List<GitLabDependency> dependencies = [];
        await foreach (GitLabDependency item in
                       repository.ListAsync(1, cancellationToken: TestContext.Current.CancellationToken))
        {
            dependencies.Add(item);
        }

        GitLabDependency dependency = Assert.Single(dependencies);
        Assert.Equal("a-brand-new-ecosystem", dependency.PackageManager);
        Assert.Null(dependency.Licenses);
        Assert.Null(dependency.Malware);
    }

    [Fact]
    public async Task CreateProjectExportAsync_PostsTheExportTypeAsItsWireValue()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ExportJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        CreateProjectDependencyListExportRequest request = new()
        {
            SendEmail = true, ExportType = GitLabProjectDependencyListExportType.CycloneDx16Json
        };

        GitLabDependencyListExport export =
            await repository.CreateProjectExportAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/dependency_list_exports",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"send_email":true,"export_type":"cyclonedx_1_6_json"}""", sentBody);

        Assert.Equal(2, export.Id);
        Assert.False(export.HasFinished);
        Assert.Equal("https://gitlab.example/api/v4/dependency_list_exports/2/download",
            export.Download?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateGroupExportAsync_PostsToTheGroupRoute_WithTheGroupVocabulary()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ExportJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        CreateGroupDependencyListExportRequest request = new()
        {
            ExportType = GitLabGroupDependencyListExportType.JsonArray
        };

        await repository.CreateGroupExportAsync("parent-group/subgroup", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/dependency_list_exports",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"export_type":"json_array"}""", sentBody);
    }

    [Fact]
    public async Task CreatePipelineExportAsync_PostsToThePipelineRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ExportJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        CreatePipelineDependencyListExportRequest request = new()
        {
            ExportType = GitLabPipelineDependencyListExportType.Sbom
        };

        await repository.CreatePipelineExportAsync(361, request, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/pipelines/361/dependency_list_exports",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"export_type":"sbom"}""", sentBody);
    }

    [Fact]
    public async Task GetExportAsync_BuildsTheTopLevelExportRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ExportJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        GitLabDependencyListExport export =
            await repository.GetExportAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/dependency_list_exports/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, export.Id);
    }

    [Fact]
    public async Task DownloadExportAsync_StreamsTheExportFile()
    {
        byte[] sbom = Encoding.UTF8.GetBytes("""{"bomFormat":"CycloneDX"}""");

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(sbom)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        using GitLabFileResponse export =
            await repository.DownloadExportAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/dependency_list_exports/2/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using MemoryStream copy = new();
        await export.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);

        Assert.Equal(sbom, copy.ToArray());
    }

    [Fact]
    public async Task ListOccurrenceVulnerabilitiesAsync_PassesTheOccurrenceIdAsAQueryParameter()
    {
        const string Json = """
                            [
                              {
                                "id": "144827",
                                "name": "Regular expression denial of service",
                                "severity": "high",
                                "url": "https://gitlab.example/x/-/security/vulnerabilities/144827"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        List<GitLabDependencyVulnerability> vulnerabilities = [];
        await foreach (GitLabDependencyVulnerability item in
                       repository.ListOccurrenceVulnerabilitiesAsync("gid://gitlab/Sbom::Occurrence/1",
                           TestContext.Current.CancellationToken))
        {
            vulnerabilities.Add(item);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/occurrences/vulnerabilities?id=gid%3A%2F%2Fgitlab%2FSbom%3A%3AOccurrence%2F1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The spec types the vulnerability id as a string while GitLab sends a number; the context runs
        // with NumberHandling.AllowReadingFromString, so both wire shapes land on the same long.
        GitLabDependencyVulnerability vulnerability = Assert.Single(vulnerabilities);
        Assert.Equal(144827, vulnerability.Id);
        Assert.Equal("high", vulnerability.Severity);
    }

    [Fact]
    public async Task ListAttestationsAsync_EscapesTheSubjectDigest_AndDeserializesTheEntries()
    {
        const string Json = """
                            [
                              {
                                "id": 12,
                                "iid": 3,
                                "created_at": "2025-11-04T08:15:00.000Z",
                                "updated_at": "2025-11-04T08:16:00.000Z",
                                "expire_at": "2026-11-04T08:15:00.000Z",
                                "project_id": 7,
                                "build_id": 9001,
                                "status": "success",
                                "predicate_kind": "provenance",
                                "predicate_type": "https://slsa.dev/provenance/v1",
                                "subject_digest": "sha256:9f86d081884c7d659a2feaa0c55ad015",
                                "download_url": "https://gitlab.example/api/v4/projects/7/attestations/3/download"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        List<GitLabAttestation> attestations = [];
        await foreach (GitLabAttestation item in repository.ListAttestationsAsync(7,
                           "sha256:9f86d081884c7d659a2feaa0c55ad015", TestContext.Current.CancellationToken))
        {
            attestations.Add(item);
        }

        // A digest is caller-supplied free text: the colon must be percent-encoded, not pasted in.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/attestations/sha256%3A9f86d081884c7d659a2feaa0c55ad015",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabAttestation attestation = Assert.Single(attestations);
        Assert.Equal(12, attestation.Id);
        Assert.Equal(3, attestation.Iid);
        Assert.Equal(9001, attestation.BuildId);
        Assert.Equal("provenance", attestation.PredicateKind);
        Assert.Equal("https://slsa.dev/provenance/v1", attestation.PredicateType);
    }

    [Fact]
    public async Task DownloadAttestationAsync_BuildsTheDownloadRoute()
    {
        byte[] bundle = [0x7B, 0x7D];

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bundle)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        using GitLabFileResponse response =
            await repository.DownloadAttestationAsync("gitlab-org/gitlab", 3,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/attestations/3/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using MemoryStream copy = new();
        await response.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);

        Assert.Equal(bundle, copy.ToArray());
    }

    [Fact]
    public async Task ScanFileAsync_EscapesTheScannerEndpoint_AndKeepsTheReportAsRawFields()
    {
        const string Json = """
                            {
                              "vulnerabilities": [{ "id": "1", "name": "Hardcoded password" }],
                              "scanner": "semgrep"
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

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        SastFileScanRequest request = new() { FilePath = "app/models/user.rb", Content = "password = hunter2" };

        JsonElement result =
            await repository.ScanFileAsync(1, "semgrep/scan", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/security_scans/sast/semgrep%2Fscan",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"file_path":"app/models/user.rb","content":"password = hunter2"}""",
            sentBody);

        // GitLab declares no response schema for this experiment, so the whole report is kept verbatim
        // rather than squeezed into a DTO invented from a shape the spec does not promise.
        Assert.Equal("semgrep", result.GetProperty("scanner").GetString());
        Assert.Equal(JsonValueKind.Array, result.GetProperty("vulnerabilities").ValueKind);
    }

    [Fact]
    public async Task AuthorizeSbomScanUploadAsync_SendsFilesizeAsOneWord()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        await repository.AuthorizeSbomScanUploadAsync(88, new AuthorizeSbomScanUploadRequest { FileSize = 4096 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/jobs/88/sbom_scans/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The snake-case policy would have written "file_size"; GitLab spells it as one word.
        Assert.Equal("""{"filesize":4096}""", sentBody);
    }

    [Fact]
    public async Task UploadSbomScanAsync_SendsMultipartWithTheDigestAsAFormField()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SbomScanJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        using MemoryStream content = new(Encoding.UTF8.GetBytes("""{"bomFormat":"CycloneDX"}"""));
        GitLabFileUpload file = new()
        {
            Content = content, FileName = "gl-sbom.cdx.json", ContentType = "application/json"
        };

        GitLabSbomScan scan = await repository.UploadSbomScanAsync(88, file, "sha256:abcdef",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/jobs/88/sbom_scans",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("gl-sbom.cdx.json", sentBody, StringComparison.Ordinal);
        Assert.Contains("sbom_digest", sentBody, StringComparison.Ordinal);
        Assert.Contains("sha256:abcdef", sentBody, StringComparison.Ordinal);

        // The upload's stream is borrowed, never owned.
        Assert.True(content.CanRead);

        Assert.Equal(9, scan.Id);
        Assert.False(scan.Throttled);
        Assert.Equal(30, scan.ProjectThrottlingResetsIn);
        Assert.Equal("up_to_date", scan.AdvisoryDbState);
    }

    [Fact]
    public async Task GetSbomScanAsync_EscapesTheDigest()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SbomScanJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        GitLabSbomScan scan =
            await repository.GetSbomScanAsync(88, "sha256:abcdef", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/jobs/88/sbom_scans/sha256%3Aabcdef",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("https://gitlab.example/scan/9", scan.DownloadUrl?.AbsoluteUri);
    }

    [Fact]
    public async Task ReuseSbomScanAsync_PostsThePurlTypesToTheDigestRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SbomScanJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        ReuseSbomScanRequest request = new() { PurlTypes = ["npm", "maven"] };

        await repository.ReuseSbomScanAsync(88, "sha256:abcdef", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/jobs/88/sbom_scans/sha256%3Aabcdef",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"purl_types":["npm","maven"]}""", sentBody);
    }

    [Fact]
    public async Task ListAsync_OnAPlanWithoutDependencyScanning_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        DependenciesRepository repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabDependency _ in repository
                               .ListAsync(1, cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                Assert.Fail("The stubbed response is a failure.");
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}