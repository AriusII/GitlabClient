using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ResourceEventsEndpointTests
{
    private const string UserJson = """
                                    {
                                      "id": 1,
                                      "username": "root",
                                      "name": "Administrator",
                                      "state": "active",
                                      "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/1/avatar.png",
                                      "web_url": "https://gitlab.example/root"
                                    }
                                    """;

    [Fact]
    public async Task ListIssueLabelEventsAsync_BuildsRoute_AndDeserializesLabelEvents()
    {
        string json = $$"""
                        [
                          {
                            "id": 142,
                            "user": {{UserJson}},
                            "created_at": "2018-08-20T13:38:20.077Z",
                            "resource_type": "Issue",
                            "resource_id": 253,
                            "label": {
                              "id": 73,
                              "name": "a1",
                              "color": "#34495E",
                              "description": "",
                              "text_color": "#FFFFFF"
                            },
                            "action": "add"
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceLabelEvent> events = [];
        await foreach (GitLabResourceLabelEvent item in repository.ListIssueLabelEventsAsync(11, 42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/11/issues/42/resource_label_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceLabelEvent labelEvent = Assert.Single(events);
        Assert.Equal(142, labelEvent.Id);
        Assert.Equal("Issue", labelEvent.ResourceType);
        Assert.Equal(253, labelEvent.ResourceId);
        Assert.Equal("add", labelEvent.Action);
        Assert.Equal(new DateTimeOffset(2018, 8, 20, 13, 38, 20, 77, TimeSpan.Zero), labelEvent.CreatedAt);

        // GitLabUser and GitLabLabel are reused verbatim, not redeclared.
        Assert.NotNull(labelEvent.User);
        Assert.Equal(1, labelEvent.User!.Id);
        Assert.Equal("root", labelEvent.User.Username);
        Assert.Equal(new Uri("https://gitlab.example/root"), labelEvent.User.WebUrl);
        Assert.NotNull(labelEvent.Label);
        Assert.Equal(73, labelEvent.Label!.Id);
        Assert.Equal("a1", labelEvent.Label.Name);
        Assert.Equal("#34495E", labelEvent.Label.Color);
    }

    [Fact]
    public async Task ListIssueLabelEventsAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = JsonHandler("[]");
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        await foreach (GitLabResourceLabelEvent _ in repository.ListIssueLabelEventsAsync("gitlab-org/gitlab", 42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/42/resource_label_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListIssueLabelEventsAsync_WithOptions_AppendsPerPageQuery()
    {
        using StubHttpMessageHandler handler = JsonHandler("[]");
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        await foreach (GitLabResourceLabelEvent _ in repository.ListIssueLabelEventsAsync(11, 42,
                           new ResourceEventListOptions { PerPage = 100 },
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/11/issues/42/resource_label_events?per_page=100",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetIssueLabelEventAsync_BuildsSingleEventRoute_AndDeserializes()
    {
        string json = $$"""
                        {
                          "id": 142,
                          "user": {{UserJson}},
                          "created_at": "2018-08-20T13:38:20.077Z",
                          "resource_type": "Issue",
                          "resource_id": 253,
                          "label": {
                            "id": 73,
                            "name": "a1",
                            "color": "#34495E",
                            "description": "",
                            "text_color": "#FFFFFF"
                          },
                          "action": "remove"
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceLabelEvent labelEvent = await repository.GetIssueLabelEventAsync(11, 42, 142,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/11/issues/42/resource_label_events/142",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(142, labelEvent.Id);
        Assert.Equal("remove", labelEvent.Action);
        Assert.Equal("a1", labelEvent.Label?.Name);
    }

    [Fact]
    public async Task ListIssueStateEventsAsync_BuildsRoute_AndDeserializesStateEvents()
    {
        string json = $$"""
                        [
                          {
                            "id": 142,
                            "user": {{UserJson}},
                            "created_at": "2018-08-20T13:38:20.077Z",
                            "resource_type": "Issue",
                            "resource_id": 11,
                            "source_commit": "d20c5aaa",
                            "source_merge_request_id": 88,
                            "state": "closed"
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceStateEvent> events = [];
        await foreach (GitLabResourceStateEvent item in repository.ListIssueStateEventsAsync(11, 42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/11/issues/42/resource_state_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceStateEvent stateEvent = Assert.Single(events);
        Assert.Equal(142, stateEvent.Id);
        Assert.Equal("closed", stateEvent.State);
        Assert.Equal("d20c5aaa", stateEvent.SourceCommit);
        Assert.Equal(88, stateEvent.SourceMergeRequestId);
        Assert.Equal("Administrator", stateEvent.User?.Name);
    }

    [Fact]
    public async Task GetIssueStateEventAsync_BuildsSingleEventRoute_AndDeserializesReopened()
    {
        string json = $$"""
                        {
                          "id": 143,
                          "user": {{UserJson}},
                          "created_at": "2018-08-21T13:38:20.077Z",
                          "resource_type": "Issue",
                          "resource_id": 11,
                          "state": "reopened"
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceStateEvent stateEvent = await repository.GetIssueStateEventAsync("gitlab-org/gitlab", 42, 143,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/42/resource_state_events/143",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("reopened", stateEvent.State);
    }

    [Fact]
    public async Task ListIssueMilestoneEventsAsync_BuildsRoute_AndDeserializesBothActionAndState()
    {
        string json = $$"""
                        [
                          {
                            "id": 142,
                            "user": {{UserJson}},
                            "created_at": "2018-08-20T13:38:20.077Z",
                            "resource_type": "Issue",
                            "resource_id": 11,
                            "milestone": {
                              "id": 61,
                              "iid": 9,
                              "project_id": 7,
                              "title": "v1.2",
                              "description": "Ipsum Lorem",
                              "state": "active",
                              "created_at": "2020-01-27T05:07:12.573Z",
                              "updated_at": "2020-01-27T05:07:12.573Z",
                              "due_date": "2020-02-15",
                              "start_date": "2020-01-27",
                              "web_url": "https://gitlab.example/group/project/-/milestones/9"
                            },
                            "action": "add",
                            "state": "opened"
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceMilestoneEvent> events = [];
        await foreach (GitLabResourceMilestoneEvent item in repository.ListIssueMilestoneEventsAsync(11, 42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/11/issues/42/resource_milestone_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceMilestoneEvent milestoneEvent = Assert.Single(events);

        // This family is the only one carrying BOTH an action and a state, and the two are drawn from
        // different vocabularies: the action is add/remove, the state is the ISSUABLE's state. The
        // milestone's own active/closed state lives on the embedded milestone.
        Assert.Equal("add", milestoneEvent.Action);
        Assert.Equal("opened", milestoneEvent.State);

        Assert.NotNull(milestoneEvent.Milestone);
        Assert.Equal(61, milestoneEvent.Milestone!.Id);
        Assert.Equal(9, milestoneEvent.Milestone.Iid);
        Assert.Equal("v1.2", milestoneEvent.Milestone.Title);
        Assert.Equal("active", milestoneEvent.Milestone.State);
        Assert.Equal(new DateOnly(2020, 2, 15), milestoneEvent.Milestone.DueDate);
        Assert.Equal(new Uri("https://gitlab.example/group/project/-/milestones/9"),
            milestoneEvent.Milestone.WebUrl);
    }

    [Fact]
    public async Task GetIssueMilestoneEventAsync_BuildsSingleEventRoute()
    {
        string json = $$"""
                        {
                          "id": 142,
                          "user": {{UserJson}},
                          "created_at": "2018-08-20T13:38:20.077Z",
                          "resource_type": "Issue",
                          "resource_id": 11,
                          "action": "remove",
                          "state": "closed"
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceMilestoneEvent milestoneEvent = await repository.GetIssueMilestoneEventAsync(11, 42, 142,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/11/issues/42/resource_milestone_events/142",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("remove", milestoneEvent.Action);
        Assert.Equal("closed", milestoneEvent.State);

        // A milestone event whose milestone has since been deleted still deserializes.
        Assert.Null(milestoneEvent.Milestone);
    }

    [Fact]
    public async Task ListIssueIterationEventsAsync_BuildsRoute_AndReusesGitLabIteration()
    {
        string json = $$"""
                        [
                          {
                            "id": 142,
                            "user": {{UserJson}},
                            "created_at": "2018-08-20T13:38:20.077Z",
                            "resource_type": "Issue",
                            "resource_id": 11,
                            "iteration": {
                              "id": 53,
                              "iid": 13,
                              "sequence": 1,
                              "group_id": 5,
                              "title": null,
                              "description": null,
                              "state": 2,
                              "created_at": "2020-01-27T05:07:12.573Z",
                              "updated_at": "2020-01-27T05:07:12.573Z",
                              "start_date": "2020-01-27",
                              "due_date": "2020-02-05",
                              "web_url": "https://gitlab.example/groups/group1/-/iterations/13"
                            },
                            "action": "add"
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceIterationEvent> events = [];
        await foreach (GitLabResourceIterationEvent item in repository.ListIssueIterationEventsAsync(11, 42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/11/issues/42/resource_iteration_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceIterationEvent iterationEvent = Assert.Single(events);
        Assert.Equal("add", iterationEvent.Action);
        Assert.NotNull(iterationEvent.Iteration);
        Assert.Equal(53, iterationEvent.Iteration!.Id);
        Assert.Equal(13, iterationEvent.Iteration.Iid);

        // Only Id is required on GitLabIteration precisely so a reduced projection like this one
        // (null title/description) deserializes instead of throwing.
        Assert.Null(iterationEvent.Iteration.Title);
        Assert.Null(iterationEvent.Iteration.Description);
        Assert.Equal(2, iterationEvent.Iteration.State);
        Assert.Equal(new DateOnly(2020, 2, 5), iterationEvent.Iteration.DueDate);
    }

    [Fact]
    public async Task GetIssueIterationEventAsync_BuildsSingleEventRoute()
    {
        string json = $$"""
                        {
                          "id": 142,
                          "user": {{UserJson}},
                          "created_at": "2018-08-20T13:38:20.077Z",
                          "resource_type": "Issue",
                          "resource_id": 11,
                          "iteration": { "id": 53, "iid": 13 },
                          "action": "add"
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceIterationEvent iterationEvent = await repository.GetIssueIterationEventAsync(11, 42, 142,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/11/issues/42/resource_iteration_events/142",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(53, iterationEvent.Iteration?.Id);
    }

    [Fact]
    public async Task ListIssueWeightEventsAsync_BuildsRoute_AndKeepsIssueIdShape()
    {
        string json = $$"""
                        [
                          {
                            "id": 142,
                            "user": {{UserJson}},
                            "created_at": "2018-08-20T13:38:20.077Z",
                            "issue_id": 253,
                            "weight": 3
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceWeightEvent> events = [];
        await foreach (GitLabResourceWeightEvent item in repository.ListIssueWeightEventsAsync(11, 42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/11/issues/42/resource_weight_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceWeightEvent weightEvent = Assert.Single(events);
        Assert.Equal(142, weightEvent.Id);

        // The odd one out: issue_id rather than resource_type/resource_id.
        Assert.Equal(253, weightEvent.IssueId);
        Assert.Equal(3, weightEvent.Weight);
        Assert.Equal("root", weightEvent.User?.Username);
    }

    [Fact]
    public async Task GetIssueWeightEventAsync_BuildsSingleEventRoute()
    {
        string json = $$"""
                        {
                          "id": 142,
                          "user": {{UserJson}},
                          "created_at": "2018-08-20T13:38:20.077Z",
                          "issue_id": 253,
                          "weight": null
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceWeightEvent weightEvent = await repository.GetIssueWeightEventAsync(11, 42, 142,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/11/issues/42/resource_weight_events/142",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // Weight is nullable because clearing an issue's weight records an event with a null weight.
        Assert.Null(weightEvent.Weight);
    }

    [Fact]
    public async Task ListMergeRequestLabelEventsAsync_BuildsMergeRequestRoute()
    {
        string json = $$"""
                        [
                          {
                            "id": 119,
                            "user": {{UserJson}},
                            "created_at": "2018-08-21T14:38:20.077Z",
                            "resource_type": "MergeRequest",
                            "resource_id": 28,
                            "label": {
                              "id": 74,
                              "name": "p1",
                              "color": "#0033CC",
                              "description": "",
                              "text_color": "#FFFFFF"
                            },
                            "action": "remove"
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceLabelEvent> events = [];
        await foreach (GitLabResourceLabelEvent item in repository.ListMergeRequestLabelEventsAsync(
                           "gitlab-org/gitlab", 7, cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/7/resource_label_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceLabelEvent labelEvent = Assert.Single(events);
        Assert.Equal("MergeRequest", labelEvent.ResourceType);
        Assert.Equal("remove", labelEvent.Action);
        Assert.Equal("p1", labelEvent.Label?.Name);
    }

    [Fact]
    public async Task GetMergeRequestLabelEventAsync_BuildsSingleEventRoute()
    {
        string json = $$"""
                        {
                          "id": 119,
                          "user": {{UserJson}},
                          "created_at": "2018-08-21T14:38:20.077Z",
                          "resource_type": "MergeRequest",
                          "resource_id": 28,
                          "label": null,
                          "action": "add"
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceLabelEvent labelEvent = await repository.GetMergeRequestLabelEventAsync(11, 7, 119,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/11/merge_requests/7/resource_label_events/119",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // GitLab keeps the event and drops the label once the label itself is deleted.
        Assert.Null(labelEvent.Label);
    }

    [Fact]
    public async Task ListMergeRequestStateEventsAsync_BuildsMergeRequestRoute_AndDeserializesMergedState()
    {
        string json = $$"""
                        [
                          {
                            "id": 143,
                            "user": {{UserJson}},
                            "created_at": "2018-08-22T09:00:00.000Z",
                            "resource_type": "MergeRequest",
                            "resource_id": 28,
                            "state": "merged"
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceStateEvent> events = [];
        await foreach (GitLabResourceStateEvent item in repository.ListMergeRequestStateEventsAsync(11, 7,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/11/merge_requests/7/resource_state_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceStateEvent stateEvent = Assert.Single(events);
        Assert.Equal("merged", stateEvent.State);
        Assert.Null(stateEvent.SourceCommit);
        Assert.Null(stateEvent.SourceMergeRequestId);
    }

    [Fact]
    public async Task GetMergeRequestStateEventAsync_BuildsSingleEventRoute()
    {
        string json = $$"""
                        {
                          "id": 143,
                          "user": {{UserJson}},
                          "created_at": "2018-08-22T09:00:00.000Z",
                          "resource_type": "MergeRequest",
                          "resource_id": 28,
                          "state": "locked"
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceStateEvent stateEvent = await repository.GetMergeRequestStateEventAsync(11, 7, 143,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/11/merge_requests/7/resource_state_events/143",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // "locked" only ever appears on a merge request, but it shares the enum with issue state events.
        Assert.Equal("locked", stateEvent.State);
    }

    [Fact]
    public async Task ListMergeRequestMilestoneEventsAsync_BuildsMergeRequestRoute()
    {
        string json = $$"""
                        [
                          {
                            "id": 144,
                            "user": {{UserJson}},
                            "created_at": "2018-08-22T09:05:00.000Z",
                            "resource_type": "MergeRequest",
                            "resource_id": 28,
                            "milestone": {
                              "id": 61,
                              "iid": 9,
                              "project_id": 7,
                              "title": "v1.2",
                              "state": "active",
                              "web_url": "https://gitlab.example/group/project/-/milestones/9"
                            },
                            "action": "remove",
                            "state": "closed"
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceMilestoneEvent> events = [];
        await foreach (GitLabResourceMilestoneEvent item in repository.ListMergeRequestMilestoneEventsAsync(11, 7,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/11/merge_requests/7/resource_milestone_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceMilestoneEvent milestoneEvent = Assert.Single(events);
        Assert.Equal("remove", milestoneEvent.Action);
        Assert.Equal("closed", milestoneEvent.State);
        Assert.Equal("v1.2", milestoneEvent.Milestone?.Title);
    }

    [Fact]
    public async Task GetMergeRequestMilestoneEventAsync_BuildsSingleEventRoute()
    {
        string json = $$"""
                        {
                          "id": 144,
                          "user": {{UserJson}},
                          "created_at": "2018-08-22T09:05:00.000Z",
                          "resource_type": "MergeRequest",
                          "resource_id": 28,
                          "action": "add",
                          "state": "opened"
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceMilestoneEvent milestoneEvent = await repository.GetMergeRequestMilestoneEventAsync(11, 7, 144,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/11/merge_requests/7/resource_milestone_events/144",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("add", milestoneEvent.Action);
    }

    [Fact]
    public async Task ListEpicLabelEventsAsync_BuildsGroupRoute_AndEncodesNamespacedGroupPath()
    {
        string json = $$"""
                        [
                          {
                            "id": 106,
                            "user": {{UserJson}},
                            "created_at": "2018-08-19T11:43:01.746Z",
                            "resource_type": "Epic",
                            "resource_id": 33,
                            "label": {
                              "id": 73,
                              "name": "a1",
                              "color": "#34495E",
                              "description": "",
                              "text_color": "#FFFFFF"
                            },
                            "action": "add"
                          }
                        ]
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceLabelEvent> events = [];
        await foreach (GitLabResourceLabelEvent item in repository.ListEpicLabelEventsAsync("gitlab-org/subgroup", 11,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        // Epic events are the only ones rooted at /groups rather than /projects.
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/epics/11/resource_label_events",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabResourceLabelEvent labelEvent = Assert.Single(events);
        Assert.Equal("Epic", labelEvent.ResourceType);
        Assert.Equal("add", labelEvent.Action);
    }

    [Fact]
    public async Task GetEpicLabelEventAsync_BuildsSingleEventRoute()
    {
        string json = $$"""
                        {
                          "id": 107,
                          "user": {{UserJson}},
                          "created_at": "2018-08-19T11:43:01.746Z",
                          "resource_type": "Epic",
                          "resource_id": 33,
                          "action": "remove"
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceLabelEvent labelEvent = await repository.GetEpicLabelEventAsync(5, 11, 107,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/11/resource_label_events/107",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(107, labelEvent.Id);
        Assert.Equal("Epic", labelEvent.ResourceType);
    }

    [Fact]
    public async Task ListEpicStateEventsAsync_BuildsGroupRoute_AndHonoursPerPage()
    {
        using StubHttpMessageHandler handler = JsonHandler("[]");
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        await foreach (GitLabResourceStateEvent _ in repository.ListEpicStateEventsAsync(5, 11,
                           new ResourceEventListOptions { PerPage = 50 }, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/11/resource_state_events?per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetEpicStateEventAsync_BuildsSingleEventRoute()
    {
        string json = $$"""
                        {
                          "id": 142,
                          "user": {{UserJson}},
                          "created_at": "2018-08-20T13:38:20.077Z",
                          "resource_type": "Epic",
                          "resource_id": 33,
                          "state": "closed"
                        }
                        """;

        using StubHttpMessageHandler handler = JsonHandler(json);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabResourceStateEvent stateEvent = await repository.GetEpicStateEventAsync(5, 11, 142,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/11/resource_state_events/142",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("closed", stateEvent.State);
    }

    [Fact]
    public async Task ListIssueLabelEventsAsync_FollowsTheLinkHeaderAcrossPages()
    {
        const string FirstPage = """
                                 [ { "id": 1, "action": "add" } ]
                                 """;

        const string SecondPage = """
                                  [ { "id": 2, "action": "remove" } ]
                                  """;

        List<Uri?> requested = [];
        using StubHttpMessageHandler handler = new(request =>
        {
            requested.Add(request.RequestUri);

            bool isFirst = requested.Count == 1;
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(isFirst ? FirstPage : SecondPage, Encoding.UTF8, "application/json")
            };

            if (isFirst)
            {
                response.Headers.TryAddWithoutValidation(
                    "Link",
                    "<https://gitlab.example/api/v4/projects/11/issues/42/resource_label_events?page=2>; rel=\"next\"");
            }

            return response;
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceLabelEvent> events = [];
        await foreach (GitLabResourceLabelEvent item in repository.ListIssueLabelEventsAsync(11, 42,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            events.Add(item);
        }

        Assert.Equal(2, events.Count);
        Assert.Equal(1, events[0].Id);
        Assert.Equal(2, events[1].Id);
        Assert.Equal(2, requested.Count);
        Assert.EndsWith("page=2", requested[1]?.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListIssueWeightEventsAsync_OnMissingIssue_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Issue Not Found" }""";

        using StubHttpMessageHandler handler = JsonHandler(Json, HttpStatusCode.NotFound);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(async () =>
        {
            await foreach (GitLabResourceWeightEvent _ in repository
                               .ListIssueWeightEventsAsync(11, 999,
                                   cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                Assert.Fail("The stub never returns a successful page.");
            }
        });

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Issue Not Found", exception.Message);
    }

    [Fact]
    public async Task GetIssueLabelEventAsync_OnMissingEvent_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Resource label event Not Found" }""";

        using StubHttpMessageHandler handler = JsonHandler(Json, HttpStatusCode.NotFound);
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetIssueLabelEventAsync(11, 42, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    /// <summary>
    ///     The spec types <c>action</c>, <c>resource_type</c> and <c>state</c> as bare strings with no
    ///     enumeration, so GitLab is free to add a value at any release. They stay <see cref="string" /> and an
    ///     unrecognised value must reach the caller intact rather than failing the whole page - a client that
    ///     throws on data it merely does not recognise is worse than one that hands it over.
    /// </summary>
    [Fact]
    public async Task ListIssueLabelEventsAsync_OnUnknownAction_SurfacesTheRawValue()
    {
        using StubHttpMessageHandler handler = JsonHandler("""[ { "id": 1, "action": "teleported" } ]""");
        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        ResourceEventsClient repository = new(connection);

        List<GitLabResourceLabelEvent> events = [];
        await foreach (GitLabResourceLabelEvent labelEvent in repository
                           .ListIssueLabelEventsAsync(11, 42,
                               cancellationToken: TestContext.Current.CancellationToken)
                           .ConfigureAwait(false))
        {
            events.Add(labelEvent);
        }

        Assert.Equal("teleported", Assert.Single(events).Action);
    }

    private static StubHttpMessageHandler JsonHandler(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new StubHttpMessageHandler(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }

    private static HttpClient CreateHttpClient(StubHttpMessageHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
    }
}