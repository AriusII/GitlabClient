using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class RunnerControllersRepositoryTests
{
    /// <summary>The summary shape (<c>APIEntitiesCiRunnerController</c>): no <c>connected</c> flag.</summary>
    private const string RunnerControllerJson = """
                                                {
                                                  "id": 7,
                                                  "description": "Controller for managing runner",
                                                  "state": "enabled",
                                                  "created_at": "2025-01-15T09:00:00.000Z",
                                                  "updated_at": "2025-02-03T14:31:00.000Z"
                                                }
                                                """;

    /// <summary>The detail shape (<c>APIEntitiesCiRunnerControllerDetail</c>), which adds <c>connected</c>.</summary>
    private const string RunnerControllerDetailJson = """
                                                      {
                                                        "id": 7,
                                                        "description": "Controller for managing runner",
                                                        "state": "dry_run",
                                                        "created_at": "2025-01-15T09:00:00.000Z",
                                                        "updated_at": "2025-02-03T14:31:00.000Z",
                                                        "connected": true
                                                      }
                                                      """;

    private const string RunnerControllerTokenJson = """
                                                     {
                                                       "id": 3,
                                                       "runner_controller_id": 7,
                                                       "description": "Token for managing runner",
                                                       "last_used_at": "2025-03-01T10:22:31.000Z",
                                                       "created_at": "2025-01-15T09:00:00.000Z",
                                                       "updated_at": "2025-02-03T14:31:00.000Z"
                                                     }
                                                     """;

    [Fact]
    public async Task ListAsync_BuildsRunnerControllersRoute_AndDeserializesTheSummaryShape()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{RunnerControllerJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabRunnerController> controllers = new();
        await foreach (GitLabRunnerController controller in repository.ListAsync(
                           TestContext.Current.CancellationToken))
        {
            controllers.Add(controller);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabRunnerController only = Assert.Single(controllers);
        Assert.Equal(7, only.Id);
        Assert.Equal("Controller for managing runner", only.Description);
        Assert.Equal(GitLabRunnerControllerState.Enabled, only.State);
        Assert.Equal(new DateTimeOffset(2025, 1, 15, 9, 0, 0, TimeSpan.Zero), only.CreatedAt);
        Assert.Equal(new DateTimeOffset(2025, 2, 3, 14, 31, 0, TimeSpan.Zero), only.UpdatedAt);

        // Detail-only on the wire, so absent from a listing.
        Assert.Null(only.Connected);
    }

    [Fact]
    public async Task GetAsync_BuildsTheControllerRoute_AndDeserializesTheDetailShape()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RunnerControllerDetailJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerController controller = await repository.GetAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabRunnerControllerState.DryRun, controller.State);
        Assert.True(controller.Connected);
    }

    /// <summary>The <c>dry_run</c> member proves the enum is projected by its wire name, not by its C# name.</summary>
    [Fact]
    public async Task RegisterAsync_PostsTheStateByItsWireName()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(RunnerControllerJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        RegisterRunnerControllerRequest request = new()
        {
            Description = "Controller for managing runner", State = GitLabRunnerControllerState.DryRun
        };

        GitLabRunnerController controller =
            await repository.RegisterAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"description":"Controller for managing runner","state":"dry_run"}""", sentBody);
        Assert.Equal(7, controller.Id);
    }

    /// <summary>An unset member must be omitted, not sent as null - a null would clear the field server-side.</summary>
    [Fact]
    public async Task UpdateAsync_PutsOnlyTheMembersThatWereSet()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(RunnerControllerJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerController controller = await repository.UpdateAsync(7,
            new UpdateRunnerControllerRequest { State = GitLabRunnerControllerState.Disabled },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"state":"disabled"}""", sentBody);
        Assert.Equal(7, controller.Id);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheControllerRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RunnerControllerJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     A scope listing mixes both scoping entities: the instance-wide one carries no <c>runner_id</c>,
    ///     which is exactly what tells the two apart.
    /// </summary>
    [Fact]
    public async Task ListScopesAsync_ReadsInstanceAndRunnerScopesFromOneListing()
    {
        const string Json = """
                            [
                              {
                                "created_at": "2025-01-15T09:00:00.000Z",
                                "updated_at": "2025-01-15T09:00:00.000Z"
                              },
                              {
                                "runner_id": 8,
                                "created_at": "2025-01-16T09:00:00.000Z",
                                "updated_at": "2025-01-16T09:00:00.000Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabRunnerControllerScope> scopes = new();
        await foreach (GitLabRunnerControllerScope scope in repository.ListScopesAsync(7,
                           TestContext.Current.CancellationToken))
        {
            scopes.Add(scope);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/scopes",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(2, scopes.Count);
        Assert.Null(scopes[0].RunnerId);
        Assert.Equal(8, scopes[1].RunnerId);
        Assert.Equal(new DateTimeOffset(2025, 1, 16, 9, 0, 0, TimeSpan.Zero), scopes[1].CreatedAt);
    }

    [Fact]
    public async Task AddInstanceScopeAsync_PostsToTheInstanceScopeRoute_WithNoBody()
    {
        const string Json = """
                            {
                              "created_at": "2025-01-15T09:00:00.000Z",
                              "updated_at": "2025-01-15T09:00:00.000Z"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerControllerScope scope =
            await repository.AddInstanceScopeAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/scopes/instance",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(scope.RunnerId);
        Assert.Equal(new DateTimeOffset(2025, 1, 15, 9, 0, 0, TimeSpan.Zero), scope.CreatedAt);
    }

    [Fact]
    public async Task RemoveInstanceScopeAsync_SendsDeleteToTheInstanceScopeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.RemoveInstanceScopeAsync(7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/scopes/instance",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task AddRunnerScopeAsync_PostsToTheRunnerScopeRoute()
    {
        const string Json = """
                            {
                              "runner_id": 8,
                              "created_at": "2025-01-16T09:00:00.000Z",
                              "updated_at": "2025-01-16T09:00:00.000Z"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerControllerScope scope =
            await repository.AddRunnerScopeAsync(7, 8, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/scopes/runners/8",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(8, scope.RunnerId);
    }

    /// <summary>
    ///     A controller holding the instance scope cannot take runner scopes; GitLab answers 409, which must
    ///     surface as the conflict type callers catch by name.
    /// </summary>
    [Fact]
    public async Task AddRunnerScopeAsync_WhenTheInstanceScopeIsHeld_ThrowsGitLabConflictException()
    {
        const string Json = """{ "message": "409 Conflict - Instance scope already exists" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabConflictException>(() =>
            repository.AddRunnerScopeAsync(7, 8, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("409 Conflict - Instance scope already exists", exception.Message);
    }

    [Fact]
    public async Task RemoveRunnerScopeAsync_SendsDeleteToTheRunnerScopeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.RemoveRunnerScopeAsync(7, 8, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/scopes/runners/8",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListTokensAsync_BuildsTheTokensRoute_AndDeserializesMetadataOnly()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{RunnerControllerTokenJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabRunnerControllerToken> tokens = new();
        await foreach (GitLabRunnerControllerToken token in repository.ListTokensAsync(7,
                           TestContext.Current.CancellationToken))
        {
            tokens.Add(token);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabRunnerControllerToken only = Assert.Single(tokens);
        Assert.Equal(3, only.Id);
        Assert.Equal(7, only.RunnerControllerId);
        Assert.Equal("Token for managing runner", only.Description);
        Assert.Equal(new DateTimeOffset(2025, 3, 1, 10, 22, 31, TimeSpan.Zero), only.LastUsedAt);
    }

    [Fact]
    public async Task CreateTokenAsync_PostsTheDescription()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(RunnerControllerTokenJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerControllerToken token = await repository.CreateTokenAsync(7,
            new CreateRunnerControllerTokenRequest { Description = "Token for managing runner" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/tokens",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"description":"Token for managing runner"}""", sentBody);
        Assert.Equal(3, token.Id);
    }

    [Fact]
    public async Task GetTokenAsync_BuildsTheSingleTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(RunnerControllerTokenJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerControllerToken token = await repository.GetTokenAsync(7, 3,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/tokens/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3, token.Id);
    }

    [Fact]
    public async Task RevokeTokenAsync_SendsDeleteToTheSingleTokenRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.RevokeTokenAsync(7, 3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/tokens/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>Rotation is the only call that hands back a usable secret, and it does so exactly once.</summary>
    [Fact]
    public async Task RotateTokenAsync_PostsToTheRotateRoute_AndReturnsTheSecret()
    {
        const string Json = """
                            {
                              "id": 3,
                              "runner_controller_id": 7,
                              "description": "Token for managing runner",
                              "last_used_at": null,
                              "created_at": "2025-01-15T09:00:00.000Z",
                              "updated_at": "2025-04-02T08:00:00.000Z",
                              "token": "glrct-8ETVzC1YMx4sJ4qkeQ2t"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabRunnerControllerTokenWithSecret token =
            await repository.RotateTokenAsync(7, 3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/runner_controllers/7/tokens/3/rotate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(3, token.Id);
        Assert.Equal("glrct-8ETVzC1YMx4sJ4qkeQ2t", token.Token);
        Assert.Null(token.LastUsedAt);
    }

    /// <summary>A rotation failure must never echo a secret back through the exception surface.</summary>
    [Fact]
    public async Task RotateTokenAsync_WhenTheTokenIsGone_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Runner Controller Token Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        RunnerControllersRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.RotateTokenAsync(7, 3, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Runner Controller Token Not Found", exception.Message);
    }
}