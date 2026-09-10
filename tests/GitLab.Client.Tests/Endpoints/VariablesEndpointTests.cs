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

public sealed class VariablesEndpointTests
{
    private const string VariableJson = """
                                        {
                                          "variable_type": "env_var",
                                          "key": "TEST_VARIABLE_1",
                                          "value": "TEST_1",
                                          "hidden": false,
                                          "protected": false,
                                          "masked": true,
                                          "raw": false,
                                          "environment_scope": "production",
                                          "description": "Deployment token"
                                        }
                                        """;

    private const string VariableArrayJson = $"[{VariableJson}]";

    [Fact]
    public async Task ListProjectVariablesAsync_BuildsVariablesRoute_WithPaging_AndDeserializesVariables()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableArrayJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        List<GitLabVariable> variables = new();
        await foreach (GitLabVariable item in repository.ListProjectVariablesAsync(1,
                           new VariableListOptions { Page = 3, PerPage = 50 }, TestContext.Current.CancellationToken))
        {
            variables.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/variables", requestUri, StringComparison.Ordinal);
        Assert.Contains("page=3", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", requestUri, StringComparison.Ordinal);

        GitLabVariable variable = Assert.Single(variables);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
        Assert.Equal("TEST_1", variable.Value);
        Assert.Equal("env_var", variable.VariableType);
        Assert.False(variable.Protected);
        Assert.True(variable.Masked);
        Assert.False(variable.Hidden);
        Assert.False(variable.Raw);
        Assert.Equal("production", variable.EnvironmentScope);
        Assert.Equal("Deployment token", variable.Description);
    }

    [Fact]
    public async Task GetProjectVariableAsync_EscapesKey_AndSendsEnvironmentScopeFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        GitLabVariable variable = await repository.GetProjectVariableAsync(1, "SCOPED/KEY", "review/app",
            TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/variables/SCOPED%2FKEY", requestUri, StringComparison.Ordinal);
        Assert.Contains("filter[environment_scope]=review%2Fapp", requestUri, StringComparison.Ordinal);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
        Assert.Equal("production", variable.EnvironmentScope);
    }

    [Fact]
    public async Task GetProjectVariableAsync_WithoutEnvironmentScope_OmitsTheFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        await repository.GetProjectVariableAsync(1, "TEST_VARIABLE_1",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/variables/TEST_VARIABLE_1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetProjectVariableAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        await repository.GetProjectVariableAsync("gitlab-org/gitlab", "TEST_VARIABLE_1",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Contains("/projects/gitlab-org%2Fgitlab/variables/TEST_VARIABLE_1",
            handler.LastRequest?.RequestUri?.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateProjectVariableAsync_PostsSerializedBody_AndDeserializesCreatedVariable()
    {
        const string Json = """
                            {
                              "variable_type": "file",
                              "key": "DEPLOY_KEY",
                              "value": null,
                              "hidden": true,
                              "protected": true,
                              "masked": true,
                              "raw": true,
                              "environment_scope": "*"
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
        VariablesClient repository = new(connection);

        CreateVariableRequest request = new()
        {
            Key = "DEPLOY_KEY",
            Value = "super-secret",
            MaskedAndHidden = true,
            Protected = true,
            Raw = true,
            VariableType = "file"
        };

        GitLabVariable variable =
            await repository.CreateProjectVariableAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/variables",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"key\":\"DEPLOY_KEY\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"value\":\"super-secret\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"masked_and_hidden\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"variable_type\":\"file\"", sentBody, StringComparison.Ordinal);

        // Unset optional fields must be omitted, never sent as null.
        Assert.DoesNotContain("\"masked\"", sentBody, StringComparison.Ordinal);

        Assert.Equal("DEPLOY_KEY", variable.Key);
        Assert.Null(variable.Value);
        Assert.True(variable.Hidden);
        Assert.Equal("file", variable.VariableType);
    }

    [Fact]
    public async Task UpdateProjectVariableAsync_PutsSerializedBody_ToTheEscapedKeyRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        UpdateVariableRequest request = new()
        {
            Value = "TEST_2", Protected = true, EnvironmentScope = "production", Description = "Deployment token"
        };

        GitLabVariable variable = await repository.UpdateProjectVariableAsync("gitlab-org/gitlab", "SCOPED/KEY",
            request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/variables/SCOPED%2FKEY",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"value\":\"TEST_2\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"protected\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"environment_scope\":\"production\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
    }

    [Fact]
    public async Task UpdateProjectVariableAsync_WithEnvironmentScopeFilter_SelectsTheExistingScopedVariable()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);
        UpdateVariableRequest request = new() { Value = "TEST_2", EnvironmentScope = "production" };

        await repository.UpdateProjectVariableAsync("gitlab-org/gitlab", "SCOPED/KEY", request, "review/app",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/variables/SCOPED%2FKEY?filter[environment_scope]=review%2Fapp",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"environment_scope\":\"production\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("filter", sentBody, StringComparison.Ordinal);
    }

    /// <summary>
    ///     GitLab answers this delete with <c>200 OK</c> and the deleted variable as the body rather than the
    ///     usual <c>204 No Content</c>, so the assertion is on the request, not on the status code.
    /// </summary>
    [Fact]
    public async Task DeleteProjectVariableAsync_SendsDeleteToEscapedKey_WithEnvironmentScopeFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        await repository.DeleteProjectVariableAsync(1, "SCOPED/KEY", "review/app",
            TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/variables/SCOPED%2FKEY", requestUri, StringComparison.Ordinal);
        Assert.Contains("filter[environment_scope]=review%2Fapp", requestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListGroupVariablesAsync_BuildsGroupVariablesRoute_AndEncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableArrayJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        List<GitLabVariable> variables = new();
        await foreach (GitLabVariable item in repository.ListGroupVariablesAsync("gitlab-org/subgroup",
                           new VariableListOptions { PerPage = 20 }, TestContext.Current.CancellationToken))
        {
            variables.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/groups/gitlab-org%2Fsubgroup/variables", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=20", requestUri, StringComparison.Ordinal);
        GitLabVariable variable = Assert.Single(variables);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
        Assert.Equal("TEST_1", variable.Value);
        Assert.False(variable.Hidden);
        Assert.False(variable.Protected);
        Assert.True(variable.Masked);
        Assert.False(variable.Raw);
        Assert.Equal("production", variable.EnvironmentScope);
        Assert.Equal("Deployment token", variable.Description);
    }

    [Fact]
    public async Task GetGroupVariableAsync_WithoutEnvironmentScope_UsesLegacyOverloadAndOmitsTheFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        GitLabVariable variable = await repository.GetGroupVariableAsync(9, "SCOPED/KEY",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/variables/SCOPED%2FKEY",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
        Assert.Equal("TEST_1", variable.Value);
    }

    [Fact]
    public async Task GetGroupVariableAsync_WithEnvironmentScopeFilter_EscapesGroupPathVariableKeyAndFilterValue()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        GitLabVariable variable = await repository.GetGroupVariableAsync("gitlab-org/subgroup", "SCOPED/KEY",
            "review/app", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/variables/SCOPED%2FKEY?filter[environment_scope]=review%2Fapp",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
    }

    [Fact]
    public async Task CreateGroupVariableAsync_PostsToGroupVariablesRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        CreateVariableRequest request = new()
        {
            Key = "TEST_VARIABLE_1",
            Value = "TEST_1",
            Protected = true,
            Masked = true,
            MaskedAndHidden = true,
            Raw = false,
            VariableType = "file",
            EnvironmentScope = "review/app",
            Description = "Deployment secret"
        };

        GitLabVariable variable =
            await repository.CreateGroupVariableAsync(9, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/variables", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"key\":\"TEST_VARIABLE_1\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"value\":\"TEST_1\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"protected\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"masked\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"masked_and_hidden\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"raw\":false", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"variable_type\":\"file\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"environment_scope\":\"review/app\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"description\":\"Deployment secret\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
    }

    [Fact]
    public async Task UpdateGroupVariableAsync_WithEnvironmentScopeFilter_SelectsTheExistingScopedVariable()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        UpdateVariableRequest request = new()
        {
            Value = "TEST_2",
            Protected = true,
            Masked = true,
            Raw = false,
            VariableType = "file",
            EnvironmentScope = "production",
            Description = "Rotated deployment secret"
        };

        GitLabVariable variable = await repository.UpdateGroupVariableAsync(9, "TEST_VARIABLE_1", request,
            "review/app", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9/variables/TEST_VARIABLE_1?filter[environment_scope]=review%2Fapp",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"value\":\"TEST_2\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"protected\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"masked\":true", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"raw\":false", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"variable_type\":\"file\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"environment_scope\":\"production\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"description\":\"Rotated deployment secret\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("filter", sentBody, StringComparison.Ordinal);
        Assert.Equal("env_var", variable.VariableType);
    }

    [Fact]
    public async Task DeleteGroupVariableAsync_SendsDeleteToGroupVariablesRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        await repository.DeleteGroupVariableAsync(9, "TEST_VARIABLE_1",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9/variables/TEST_VARIABLE_1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteGroupVariableAsync_WithEnvironmentScopeFilter_EscapesTheFilterValue()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        await repository.DeleteGroupVariableAsync("gitlab-org/subgroup", "SCOPED/KEY", "review/app",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/variables/SCOPED%2FKEY?filter[environment_scope]=review%2Fapp",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetProjectVariableAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Variable Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetProjectVariableAsync(1, "MISSING",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Variable Not Found", exception.Message);
    }

    /// <summary>Reusing a key inside one environment scope is GitLab's 400, not a 409.</summary>
    [Fact]
    public async Task CreateProjectVariableAsync_OnDuplicateKey_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "key": ["(TEST_VARIABLE_1) has already been taken"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        CreateVariableRequest request = new() { Key = "TEST_VARIABLE_1", Value = "TEST_1" };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateProjectVariableAsync(1, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("has already been taken", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>The instance scope has no id segment: /admin/ci/variables is three fixed path words.</summary>
    [Fact]
    public async Task ListInstanceVariablesAsync_BuildsTheAdminRoute_WithPaging()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableArrayJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        List<GitLabVariable> variables = new();
        await foreach (GitLabVariable item in repository.ListInstanceVariablesAsync(
                           new VariableListOptions { PerPage = 20 }, TestContext.Current.CancellationToken))
        {
            variables.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/ci/variables?per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabVariable variable = Assert.Single(variables);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
        Assert.Equal("env_var", variable.VariableType);
        Assert.True(variable.Masked);
    }

    [Fact]
    public async Task GetInstanceVariableAsync_EscapesTheKey()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        GitLabVariable variable =
            await repository.GetInstanceVariableAsync("TEST/VARIABLE", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/ci/variables/TEST%2FVARIABLE",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
    }

    [Fact]
    public async Task CreateInstanceVariableAsync_PostsToTheAdminRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        CreateInstanceVariableRequest request = new()
        {
            Key = "NEW_VARIABLE", Value = "new value", Masked = true, VariableType = "env_var"
        };

        GitLabVariable variable =
            await repository.CreateInstanceVariableAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/ci/variables",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"key":"NEW_VARIABLE","value":"new value","masked":true,"variable_type":"env_var"}""",
            sentBody);
        Assert.Equal("TEST_VARIABLE_1", variable.Key);
    }

    [Fact]
    public async Task UpdateInstanceVariableAsync_PutsToTheAdminRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        UpdateInstanceVariableRequest request = new()
        {
            Value = "rotated", Description = "Rotated by the instance operator", Protected = true, Raw = false
        };

        await repository.UpdateInstanceVariableAsync("TEST_VARIABLE_1", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/ci/variables/TEST_VARIABLE_1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"value":"rotated","description":"Rotated by the instance operator","protected":true,"raw":false}""",
            sentBody);
    }

    /// <summary>GitLab answers this delete with 200 and the deleted variable - body and secret both discarded.</summary>
    [Fact]
    public async Task DeleteInstanceVariableAsync_SendsDeleteToTheAdminRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(VariableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        await repository.DeleteInstanceVariableAsync("TEST_VARIABLE_1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/ci/variables/TEST_VARIABLE_1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>Every instance-variable endpoint is administrator-only; a plain token gets a 403.</summary>
    [Fact]
    public async Task GetInstanceVariableAsync_WithoutAdminRights_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        VariablesClient repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.GetInstanceVariableAsync("TEST_VARIABLE_1", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}