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

public sealed class LabelsEndpointTests
{
    /// <summary>A project label: the only shape carrying <c>priority</c> and <c>is_project_label</c>.</summary>
    private const string ProjectLabelJson = """
                                            {
                                              "id": 1,
                                              "name": "type::bug",
                                              "description": "Something is broken",
                                              "description_html": "<p>Something is broken</p>",
                                              "text_color": "#FFFFFF",
                                              "color": "#d9534f",
                                              "archived": false,
                                              "open_issues_count": 3,
                                              "closed_issues_count": 5,
                                              "open_merge_requests_count": 1,
                                              "subscribed": false,
                                              "priority": 10,
                                              "is_project_label": true
                                            }
                                            """;

    /// <summary>A group label: the same payload minus the two project-only members.</summary>
    private const string GroupLabelJson = """
                                          {
                                            "id": 7,
                                            "name": "workflow::in review",
                                            "description": "Waiting on a reviewer",
                                            "description_html": "<p>Waiting on a reviewer</p>",
                                            "text_color": "#333333",
                                            "color": "#5cb85c",
                                            "archived": false,
                                            "open_issues_count": 2,
                                            "closed_issues_count": 0,
                                            "open_merge_requests_count": 4,
                                            "subscribed": true
                                          }
                                          """;

    [Fact]
    public async Task ListAsync_BuildsProjectScopedRoute_AndDeserializesLabels()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "name": "bug",
                                "description": "Something isn't working",
                                "color": "#d9534f",
                                "text_color": "#FFFFFF",
                                "archived": false,
                                "open_issues_count": 3,
                                "closed_issues_count": 5,
                                "open_merge_requests_count": 1,
                                "priority": null
                              },
                              {
                                "id": 2,
                                "name": "enhancement",
                                "color": "#5cb85c"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        List<GitLabLabel> labels = new();
        await foreach (GitLabLabel label in repository.ListAsync("gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            labels.Add(label);
        }

        Assert.Contains("/projects/gitlab-org%2Fgitlab/labels", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, labels.Count);
        Assert.Equal("bug", labels[0].Name);
        Assert.Equal("#d9534f", labels[0].Color);
        Assert.Equal(3, labels[0].OpenIssuesCount);
        Assert.Equal("enhancement", labels[1].Name);
    }

    [Fact]
    public async Task ListAsync_AppliesSearchAndPerPageQueryParameters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        LabelListOptions options = new() { Search = "bug", PerPage = 50 };

        await foreach (GitLabLabel _ in repository.ListAsync(1, options, TestContext.Current.CancellationToken))
        {
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/1/labels", requestUri);
        Assert.Contains("search=bug", requestUri);
        Assert.Contains("per_page=50", requestUri);
    }

    [Fact]
    public async Task ListAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Project Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(async () =>
        {
            await foreach (GitLabLabel _ in repository
                               .ListAsync(999, cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
            }
        });

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Project Not Found", exception.Message);
    }

    [Fact]
    public async Task ListAsync_AppliesTheAncestorAndArchivedFilters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        LabelListOptions options = new() { WithCounts = true, IncludeAncestorGroups = true, Archived = false };

        await foreach (GitLabLabel _ in repository.ListAsync(1, options, TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/1/labels"
            + "?with_counts=true&include_ancestor_groups=true&archived=false",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_PercentEncodesALabelNameWithSeparatorsAndSpaces()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProjectLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        // Label names are free text: "::" scoping, spaces and slashes are all legal, and all of them have
        // to survive as ONE path segment or GitLab answers 404 against a route that does not exist.
        GitLabLabel label = await repository.GetAsync("gitlab-org/gitlab", "group::needs review/urgent",
            new LabelGetOptions { IncludeAncestorGroups = true }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/labels/"
            + "group%3A%3Aneeds%20review%2Furgent?include_ancestor_groups=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(1, label.Id);
        Assert.Equal("type::bug", label.Name);
        Assert.Equal("<p>Something is broken</p>", label.DescriptionHtml);
        Assert.Equal(10, label.Priority);
        Assert.True(label.IsProjectLabel);
        Assert.False(label.Subscribed);
    }

    [Fact]
    public async Task CreateAsync_PostsTheLabelBody_IncludingPriority()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ProjectLabelJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        CreateLabelRequest request = new()
        {
            Name = "type::bug", Color = "#d9534f", Description = "Something is broken", Priority = 10
        };

        GitLabLabel label = await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/labels",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"name":"type::bug","color":"#d9534f","description":"Something is broken","priority":10}""",
            sentBody);
        Assert.Equal("type::bug", label.Name);
    }

    [Fact]
    public async Task UpdateAsync_PutsToTheNamedRoute_AndSendsNewName()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ProjectLabelJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        UpdateLabelRequest request = new() { NewName = "type::defect", Archived = true };

        await repository.UpdateAsync(42, "type::bug", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/labels/type%3A%3Abug",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"new_name":"type::defect","archived":true}""", sentBody);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheNamedRoute_AndReturnsTheDeletedProjectLabel()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProjectLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        GitLabLabel deleted = await repository.DeleteAsync(42, "type::bug", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/labels/type%3A%3Abug",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, deleted.Id);
        Assert.Equal("type::bug", deleted.Name);
        Assert.Equal(10, deleted.Priority);
    }

    [Fact]
    public async Task PromoteAsync_PutsToThePromoteRoute_WithNoRequestBody_AndReturnsTheGroupLabel()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        GitLabGroupLabel promoted = await repository.PromoteAsync(42, "type::bug",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/labels/type%3A%3Abug/promote",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // The endpoint declares no request body, so none is sent.
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(7, promoted.Id);
        Assert.Equal("workflow::in review", promoted.Name);
        Assert.True(promoted.Subscribed);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupRoute_WithEveryFilter_AndDeserializesGroupLabels()
    {
        string json = $"[{GroupLabelJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        GroupLabelListOptions options = new()
        {
            WithCounts = true,
            IncludeAncestorGroups = false,
            IncludeDescendantGroups = true,
            OnlyGroupLabels = true,
            Search = "workflow",
            Archived = false,
            PerPage = 20
        };

        List<GitLabGroupLabel> labels = new();
        await foreach (GitLabGroupLabel label in repository.ListForGroupAsync("parent-group/subgroup", options,
                           TestContext.Current.CancellationToken))
        {
            labels.Add(label);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/labels"
            + "?with_counts=true&include_ancestor_groups=false&include_descendant_groups=true"
            + "&only_group_labels=true&search=workflow&archived=false&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabGroupLabel single = Assert.Single(labels);
        Assert.Equal(7, single.Id);
        Assert.Equal("workflow::in review", single.Name);
        Assert.Equal("#5cb85c", single.Color);
        Assert.Equal("#333333", single.TextColor);
        Assert.Equal(4, single.OpenMergeRequestsCount);
        Assert.True(single.Subscribed);
    }

    [Fact]
    public async Task GetForGroupAsync_EscapesTheLabelName_AndAppliesTheGroupScopeFilters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        GroupLabelGetOptions options = new() { IncludeDescendantGroups = true, OnlyGroupLabels = true };

        GitLabGroupLabel label = await repository.GetForGroupAsync(9970, "workflow::in review", options,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/labels/workflow%3A%3Ain%20review"
            + "?include_descendant_groups=true&only_group_labels=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("workflow::in review", label.Name);
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsAGroupLabelBody_WithoutAProjectOnlyPriority()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(GroupLabelJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        CreateGroupLabelRequest request = new() { Name = "workflow::in review", Color = "#5cb85c", Archived = false };

        await repository.CreateForGroupAsync(9970, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/labels",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"workflow::in review","color":"#5cb85c","archived":false}""", sentBody);
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsToTheNamedGroupRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GroupLabelJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        UpdateGroupLabelRequest request = new() { NewName = "workflow::reviewing", Color = "#5cb85c" };

        await repository.UpdateForGroupAsync("parent-group/subgroup", "workflow::in review", request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/labels/workflow%3A%3Ain%20review",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"new_name":"workflow::reviewing","color":"#5cb85c"}""", sentBody);
    }

    [Fact]
    public async Task UpdateForGroupByLabelIdAsync_PutsToTheCollectionRoute_AndCarriesTheIdInTheBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GroupLabelJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        UpdateGroupLabelByIdRequest request = new() { LabelId = 7, NewName = "workflow::reviewing" };

        await repository.UpdateForGroupByLabelIdAsync(9970, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);

        // The label is named in the body here, so the route carries no name segment at all.
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/labels",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"label_id":7,"new_name":"workflow::reviewing"}""", sentBody);
    }

    [Fact]
    public async Task DeleteForGroupAsync_SendsDeleteToTheNamedGroupRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        await repository.DeleteForGroupAsync(9970, "workflow::in review", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/labels/workflow%3A%3Ain%20review",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForGroupByQueryAsync_SendsNameAsAQueryParameter_AndReturnsTheDeletedLabel()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupLabelJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        GitLabGroupLabel deleted = await repository.DeleteForGroupByQueryAsync(9970, "workflow::in review",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/labels?name=workflow%3A%3Ain%20review",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("workflow::in review", deleted.Name);
        Assert.Equal(7, deleted.Id);
    }

    [Fact]
    public async Task CreateAsync_OnDuplicateName_ThrowsGitLabConflictException()
    {
        const string Json = """{ "message": "Label already exists" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        LabelsClient repository = new(connection);

        CreateLabelRequest request = new() { Name = "bug", Color = "#d9534f" };

        GitLabConflictException exception = await Assert.ThrowsAsync<GitLabConflictException>(() =>
            repository.CreateAsync(42, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("Label already exists", exception.Message);
    }
}