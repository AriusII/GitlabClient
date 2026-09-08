using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class CiLintRepositoryTests
{
    private const string ValidResultJson = """
                                           {
                                             "valid": true,
                                             "errors": [],
                                             "warnings": ["jobs:build may allow multiple pipelines to run"],
                                             "merged_yaml": "---\nstages:\n- build\n",
                                             "includes": [
                                               {
                                                 "type": "local",
                                                 "location": ".gitlab/ci/build-images.gitlab-ci.yml",
                                                 "blob": "https://gitlab.example/gitlab-org/gitlab/-/blob/e52d6d0/.gitlab/ci/build-images.gitlab-ci.yml",
                                                 "raw": "https://gitlab.example/gitlab-org/gitlab/-/raw/e52d6d0/.gitlab/ci/build-images.gitlab-ci.yml",
                                                 "extra": { "job_name": "test" },
                                                 "context_project": "gitlab-org/gitlab",
                                                 "context_sha": "e52d6d0246d7375291850e61f0abc101fbda9dc2"
                                               }
                                             ]
                                           }
                                           """;

    [Fact]
    public async Task ValidateProjectConfigurationAsync_BuildsCiLintRoute_WithEveryOption_AndDeserializesTheResult()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ValidResultJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CiLintRepository repository = new(connection);

        CiLintOptions options = new()
        {
            ContentRef = "refs/heads/main", DryRun = true, DryRunRef = "release/1.0", IncludeJobs = false
        };

        GitLabCiLintResult result =
            await repository.ValidateProjectConfigurationAsync(1, options, TestContext.Current.CancellationToken);

        // Asserted whole: it pins the "ci" and "lint" segments, every wire name, the encoding of the two refs,
        // and - by exhausting the query string - that the deprecated "sha"/"ref" aliases are never sent.
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/ci/lint"
            + "?content_ref=refs%2Fheads%2Fmain&dry_run=true&dry_run_ref=release%2F1.0&include_jobs=false",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.True(result.Valid);
        Assert.Empty(result.Errors!);
        Assert.Equal("jobs:build may allow multiple pipelines to run", Assert.Single(result.Warnings!));
        Assert.Equal("---\nstages:\n- build\n", result.MergedYaml);

        GitLabCiLintInclude include = Assert.Single(result.Includes!);
        Assert.Equal("local", include.Type);
        Assert.Equal(".gitlab/ci/build-images.gitlab-ci.yml", include.Location);
        Assert.Equal(
            new Uri("https://gitlab.example/gitlab-org/gitlab/-/blob/e52d6d0/.gitlab/ci/build-images.gitlab-ci.yml"),
            include.Blob);
        Assert.Equal(
            new Uri("https://gitlab.example/gitlab-org/gitlab/-/raw/e52d6d0/.gitlab/ci/build-images.gitlab-ci.yml"),
            include.Raw);
        Assert.Equal("gitlab-org/gitlab", include.ContextProject);
        Assert.Equal("e52d6d0246d7375291850e61f0abc101fbda9dc2", include.ContextSha);
    }

    [Fact]
    public async Task ValidateProjectConfigurationAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ValidResultJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CiLintRepository repository = new(connection);

        await repository.ValidateProjectConfigurationAsync(1,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/ci/lint",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ValidateProjectConfigurationAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ValidResultJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CiLintRepository repository = new(connection);

        await repository.ValidateProjectConfigurationAsync("gitlab-org/gitlab",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/ci/lint",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     An invalid configuration is a <c>200 OK</c> carrying <c>valid: false</c>, not a 4xx - so this must
    ///     deserialize rather than throw.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_PostsTheYamlContent_AndReportsAnInvalidConfigurationAsASuccessfulResponse()
    {
        const string Json = """
                            {
                              "valid": false,
                              "errors": ["variables config should be a hash of key value pairs"],
                              "warnings": []
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

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CiLintRepository repository = new(connection);

        ValidateCiConfigurationRequest request = new()
        {
            Content = "stages:\n  - build\nvariables: invalid", DryRun = true, IncludeJobs = true, Ref = "main"
        };

        GitLabCiLintResult result = await repository.ValidateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/ci/lint",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        // The whole document travels as one JSON string; newlines are escaped by the serializer, not the caller.
        Assert.Contains("\"content\":\"stages:\\n  - build\\nvariables: invalid\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"dry_run\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"include_jobs\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"ref\":\"main\"", sentBody, StringComparison.Ordinal);

        Assert.False(result.Valid);
        Assert.Equal("variables config should be a hash of key value pairs", Assert.Single(result.Errors!));
        Assert.Empty(result.Warnings!);
        Assert.Null(result.MergedYaml);
        Assert.Null(result.Includes);
    }

    [Fact]
    public async Task ValidateAsync_WithOnlyContent_OmitsTheOptionalFields()
    {
        const string Json = """{ "valid": true }""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CiLintRepository repository = new(connection);

        GitLabCiLintResult result = await repository.ValidateAsync("gitlab-org/gitlab",
            new ValidateCiConfigurationRequest { Content = "stages: [build]" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/ci/lint",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{\"content\":\"stages: [build]\"}", sentBody);
        Assert.True(result.Valid);
        Assert.Null(result.Errors);
    }

    /// <summary>
    ///     GitLab's spec leaves each job's shape unspecified (a bare <c>object</c>), so
    ///     <see cref="GitLabCiLintResult.Jobs" /> must round-trip whatever fields are present rather than
    ///     drop the array entirely.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithIncludeJobs_DeserializesTheJobsArray()
    {
        const string Json = """
                            {
                              "valid": true,
                              "errors": [],
                              "warnings": [],
                              "jobs": [
                                { "name": "test", "stage": "test", "before_script": [], "script": ["echo"], "tags": [], "when": "on_success", "allow_failure": false, "only": ["branches"], "except": null }
                              ]
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CiLintRepository repository = new(connection);

        GitLabCiLintResult result = await repository.ValidateAsync(1,
            new ValidateCiConfigurationRequest { Content = "stages: [test]", IncludeJobs = true },
            TestContext.Current.CancellationToken);

        JsonElement job = Assert.Single(result.Jobs!);
        Assert.Equal("test", job.GetProperty("name").GetString());
        Assert.Equal("on_success", job.GetProperty("when").GetString());
    }

    [Fact]
    public async Task ValidateProjectConfigurationAsync_OnMissingProject_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Project Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CiLintRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.ValidateProjectConfigurationAsync(404,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Project Not Found", exception.Message);
    }

    [Fact]
    public async Task ValidateAsync_OnForbidden_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CiLintRepository repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.ValidateAsync(1, new ValidateCiConfigurationRequest { Content = "stages: [build]" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }
}