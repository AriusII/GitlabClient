using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ServiceAccountsRepositoryTests
{
    private const string ServiceAccountJson = """
                                              {
                                                "id": 57,
                                                "username": "service_account_group_345_6018816a18e515214e0c34c2b33523fc",
                                                "name": "Automation bot",
                                                "public_email": "bot@example.com",
                                                "email": "service_account_group_345_6018816a@noreply.gitlab.example",
                                                "unconfirmed_email": "new-bot@example.com"
                                              }
                                              """;

    [Fact]
    public async Task ListAsync_BuildsTheInstanceRoute_AndDeserializesEveryField()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{ServiceAccountJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabServiceAccount> accounts = [];
        await foreach (GitLabServiceAccount account in repository.ListAsync(
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            accounts.Add(account);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/service_accounts", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabServiceAccount single = Assert.Single(accounts);
        Assert.Equal(57, single.Id);
        Assert.Equal("service_account_group_345_6018816a18e515214e0c34c2b33523fc", single.Username);
        Assert.Equal("Automation bot", single.Name);
        Assert.Equal("bot@example.com", single.PublicEmail);
        Assert.Equal("service_account_group_345_6018816a@noreply.gitlab.example", single.Email);
        Assert.Equal("new-bot@example.com", single.UnconfirmedEmail);
    }

    [Fact]
    public async Task ListForGroupAsync_EncodesTheNamespacedGroupPath_AndProjectsTheListOptions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        ServiceAccountListOptions options = new()
        {
            OrderBy = GitLabServiceAccountOrderBy.Username, Sort = GitLabServiceAccountSort.Asc, PerPage = 50
        };

        await foreach (GitLabServiceAccount _ in repository.ListForGroupAsync("parent-group/subgroup", options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/service_accounts"
            + "?order_by=username&sort=asc&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesTheNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{ServiceAccountJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabServiceAccount> accounts = [];
        await foreach (GitLabServiceAccount account in repository.ListForProjectAsync("gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            accounts.Add(account);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/service_accounts",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(57, Assert.Single(accounts).Id);
    }

    [Fact]
    public async Task GetForGroupAsync_BuildsTheUserIdRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ServiceAccountJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabServiceAccount account =
            await repository.GetForGroupAsync(345, 57, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/345/service_accounts/57",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Automation bot", account.Name);
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsTheUserIdRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ServiceAccountJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabServiceAccount account =
            await repository.GetForProjectAsync(7, 57, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/7/service_accounts/57",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(57, account.Id);
    }

    [Fact]
    public async Task CreateAsync_PostsTheRequestedAttributes_ToTheInstanceRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ServiceAccountJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        CreateServiceAccountRequest request = new()
        {
            Name = "Automation bot", Username = "automation-bot", Email = "bot@example.com"
        };

        GitLabServiceAccount account = await repository.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/service_accounts",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal(
            """{"name":"Automation bot","username":"automation-bot","email":"bot@example.com"}""",
            sentBody);
        Assert.Equal(57, account.Id);
    }

    [Fact]
    public async Task CreateForProjectAsync_OmitsTheMembersTheCallerLeftUnset()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ServiceAccountJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.CreateForProjectAsync("gitlab-org/gitlab", new CreateServiceAccountRequest(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/service_accounts",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // GitLab generates a name, a username and a no-reply address for whatever the body omits, so an empty
        // request must serialize to an empty object rather than three explicit nulls.
        Assert.Equal("{}", sentBody);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsToTheGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(ServiceAccountJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.CreateForGroupAsync(345, new CreateServiceAccountRequest { Username = "automation-bot" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/345/service_accounts",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateAsync_SendsPatch_NotPut()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ServiceAccountJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabServiceAccount account = await repository.UpdateAsync(57,
            new UpdateServiceAccountRequest { Email = "new-bot@example.com" },
            TestContext.Current.CancellationToken);

        // GitLab publishes only PATCH here; a PUT is a 404 on this route.
        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/service_accounts/57",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"email":"new-bot@example.com"}""", sentBody);
        Assert.Equal("new-bot@example.com", account.UnconfirmedEmail);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PatchesTheGroupUserRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ServiceAccountJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdateForGroupAsync("parent-group/subgroup", 57,
            new UpdateServiceAccountRequest { Name = "Renamed bot" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/service_accounts/57",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateForProjectAsync_PatchesTheProjectUserRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ServiceAccountJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdateForProjectAsync(7, 57, new UpdateServiceAccountRequest { Username = "renamed-bot" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/service_accounts/57",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForGroupAsync_AddsHardDelete_WhenRequested()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteForGroupAsync("parent-group/subgroup", 57, true,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/service_accounts/57?hard_delete=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForProjectAsync_OmitsHardDelete_WhenTheCallerDoesNotAskForIt()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteForProjectAsync(7, 57, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/7/service_accounts/57",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForGroupAsync_OnAFreeInstance_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.GetForGroupAsync(345, 57, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task CreateForGroupAsync_OnATakenUsername_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "username": ["has already been taken"] } }""";

        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        ServiceAccountsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateForGroupAsync(345, new CreateServiceAccountRequest { Username = "taken" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("has already been taken", exception.Message, StringComparison.Ordinal);
    }
}