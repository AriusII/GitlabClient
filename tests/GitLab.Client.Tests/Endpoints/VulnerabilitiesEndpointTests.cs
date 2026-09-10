using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class VulnerabilitiesEndpointTests
{
    private const string VulnerabilityJson = """
                                             {
                                               "id": 42,
                                               "title": "SQL injection in search",
                                               "description": "Unsanitized input reaches a query.",
                                               "state": "detected",
                                               "severity": "high",
                                               "report_type": "sast",
                                               "resolved_on_default_branch": false,
                                               "created_at": "2026-01-02T03:04:05.000Z"
                                             }
                                             """;

    private const string VulnerabilityExportJson = """
                                                   {
                                                     "id": 71,
                                                     "project_id": 5,
                                                     "format": "csv",
                                                     "status": "finished",
                                                     "send_email": true,
                                                     "created_at": "2026-01-02T03:04:05.000Z",
                                                     "expires_at": "2026-01-03T03:04:05.000Z",
                                                     "_links": { "self": "/security/vulnerability_exports/71" }
                                                   }
                                                   """;

    private const string ArchiveExportJson = """
                                             {
                                               "id": 72,
                                               "project_id": 5,
                                               "format": "csv",
                                               "status": "finished",
                                               "created_at": "2026-01-02T03:04:05.000Z",
                                               "_links": { "self": "/security/vulnerability_archive_exports/72" }
                                             }
                                             """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task ListAsync_EncodesTheProjectPath_AppliesPagination_AndDeserializesVulnerabilities()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{VulnerabilityJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabVulnerability> vulnerabilities = [];
        await foreach (GitLabVulnerability vulnerability in client.ListAsync("group/project",
                           new VulnerabilityListOptions { PerPage = 50 }, TestContext.Current.CancellationToken))
        {
            vulnerabilities.Add(vulnerability);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/group%2Fproject/vulnerabilities?per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabVulnerability only = Assert.Single(vulnerabilities);
        Assert.Equal(42, only.Id);
        Assert.Equal("SQL injection in search", only.Title);
        Assert.Equal("detected", only.State);
        Assert.Equal("high", only.Severity);
        Assert.Equal(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), only.CreatedAt);
    }

    [Fact]
    public async Task ListFindingsAsync_RepeatsArrayFilters_AndDeserializesTheFindingProjection()
    {
        const string Json = """
                            [
                              {
                                "id": "gid://gitlab/Vulnerabilities::Finding/9",
                                "report_type": "sast",
                                "name": "Unsafe deserialization",
                                "severity": "critical",
                                "scanner": { "external_id": "semgrep", "name": "Semgrep", "vendor": "Semgrep" },
                                "identifiers": {
                                  "external_type": "cve",
                                  "external_id": "CVE-2026-0001",
                                  "name": "CVE-2026-0001",
                                  "url": "https://cve.example/CVE-2026-0001"
                                },
                                "found_by_pipeline": { "iid": "123" }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));
        VulnerabilityFindingListOptions options = new()
        {
            ReportType = ["sast", "dast"],
            Scope = GitLabVulnerabilityFindingScope.Dismissed,
            Severity = ["high", "critical"],
            PipelineId = "123",
            PerPage = 20
        };

        List<GitLabVulnerabilityFinding> findings = [];
        await foreach (GitLabVulnerabilityFinding finding in client.ListFindingsAsync(5, options,
                           TestContext.Current.CancellationToken))
        {
            findings.Add(finding);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/5/vulnerability_findings"
            + "?report_type[]=sast&report_type[]=dast&scope=dismissed&severity[]=high&severity[]=critical"
            + "&pipeline_id=123&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabVulnerabilityFinding only = Assert.Single(findings);
        Assert.Equal("gid://gitlab/Vulnerabilities::Finding/9", only.Id);
        Assert.Equal("Semgrep", only.Scanner?.Name);
        Assert.Equal("CVE-2026-0001", only.Identifiers?.ExternalId);
        Assert.Equal("https://cve.example/CVE-2026-0001", only.Identifiers?.Url?.AbsoluteUri);
        Assert.Equal("123", only.FoundByPipeline?.Iid);
    }

    [Fact]
    public async Task CreateAsync_PostsTheConfirmedFindingId_AndDeserializesTheVulnerability()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse(HttpStatusCode.Created, VulnerabilityJson);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));

        GitLabVulnerability created = await client.CreateAsync(5, new CreateVulnerabilityRequest { FindingId = 33 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/vulnerabilities",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"finding_id":33}""", sentBody);
        Assert.Equal(42, created.Id);
    }

    [Theory]
    [InlineData("confirm")]
    [InlineData("resolve")]
    [InlineData("revert")]
    public async Task LifecycleActions_PostTheOptionalAuditComment_AndReturnTheUpdatedVulnerability(string action)
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse(HttpStatusCode.Created, VulnerabilityJson);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));
        VulnerabilityCommentRequest request = new() { Comment = "Reviewed by security." };

        GitLabVulnerability result = action switch
        {
            "confirm" => await client.ConfirmAsync(42, request, TestContext.Current.CancellationToken),
            "resolve" => await client.ResolveAsync(42, request, TestContext.Current.CancellationToken),
            "revert" => await client.RevertAsync(42, request, TestContext.Current.CancellationToken),
            _ => throw new InvalidOperationException($"Unexpected action '{action}'.")
        };

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/vulnerabilities/42/{action}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"comment":"Reviewed by security."}""", sentBody);
        Assert.Equal(42, result.Id);
    }

    [Fact]
    public async Task DismissAsync_PostsTheBodylessAction_AndReturnsTheUpdatedVulnerability()
    {
        using StubHttpMessageHandler handler = new(_ => JsonResponse(HttpStatusCode.Created, VulnerabilityJson));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));

        GitLabVulnerability dismissed = await client.DismissAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/vulnerabilities/42/dismiss",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(42, dismissed.Id);
    }

    [Fact]
    public async Task UpdateAiDetectionAsync_PostsTheNoContentEvidenceBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));

        await client.UpdateAiDetectionAsync(42,
            new UpdateVulnerabilityAiDetectionRequest
            {
                ConfidenceScore = 91,
                Description = "The sanitizer makes this a false positive.",
                Origin = "duo_sast_fp_detection"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/vulnerabilities/42/flags/ai_detection",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"confidence_score":91,"description":"The sanitizer makes this a false positive.","origin":"duo_sast_fp_detection"}""",
            sentBody);
    }

    [Fact]
    public async Task GetAsync_BuildsTheGlobalVulnerabilityRoute()
    {
        using StubHttpMessageHandler handler = new(_ => JsonResponse(HttpStatusCode.OK, VulnerabilityJson));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));

        GitLabVulnerability vulnerability = await client.GetAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/vulnerabilities/42", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("sast", vulnerability.ReportType);
    }

    [Fact]
    public async Task ListIssueLinksAsync_BuildsTheRelationshipRoute_AndDeserializesTheLinkMetadata()
    {
        const string Json = """
                            [
                              {
                                "id": 88,
                                "iid": 7,
                                "project_id": 5,
                                "title": "Track remediation",
                                "state": "opened",
                                "vulnerability_link_id": 6,
                                "vulnerability_link_type": "relates_to"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => JsonResponse(HttpStatusCode.OK, Json));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));

        List<GitLabVulnerabilityRelatedIssue> issues = [];
        await foreach (GitLabVulnerabilityRelatedIssue issue in client.ListIssueLinksAsync(42,
                           TestContext.Current.CancellationToken))
        {
            issues.Add(issue);
        }

        Assert.Equal("https://gitlab.example/api/v4/vulnerabilities/42/issue_links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabVulnerabilityRelatedIssue only = Assert.Single(issues);
        Assert.Equal(88, only.Id);
        Assert.Equal(6, only.VulnerabilityLinkId);
        Assert.Equal("relates_to", only.VulnerabilityLinkType);
    }

    [Fact]
    public async Task CreateIssueLinkAsync_PostsTheTargetIssue_AndDeserializesTheRelationship()
    {
        const string Json = """
                            {
                              "id": 6,
                              "link_type": "relates_to",
                              "vulnerability": { "id": 42, "title": "SQL injection" },
                              "issue": { "id": 88, "iid": 7, "title": "Track remediation" }
                            }
                            """;
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse(HttpStatusCode.Created, Json);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));

        GitLabVulnerabilityIssueLink link = await client.CreateIssueLinkAsync(42,
            new CreateVulnerabilityIssueLinkRequest
            {
                TargetIssueIid = 7, TargetProjectId = "5", LinkType = "relates_to"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/vulnerabilities/42/issue_links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"target_issue_iid":7,"target_project_id":"5","link_type":"relates_to"}""", sentBody);
        Assert.Equal(6, link.Id);
        Assert.Equal(42, link.Vulnerability?.Id);
        Assert.Equal(88, link.Issue?.Id);
    }

    [Fact]
    public async Task DeleteIssueLinkAsync_UsesTheSpecificRelationshipRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));

        await client.DeleteIssueLinkAsync(42, 6, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/vulnerabilities/42/issue_links/6",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Theory]
    [InlineData("project")]
    [InlineData("group")]
    [InlineData("instance")]
    public async Task StandardExportCreates_UseTheirSecurityScopeRoute_AndSerializeTheTypedFormat(string scope)
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse(HttpStatusCode.Created, VulnerabilityExportJson);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));
        CreateVulnerabilityExportRequest request = new()
        {
            ExportFormat = GitLabVulnerabilityExportFormat.Csv, SendEmail = true
        };

        GitLabVulnerabilityExport export = scope switch
        {
            "project" => await client.CreateProjectExportAsync("group/project", request,
                TestContext.Current.CancellationToken),
            "group" => await client.CreateGroupExportAsync("parent/subgroup", request,
                TestContext.Current.CancellationToken),
            "instance" => await client.CreateInstanceExportAsync(request, TestContext.Current.CancellationToken),
            _ => throw new InvalidOperationException($"Unexpected scope '{scope}'.")
        };

        string expectedRoute = scope switch
        {
            "project" => "https://gitlab.example/api/v4/security/projects/group%2Fproject/vulnerability_exports",
            "group" => "https://gitlab.example/api/v4/security/groups/parent%2Fsubgroup/vulnerability_exports",
            _ => "https://gitlab.example/api/v4/security/vulnerability_exports"
        };

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(expectedRoute, handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"export_format":"csv","send_email":true}""", sentBody);
        Assert.Equal(71, export.Id);
        Assert.Equal("finished", export.Status);
        Assert.True(export.Links.HasValue);
        Assert.True(export.Links.Value.TryGetProperty("self", out _));
    }

    [Fact]
    public async Task GetAndDownloadExportAsync_UseTheInstanceExportRoutes()
    {
        int requests = 0;
        byte[] csv = Encoding.UTF8.GetBytes("id,title\n42,SQL injection\n");
        using StubHttpMessageHandler handler = new(_ =>
        {
            requests++;
            return requests == 1
                ? JsonResponse(HttpStatusCode.OK, VulnerabilityExportJson)
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(csv) };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));

        GitLabVulnerabilityExport export = await client.GetExportAsync(71, TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/security/vulnerability_exports/71",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(71, export.Id);

        using GitLabFileResponse file = await client.DownloadExportAsync(71, TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/security/vulnerability_exports/71/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        using MemoryStream copy = new();
        await file.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(csv, copy.ToArray());
    }

    [Fact]
    public async Task ArchiveExportCreateGetAndDownloadAsync_UseTheArchiveRoutes_AndDateOnlyWireValues()
    {
        int requests = 0;
        byte[] csv = Encoding.UTF8.GetBytes("id,title\n42,Archived finding\n");
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            requests++;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return requests switch
            {
                1 => JsonResponse(HttpStatusCode.Created, ArchiveExportJson),
                2 => JsonResponse(HttpStatusCode.OK, ArchiveExportJson),
                _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(csv) }
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        VulnerabilitiesClient client = new(new GitLabApiConnection(httpClient));
        CreateVulnerabilityArchiveExportRequest request = new()
        {
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 12, 31),
            ExportFormat = GitLabVulnerabilityArchiveExportFormat.Csv
        };

        GitLabVulnerabilityArchiveExport created = await client.CreateProjectArchiveExportAsync("group/project",
            request, TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/security/projects/group%2Fproject/vulnerability_archive_exports",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"start_date":"2025-01-01","end_date":"2025-12-31","export_format":"csv"}""", sentBody);
        Assert.Equal(72, created.Id);

        GitLabVulnerabilityArchiveExport polled = await client.GetArchiveExportAsync(72,
            TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/security/vulnerability_archive_exports/72",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("finished", polled.Status);

        using GitLabFileResponse file = await client.DownloadArchiveExportAsync(72,
            TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/security/vulnerability_archive_exports/72/download",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        using MemoryStream copy = new();
        await file.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(csv, copy.ToArray());
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}