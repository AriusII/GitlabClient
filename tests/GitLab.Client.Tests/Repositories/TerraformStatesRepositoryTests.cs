using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class TerraformStatesRepositoryTests
{
    /// <summary>
    ///     A state name that exercises both characters that must survive the round trip: a slash, which
    ///     would otherwise invent a path segment, and a dot.
    /// </summary>
    private const string StateName = "env/production.tfstate";

    private const string StateDocument = """
                                         {
                                           "version": 4,
                                           "terraform_version": "1.9.5",
                                           "serial": 12,
                                           "lineage": "cb2f2b3d-4c9c-4c0e-9e3f-2c0c04d0d9a0",
                                           "outputs": {},
                                           "resources": []
                                         }
                                         """;

    private const string ProtectionRuleJson = """
                                              {
                                                "id": 3,
                                                "project_id": 7,
                                                "state_name": "production",
                                                "minimum_access_level_for_write": "maintainer",
                                                "allowed_from": "ci_on_protected_branch_only"
                                              }
                                              """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task DownloadAsync_EscapesTheStateName_AndStreamsTheOpaqueDocument()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(StateDocument, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        using GitLabFileResponse state =
            await repository.DownloadAsync(7, StateName, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        // The state name is caller-supplied free text, so the slash must be percent-encoded rather than
        // becoming a path segment - .Escaped, never .Literal.
        Assert.Equal("https://gitlab.example/api/v4/projects/7/terraform/state/env%2Fproduction.tfstate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using StreamReader reader = new(state.Content);
        string body = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"terraform_version\": \"1.9.5\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DownloadAsync_SendsTheLockIdAsTerraformsUppercaseIdParameter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(StateDocument, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        using GitLabFileResponse _ = await repository.DownloadAsync(
            ProjectId.FromPath("group/subgroup/infra"),
            "production",
            "d2f4e0c9-4d4c-4f6a-9c9a-1b2c3d4e5f60",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/group%2Fsubgroup%2Finfra/terraform/state/production"
            + "?ID=d2f4e0c9-4d4c-4f6a-9c9a-1b2c3d4e5f60",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_DeletesTheWholeState()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        await repository.DeleteAsync(7, StateName, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/terraform/state/env%2Fproduction.tfstate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task AuthorizeUploadAsync_PostsToTheAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"TempPath":"/var/opt/gitlab/terraform/tmp"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        await repository.AuthorizeUploadAsync(7, StateName, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/terraform/state/env%2Fproduction.tfstate/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task LockAsync_SendsTerraformsPascalCaseLockInfoDocument()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        LockTerraformStateRequest request = new()
        {
            Id = "d2f4e0c9",
            Operation = "OperationTypePlan",
            Info = "",
            Who = "deployer@runner-1",
            Version = "1.9.5",
            Created = "2024-06-20 15:53:00.123456 UTC",
            Path = "env/production.tfstate"
        };

        await repository.LockAsync(7, StateName, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/terraform/state/env%2Fproduction.tfstate/lock",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // Terraform's LockInfo is Go-cased, and "ID" is not "id". The library's snake_case policy would
        // rename all seven members, so every one carries an explicit [JsonPropertyName].
        Assert.Equal(
            """
            {"ID":"d2f4e0c9","Operation":"OperationTypePlan","Info":"","Who":"deployer@runner-1","Version":"1.9.5","Created":"2024-06-20 15:53:00.123456 UTC","Path":"env/production.tfstate"}
            """,
            sentBody);
    }

    [Fact]
    public async Task UnlockAsync_DeletesTheLock_WithTheLockIdInTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        await repository.UnlockAsync(7, StateName, "d2f4e0c9", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/terraform/state/env%2Fproduction.tfstate/lock?ID=d2f4e0c9",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UnlockAsync_WithoutALockId_OmitsTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        // The force-unlock shape: no lock id, so no ID parameter at all rather than "ID=".
        await repository.UnlockAsync(7, "production", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/terraform/state/production/lock",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task LockAsync_MapsA409ToTheTypedConflictException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent("""{"message":"409 Conflict"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        LockTerraformStateRequest request = new()
        {
            Id = "d2f4e0c9",
            Operation = "OperationTypeApply",
            Info = "",
            Who = "deployer@runner-1",
            Version = "1.9.5",
            Created = "2024-06-20 15:53:00.123456 UTC",
            Path = "production"
        };

        GitLabConflictException exception = await Assert.ThrowsAsync<GitLabConflictException>(() =>
            repository.LockAsync(7, "production", request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
    }

    [Fact]
    public async Task DownloadVersionAsync_AddressesOneSerial()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(StateDocument, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        using GitLabFileResponse version =
            await repository.DownloadVersionAsync(7, StateName, 12, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/terraform/state/env%2Fproduction.tfstate/versions/12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.OK, version.StatusCode);
    }

    [Fact]
    public async Task DeleteVersionAsync_DeletesOneSerial()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        await repository.DeleteVersionAsync(7, StateName, 12, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/terraform/state/env%2Fproduction.tfstate/versions/12",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListProtectionRulesAsync_BuildsTheRulesRoute_AndDeserializesTheEntry()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{ProtectionRuleJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        List<GitLabTerraformStateProtectionRule> rules = [];
        await foreach (GitLabTerraformStateProtectionRule rule in
                       repository.ListProtectionRulesAsync(7, TestContext.Current.CancellationToken))
        {
            rules.Add(rule);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/7/terraform/state_protection_rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabTerraformStateProtectionRule only = Assert.Single(rules);
        Assert.Equal(3, only.Id);
        Assert.Equal(7, only.ProjectId);
        Assert.Equal("production", only.StateName);
        Assert.Equal("maintainer", only.MinimumAccessLevelForWrite);
        Assert.Equal("ci_on_protected_branch_only", only.AllowedFrom);
    }

    [Fact]
    public async Task CreateProtectionRuleAsync_SendsTheEnumsAsTheirWireValues()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ProtectionRuleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        GitLabTerraformStateProtectionRule rule = await repository.CreateProtectionRuleAsync(
            7,
            new CreateTerraformStateProtectionRuleRequest
            {
                StateName = "production",
                MinimumAccessLevelForWrite = GitLabTerraformStateWriteAccessLevel.Maintainer,
                AllowedFrom = GitLabTerraformStateAllowedFrom.CiOnProtectedBranchOnly
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/terraform/state_protection_rules",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(
            """
            {"state_name":"production","minimum_access_level_for_write":"maintainer","allowed_from":"ci_on_protected_branch_only"}
            """,
            sentBody);

        Assert.Equal(3, rule.Id);
    }

    [Fact]
    public async Task UpdateProtectionRuleAsync_PatchesOneRule_AndOmitsUnsetMembers()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ProtectionRuleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        await repository.UpdateProtectionRuleAsync(
            7,
            3,
            new UpdateTerraformStateProtectionRuleRequest { AllowedFrom = GitLabTerraformStateAllowedFrom.CiOnly },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/terraform/state_protection_rules/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"allowed_from":"ci_only"}""", sentBody);
    }

    [Fact]
    public async Task DeleteProtectionRuleAsync_DeletesOneRule()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        await repository.DeleteProtectionRuleAsync(7, 3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/terraform/state_protection_rules/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     Terraform's HTTP backend writes a state with a plain POST body, but GitLab takes it as a
    ///     <c>multipart/form-data</c> upload and answers with no content at all - so this must not go
    ///     through a deserializing overload.
    /// </summary>
    [Fact]
    public async Task UploadAsync_PostsTheStateAsMultipart_AndEscapesTheStateName()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);
        TerraformStatesRepository repository = new(connection);

        using MemoryStream content = new(Encoding.UTF8.GetBytes(StateDocument));
        GitLabFileUpload state = new()
        {
            Content = content, FileName = "production.tfstate", ContentType = "application/json"
        };

        await repository.UploadAsync(7, StateName, state, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/terraform/state/env%2Fproduction.tfstate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("production.tfstate", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"terraform_version\": \"1.9.5\"", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);
    }
}