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

public sealed class NamespacesEndpointTests
{
    private const string NamespaceJson = """
                                         {
                                           "id": 2,
                                           "name": "group1",
                                           "path": "group1",
                                           "kind": "group",
                                           "full_path": "group1",
                                           "parent_id": null,
                                           "avatar_url": null,
                                           "web_url": "https://gitlab.example.com/groups/group1",
                                           "members_count_with_descendants": 5,
                                           "root_repository_size": 123,
                                           "projects_count": 3,
                                           "shared_runners_minutes_limit": 133,
                                           "extra_shared_runners_minutes_limit": 133,
                                           "additional_purchased_storage_size": 1000,
                                           "additional_purchased_storage_ends_on": "2022-06-18",
                                           "billable_members_count": 2,
                                           "seats_in_use": 5,
                                           "max_seats_used": 100,
                                           "max_seats_used_changed_at": "2022-06-18",
                                           "end_date": "2022-06-18",
                                           "plan": "ultimate",
                                           "trial_ends_on": "2022-06-18",
                                           "trial": false
                                         }
                                         """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task ListAsync_BuildsTheQueryString_AndDeserializesTheFullEntity()
    {
        string json = $"[{NamespaceJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabNamespace> namespaces = [];
        await foreach (GitLabNamespace item in repository.ListAsync(
                           new NamespaceListOptions { Search = "group", OwnedOnly = true, TopLevelOnly = true },
                           TestContext.Current.CancellationToken))
        {
            namespaces.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/namespaces?search=group&owned_only=true&top_level_only=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabNamespace ns = Assert.Single(namespaces);
        Assert.Equal(2, ns.Id);
        Assert.Equal("group1", ns.Name);
        Assert.Equal("group", ns.Kind);
        Assert.Equal(1000, ns.AdditionalPurchasedStorageSize);
        Assert.Equal(new DateOnly(2022, 6, 18), ns.AdditionalPurchasedStorageEndsOn);
        Assert.Equal("ultimate", ns.Plan);
        Assert.False(ns.Trial);
        Assert.Null(ns.ParentId);
        Assert.Null(ns.AvatarUrl);
    }

    [Fact]
    public async Task GetAsync_EncodesANamespacedFullPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NamespaceJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabNamespace ns = await repository.GetAsync("parent-group/subgroup", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/namespaces/parent-group%2Fsubgroup",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, ns.Id);
    }

    [Fact]
    public async Task UpdateAsync_PutsTheComputeMinutesAndStorageFields_AndDeserializesTheUpdatedNamespace()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(NamespaceJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabNamespace ns = await repository.UpdateAsync(
            2,
            new UpdateNamespaceRequest
            {
                SharedRunnersMinutesLimit = 133,
                ExtraSharedRunnersMinutesLimit = 133,
                AdditionalPurchasedStorageSize = 1000,
                AdditionalPurchasedStorageEndsOn = new DateOnly(2022, 6, 18)
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/namespaces/2", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"shared_runners_minutes_limit":133,"extra_shared_runners_minutes_limit":133,"additional_purchased_storage_size":1000,"additional_purchased_storage_ends_on":"2022-06-18"}""",
            sentBody);
        Assert.Equal(2, ns.Id);
        Assert.Equal(1000, ns.AdditionalPurchasedStorageSize);
    }

    [Fact]
    public async Task ExistsAsync_EscapesThePathCandidate_AndSendsTheParentIdFilter()
    {
        const string Json = """{ "exists": true, "suggests": ["my-group1"] }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabNamespaceExistence existence =
            await repository.ExistsAsync("my group1", 42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/namespaces/my%20group1/exists?parent_id=42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(existence.Exists);
        Assert.Equal("my-group1", Assert.Single(existence.Suggests!));
    }

    [Fact]
    public async Task GetSubscriptionAsync_DeserializesThePlanUsageAndBillingObjects()
    {
        const string Json = """
                            {
                              "plan": { "code": "ultimate", "name": "Ultimate", "trial": false, "auto_renew": true, "upgradable": false, "exclude_guests": false },
                              "usage": { "seats_in_subscription": 10, "seats_in_use": 4, "max_seats_used": 6, "seats_owed": 0 },
                              "billing": { "subscription_start_date": "2024-01-01", "subscription_end_date": "2025-01-01", "trial_ends_on": null }
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabNamespaceSubscription subscription =
            await repository.GetSubscriptionAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/namespaces/9970/gitlab_subscription",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal("ultimate", subscription.Plan?.Code);
        Assert.True(subscription.Plan?.AutoRenew);
        Assert.Equal(10, subscription.Usage?.SeatsInSubscription);
        Assert.Equal(4, subscription.Usage?.SeatsInUse);
        Assert.Equal(new DateOnly(2024, 1, 1), subscription.Billing?.SubscriptionStartDate);
        Assert.Null(subscription.Billing?.TrialEndsOn);
    }

    [Fact]
    public async Task ListStorageLimitExclusionsAsync_BuildsTheGlobalRoute()
    {
        const string Json = """[{ "id": 1, "namespace_id": 123, "namespace_name": "GitLab", "reason": "a reason" }]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabNamespaceStorageLimitExclusion> exclusions = [];
        await foreach (GitLabNamespaceStorageLimitExclusion item in repository.ListStorageLimitExclusionsAsync(
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            exclusions.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/namespaces/storage/limit_exclusions",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabNamespaceStorageLimitExclusion exclusion = Assert.Single(exclusions);
        Assert.Equal(1, exclusion.Id);
        Assert.Equal(123, exclusion.NamespaceId);
        Assert.Equal("a reason", exclusion.Reason);
    }

    [Fact]
    public async Task CreateStorageLimitExclusionAsync_PostsTheReason_ToTheScopedNamespaceRoute()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    """{ "id": 1, "namespace_id": 9970, "namespace_name": "GitLab", "reason": "known exception" }""",
                    Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabNamespaceStorageLimitExclusion exclusion = await repository.CreateStorageLimitExclusionAsync(9970,
            new CreateNamespaceStorageLimitExclusionRequest { Reason = "known exception" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/namespaces/9970/storage/limit_exclusion",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"reason":"known exception"}""", sentBody);
        Assert.Equal("known exception", exclusion.Reason);
    }

    [Fact]
    public async Task DeleteStorageLimitExclusionAsync_SendsDeleteToTheScopedNamespaceRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteStorageLimitExclusionAsync("parent-group/subgroup",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/namespaces/parent-group%2Fsubgroup/storage/limit_exclusion",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListProjectsAsync_BuildsTheInternalRoute_AndDeserializesTheEmbeddedLicense()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "path_with_namespace": "namespace1/project1",
                                "web_url": "https://gitlab.example.com/namespace1/project1",
                                "visibility": "public",
                                "empty_repo": false,
                                "wiki_enabled": true,
                                "license": {
                                  "key": "gpl-3.0",
                                  "name": "GNU General Public License v3.0",
                                  "nickname": "GNU GPLv3",
                                  "html_url": "http://choosealicense.com/licenses/gpl-3.0",
                                  "source_url": null
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabNamespaceProject> projects = [];
        await foreach (GitLabNamespaceProject item in repository.ListProjectsAsync(9970,
                           new NamespaceProjectListOptions { PerPage = 50 }, TestContext.Current.CancellationToken))
        {
            projects.Add(item);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/internal/gitlab_subscriptions/namespaces/9970/projects?per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabNamespaceProject project = Assert.Single(projects);
        Assert.Equal("namespace1/project1", project.PathWithNamespace);
        Assert.Equal("public", project.Visibility);
        Assert.False(project.EmptyRepo);
        Assert.Equal("gpl-3.0", project.License?.Key);
        Assert.Null(project.License?.SourceUrl);
    }

    [Fact]
    public async Task GetAsync_OnMissingNamespace_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Namespace Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        NamespacesClient repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(999_999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}