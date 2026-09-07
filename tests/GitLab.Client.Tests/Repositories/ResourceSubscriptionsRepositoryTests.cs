using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ResourceSubscriptionsRepositoryTests
{
    private const string SubscribedIssueJson = """
                                               {
                                                 "id": 92,
                                                 "iid": 11,
                                                 "project_id": 5,
                                                 "title": "Ut commodi ullam eos dolores perferendis nihil sunt.",
                                                 "description": "Omnis vero earum sunt corporis.",
                                                 "state": "opened",
                                                 "created_at": "2016-01-04T15:31:51.081Z",
                                                 "updated_at": "2016-01-05T15:31:51.081Z",
                                                 "labels": ["bug", "documentation"],
                                                 "web_url": "https://gitlab.example/my-group/my-project/-/issues/11",
                                                 "subscribed": true
                                               }
                                               """;

    private const string SubscribedMergeRequestJson = """
                                                      {
                                                        "id": 17,
                                                        "iid": 1,
                                                        "project_id": 5,
                                                        "title": "Ipsam eveniet rem.",
                                                        "state": "opened",
                                                        "source_branch": "feature/widget",
                                                        "target_branch": "main",
                                                        "draft": false,
                                                        "merge_status": "can_be_merged",
                                                        "web_url": "https://gitlab.example/my-group/my-project/-/merge_requests/1",
                                                        "subscribed": true
                                                      }
                                                      """;

    private const string SubscribedProjectLabelJson = """
                                                      {
                                                        "id": 1,
                                                        "name": "team::backend",
                                                        "description": "Owned by the backend team",
                                                        "color": "#428BCA",
                                                        "text_color": "#FFFFFF",
                                                        "open_issues_count": 1,
                                                        "closed_issues_count": 0,
                                                        "open_merge_requests_count": 1,
                                                        "priority": 10,
                                                        "subscribed": true
                                                      }
                                                      """;

    private const string UnsubscribedGroupLabelJson = """
                                                      {
                                                        "id": 7,
                                                        "name": "group::planning",
                                                        "description": "Group-wide planning label",
                                                        "color": "#8E44AD",
                                                        "text_color": "#FFFFFF",
                                                        "open_issues_count": 3,
                                                        "closed_issues_count": 2,
                                                        "open_merge_requests_count": 0,
                                                        "subscribed": false
                                                      }
                                                      """;

    [Fact]
    public async Task SubscribeToIssueAsync_PostsEmptyBodyToSubscribeRoute_AndDeserializesIssue()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(SubscribedIssueJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        GitLabIssue issue = await repository.SubscribeToIssueAsync(5, 11, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/issues/11/subscribe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);

        Assert.Equal(92, issue.Id);
        Assert.Equal(11, issue.Iid);
        Assert.Equal("opened", issue.State);
        Assert.True(issue.Subscribed);
        Assert.Equal(new Uri("https://gitlab.example/my-group/my-project/-/issues/11"), issue.WebUrl);
    }

    [Fact]
    public async Task UnsubscribeFromIssueAsync_PostsToUnsubscribeRoute_AndEncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(SubscribedIssueJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        GitLabIssue issue = await repository.UnsubscribeFromIssueAsync("gitlab-org/gitlab", 11,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/11/unsubscribe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(92, issue.Id);
    }

    [Fact]
    public async Task SubscribeToMergeRequestAsync_PostsToSubscribeRoute_AndDeserializesMergeRequest()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(SubscribedMergeRequestJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        GitLabMergeRequest mergeRequest =
            await repository.SubscribeToMergeRequestAsync(5, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/merge_requests/1/subscribe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);

        Assert.Equal(17, mergeRequest.Id);
        Assert.Equal(1, mergeRequest.Iid);
        Assert.Equal("feature/widget", mergeRequest.SourceBranch);
        Assert.Equal("main", mergeRequest.TargetBranch);
        Assert.True(mergeRequest.Subscribed);
    }

    [Fact]
    public async Task UnsubscribeFromMergeRequestAsync_PostsToUnsubscribeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(SubscribedMergeRequestJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        GitLabMergeRequest mergeRequest = await repository.UnsubscribeFromMergeRequestAsync("gitlab-org/gitlab", 1,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/1/unsubscribe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("can_be_merged", mergeRequest.MergeStatus);
    }

    [Fact]
    public async Task SubscribeToProjectLabelAsync_EscapesScopedLabelName_AndDeserializesLabel()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(SubscribedProjectLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        GitLabLabel label = await repository.SubscribeToProjectLabelAsync(5, "team::backend",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/5/labels/team%3A%3Abackend/subscribe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(1, label.Id);
        Assert.Equal("team::backend", label.Name);
        Assert.Equal("#428BCA", label.Color);
        Assert.Equal(10, label.Priority);
        Assert.True(label.Subscribed);
    }

    [Fact]
    public async Task UnsubscribeFromProjectLabelAsync_EscapesLabelNameContainingSlashAndSpace()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(SubscribedProjectLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        await repository.UnsubscribeFromProjectLabelAsync("gitlab-org/gitlab", "needs review/triage",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/labels/needs%20review%2Ftriage/unsubscribe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SubscribeToGroupLabelAsync_BuildsGroupRoute_AndEncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(UnsubscribedGroupLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        GitLabLabel label = await repository.SubscribeToGroupLabelAsync("gitlab-org/subgroup", "group::planning",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/labels/group%3A%3Aplanning/subscribe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(7, label.Id);
        Assert.Equal("group::planning", label.Name);
        Assert.False(label.Subscribed);

        // APIEntitiesGroupLabel carries no "priority", so the shared GitLabLabel simply reports null there.
        Assert.Null(label.Priority);
    }

    [Fact]
    public async Task UnsubscribeFromGroupLabelAsync_BuildsGroupUnsubscribeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(UnsubscribedGroupLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        GitLabLabel label = await repository.UnsubscribeFromGroupLabelAsync(42, "group::planning",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/42/labels/group%3A%3Aplanning/unsubscribe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.False(label.Subscribed);
    }

    /// <summary>
    ///     GitLab answers an already-subscribed request with 304, which is not a success status, so the
    ///     idempotent path surfaces as a thrown <see cref="GitLabApiException" /> rather than a no-op. 304 has
    ///     no dedicated derived type, so the base type is what callers catch.
    /// </summary>
    [Fact]
    public async Task SubscribeToIssueAsync_WhenAlreadySubscribed_ThrowsWithNotModified()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotModified));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabApiException>(() =>
            repository.SubscribeToIssueAsync(5, 11, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotModified, exception.StatusCode);
    }

    [Fact]
    public async Task SubscribeToProjectLabelAsync_OnMissingLabel_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Label Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ResourceSubscriptionsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.SubscribeToProjectLabelAsync(5, "does-not-exist", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Label Not Found", exception.Message);
    }
}