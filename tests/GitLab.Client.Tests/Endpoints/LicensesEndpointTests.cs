using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class LicensesEndpointTests
{
    private const string LicenseJson = """
                                       {
                                         "id": 2,
                                         "plan": "ultimate",
                                         "created_at": "2024-01-11T09:00:00.000Z",
                                         "starts_at": "2024-01-11",
                                         "expires_at": "2025-01-11",
                                         "historical_max": 300,
                                         "maximum_user_count": 300,
                                         "licensee": {
                                           "Name": "Jane Smith",
                                           "Email": "jane@example.com",
                                           "Company": "Example Inc"
                                         },
                                         "add_ons": {
                                           "GitLab_FileLocks": 1,
                                           "GitLab_Auditor_User": 1
                                         },
                                         "expired": false,
                                         "overage": 2,
                                         "user_limit": 300,
                                         "active_users": 302
                                       }
                                       """;

    private const string ManagedLicenseJson = """
                                              {
                                                "id": 5,
                                                "name": "MIT",
                                                "approval_status": "allowed"
                                              }
                                              """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetCurrentAsync_BuildsTheLicenseRoute_AndDeserializesThePayload()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(LicenseJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        GitLabLicense license = await repository.GetCurrentAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/license", handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(2, license.Id);
        Assert.Equal("ultimate", license.Plan);

        // starts_at/expires_at are plain dates on the wire, created_at is a timestamp.
        Assert.Equal(new DateOnly(2024, 1, 11), license.StartsAt);
        Assert.Equal(new DateOnly(2025, 1, 11), license.ExpiresAt);
        Assert.Equal(
            DateTimeOffset.Parse("2024-01-11T09:00:00Z", CultureInfo.InvariantCulture),
            license.CreatedAt);

        Assert.False(license.Expired);
        Assert.Equal(2, license.Overage);
        Assert.Equal(302, license.ActiveUsers);

        // "licensee" and "add_ons" are bare objects in the spec, and GitLab spells the licensee's keys
        // in PascalCase - forcing them into a DTO would have been a guess.
        Assert.Equal("Jane Smith", license.Licensee?.GetProperty("Name").GetString());
        Assert.Equal(1, license.AddOns?.GetProperty("GitLab_FileLocks").GetInt32());
    }

    [Fact]
    public async Task ListAsync_BuildsThePluralLicensesRoute_AndToleratesAMissingActiveUserCount()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "plan": "premium",
                                "starts_at": "2023-01-11",
                                "expires_at": "2024-01-11",
                                "user_limit": 100
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        List<GitLabLicense> licenses = [];
        await foreach (GitLabLicense item in repository.ListAsync(TestContext.Current.CancellationToken))
        {
            licenses.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/licenses", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabLicense license = Assert.Single(licenses);
        Assert.Equal(1, license.Id);

        // The listing entity has no active_users member; the single-licence entity does.
        Assert.Null(license.ActiveUsers);
    }

    [Fact]
    public async Task GetAsync_AppendsTheNumericLicenseId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(LicenseJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        GitLabLicense license = await repository.GetAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/license/42", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("ultimate", license.Plan);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheLicenseRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        await repository.DeleteAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/license/42", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_PostsTheLicenseKey_AndTheRequestNeverPrintsItInToString()
    {
        const string Key = "eyJkYXRhIjoiVGhlLXNlY3JldC1saWNlbmNlLWtleSJ9";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(LicenseJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        CreateLicenseRequest request = new() { License = Key };

        GitLabLicense license = await repository.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/license", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal($$"""{"license":"{{Key}}"}""", sentBody);
        Assert.Equal(2, license.Id);

        // The key is credential-adjacent: the record's compiler-generated ToString would leak it the
        // first time someone interpolated the request into a log line.
        Assert.DoesNotContain(Key, request.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RefreshBillableUsersAsync_PutsWithoutABody()
    {
        const string Json = """{ "success": true }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        GitLabLicenseRefreshResult result =
            await repository.RefreshBillableUsersAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/license/42/refresh_billable_users",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(JsonValueKind.True, result.Success?.ValueKind);
    }

    [Fact]
    public async Task DownloadUsageExportAsync_StreamsTheReportInsteadOfDeserializingIt()
    {
        byte[] csv = Encoding.UTF8.GetBytes("license_start_date,users\n2024-01-11,302\n");

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(csv)
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        using GitLabFileResponse export =
            await repository.DownloadUsageExportAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/license/usage_export",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using MemoryStream copy = new();
        await export.Content.CopyToAsync(copy, TestContext.Current.CancellationToken);

        Assert.Equal(csv, copy.ToArray());
    }

    [Fact]
    public async Task ListManagedAsync_EncodesTheNamespacedProjectPath_AndProjectsThePageSize()
    {
        string json = $"[{ManagedLicenseJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        List<GitLabManagedLicense> policies = [];
        await foreach (GitLabManagedLicense item in repository.ListManagedAsync("gitlab-org/gitlab",
                           new ManagedLicenseListOptions { PerPage = 50 }, TestContext.Current.CancellationToken))
        {
            policies.Add(item);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/managed_licenses?per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabManagedLicense policy = Assert.Single(policies);
        Assert.Equal(5, policy.Id);
        Assert.Equal("MIT", policy.Name);
        Assert.Equal("allowed", policy.ApprovalStatus);
    }

    [Fact]
    public async Task GetManagedAsync_EscapesALicenceNameCarryingSlashesAndDots()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ManagedLicenseJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        // GitLab accepts the licence name in place of the id, and licence names carry dots and slashes -
        // Escaped, never Literal.
        GitLabManagedLicense policy =
            await repository.GetManagedAsync(1, "Apache-2.0/GPL-3.0", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/managed_licenses/Apache-2.0%2FGPL-3.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("MIT", policy.Name);
    }

    [Fact]
    public async Task DeleteManagedAsync_EscapesTheIdentifier_AndSendsDelete()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        await repository.DeleteManagedAsync("gitlab-org/gitlab", "Apache-2.0 WITH LLVM-exception",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/managed_licenses/Apache-2.0%20WITH%20LLVM-exception",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateManagedAsync_WritesTheApprovalStatusAsItsWireValue()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ManagedLicenseJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        CreateManagedLicenseRequest request = new()
        {
            Name = "MIT", ApprovalStatus = GitLabManagedLicenseApprovalStatus.Allowed
        };

        GitLabManagedLicense policy =
            await repository.CreateManagedAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/managed_licenses",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"MIT","approval_status":"allowed"}""", sentBody);
        Assert.Equal(5, policy.Id);
    }

    [Fact]
    public async Task UpdateManagedAsync_UsesPatch_AndOmitsTheMembersLeftUnset()
    {
        const string Json = """
                            {
                              "id": 5,
                              "name": "MIT",
                              "approval_status": "denied"
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
        LicensesClient repository = new(connection);

        UpdateManagedLicenseRequest request = new() { ApprovalStatus = GitLabManagedLicenseApprovalStatus.Denied };

        GitLabManagedLicense policy =
            await repository.UpdateManagedAsync(1, "5", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/managed_licenses/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"approval_status":"denied"}""", sentBody);
        Assert.Equal("denied", policy.ApprovalStatus);
    }

    [Fact]
    public async Task GetManagedAsync_AcceptsTheLegacyApprovalStatusSpellings()
    {
        // approval_status is modelled as a string, not an enum: policies created before GitLab renamed
        // the vocabulary still come back as "approved"/"blacklisted", and an enum would throw on them.
        const string Json = """
                            {
                              "id": 7,
                              "name": "GPL-3.0",
                              "approval_status": "blacklisted"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        GitLabManagedLicense policy =
            await repository.GetManagedAsync(1, "GPL-3.0", TestContext.Current.CancellationToken);

        Assert.Equal("blacklisted", policy.ApprovalStatus);
    }

    [Fact]
    public async Task GetCurrentAsync_WithoutAdministratorAccess_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.GetCurrentAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_OnAnInvalidKey_ThrowsGitLabValidationException_WithoutEchoingTheKey()
    {
        const string Key = "eyJkYXRhIjoiVGhlLXNlY3JldC1saWNlbmNlLWtleSJ9";
        const string Json = """{ "message": { "license": ["is invalid"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        LicensesClient repository = new(connection);

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(new CreateLicenseRequest { License = Key },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

        // The failure must never carry the licence key - not in the message, not in the raw body.
        Assert.DoesNotContain(Key, exception.ToString(), StringComparison.Ordinal);
    }
}