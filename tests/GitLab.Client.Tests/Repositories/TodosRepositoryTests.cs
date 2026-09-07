using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class TodosRepositoryTests
{
    /// <summary>A project-scoped to-do item, shaped like GitLab's <c>APIEntitiesTodo</c>.</summary>
    private const string TodoJson = """
                                    {
                                      "id": 102,
                                      "project": {
                                        "id": 2,
                                        "name": "Gitlab Ce",
                                        "name_with_namespace": "Gitlab Org / Gitlab Ce",
                                        "path": "gitlab-ce",
                                        "path_with_namespace": "gitlab-org/gitlab-ce",
                                        "created_at": "2016-06-15T10:09:34.206Z"
                                      },
                                      "author": {
                                        "id": 1,
                                        "username": "root",
                                        "name": "Administrator",
                                        "state": "active",
                                        "web_url": "https://gitlab.example/root"
                                      },
                                      "action_name": "marked",
                                      "target_type": "MergeRequest",
                                      "target_url": "https://gitlab.example/gitlab-org/gitlab-ce/-/merge_requests/7",
                                      "body": "Vel voluptas atque dicta mollitia.",
                                      "state": "pending",
                                      "created_at": "2016-06-17T07:52:35.225Z",
                                      "updated_at": "2016-06-17T07:52:35.225Z"
                                    }
                                    """;

    /// <summary>A group-scoped to-do item: <c>group</c> is populated instead of <c>project</c>.</summary>
    private const string GroupTodoJson = """
                                         {
                                           "id": 103,
                                           "group": {
                                             "id": 41,
                                             "name": "Gitlab Org",
                                             "path": "gitlab-org",
                                             "kind": "group",
                                             "full_path": "gitlab-org",
                                             "parent_id": null,
                                             "web_url": "https://gitlab.example/groups/gitlab-org"
                                           },
                                           "action_name": "member_access_requested",
                                           "target_type": "Namespace",
                                           "target_url": "https://gitlab.example/groups/gitlab-org/-/group_members",
                                           "body": "Access requested",
                                           "state": "pending",
                                           "created_at": "2016-06-17T07:52:35.225Z",
                                           "updated_at": "2016-06-17T07:52:35.225Z"
                                         }
                                         """;

    [Fact]
    public async Task ListAsync_BuildsRootTodosRoute_WithQueryOptions_AndDeserializesTodos()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{TodoJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        TodoListOptions options = new()
        {
            Action = "marked",
            AuthorId = 1,
            ProjectId = 2,
            GroupId = 41,
            State = "pending",
            Type = "MergeRequest",
            PerPage = 50
        };

        List<GitLabTodo> todos = new();
        await foreach (GitLabTodo item in repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            todos.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.StartsWith("https://gitlab.example/api/v4/todos?", requestUri, StringComparison.Ordinal);
        Assert.Contains("action=marked", requestUri, StringComparison.Ordinal);
        Assert.Contains("author_id=1", requestUri, StringComparison.Ordinal);
        Assert.Contains("project_id=2", requestUri, StringComparison.Ordinal);
        Assert.Contains("group_id=41", requestUri, StringComparison.Ordinal);
        Assert.Contains("state=pending", requestUri, StringComparison.Ordinal);
        Assert.Contains("type=MergeRequest", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=50", requestUri, StringComparison.Ordinal);

        GitLabTodo todo = Assert.Single(todos);
        Assert.Equal(102, todo.Id);
        Assert.Equal("marked", todo.ActionName);
        Assert.Equal("MergeRequest", todo.TargetType);
        Assert.Equal("pending", todo.State);
        Assert.Equal("Vel voluptas atque dicta mollitia.", todo.Body);
        Assert.Equal(new Uri("https://gitlab.example/gitlab-org/gitlab-ce/-/merge_requests/7"), todo.TargetUrl);
        Assert.Equal(new DateTimeOffset(2016, 6, 17, 7, 52, 35, 225, TimeSpan.Zero), todo.CreatedAt);

        Assert.NotNull(todo.Project);
        Assert.Equal(2, todo.Project!.Id);
        Assert.Equal("Gitlab Ce", todo.Project.Name);
        Assert.Equal("gitlab-org/gitlab-ce", todo.Project.PathWithNamespace);
        Assert.Null(todo.Group);

        Assert.NotNull(todo.Author);
        Assert.Equal("root", todo.Author!.Username);
    }

    [Fact]
    public async Task ListAsync_WithNoOptions_RequestsTheBareTodosRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{GroupTodoJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        List<GitLabTodo> todos = new();
        await foreach (GitLabTodo item in repository.ListAsync(cancellationToken: TestContext.Current
                           .CancellationToken))
        {
            todos.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/todos", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabTodo todo = Assert.Single(todos);
        Assert.Equal(103, todo.Id);
        Assert.Null(todo.Project);
        Assert.NotNull(todo.Group);
        Assert.Equal(41, todo.Group!.Id);
        Assert.Equal("group", todo.Group.Kind);
        Assert.Equal("gitlab-org", todo.Group.FullPath);
        Assert.Null(todo.Group.ParentId);
        Assert.Equal(new Uri("https://gitlab.example/groups/gitlab-org"), todo.Group.WebUrl);
    }

    [Fact]
    public async Task MarkAsDoneAsync_PostsEmptyBodyToTodoRoute_AndDeserializesUpdatedTodo()
    {
        const string DoneJson = """
                                {
                                  "id": 102,
                                  "action_name": "marked",
                                  "target_type": "MergeRequest",
                                  "state": "done",
                                  "created_at": "2016-06-17T07:52:35.225Z",
                                  "updated_at": "2016-06-17T08:01:00.000Z"
                                }
                                """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(DoneJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        GitLabTodo todo = await repository.MarkAsDoneAsync(102, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/todos/102/mark_as_done",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);

        Assert.Equal(102, todo.Id);
        Assert.Equal("done", todo.State);
        Assert.Null(todo.Project);
    }

    [Fact]
    public async Task MarkAllAsDoneAsync_PostsToCollectionRoute_AndToleratesAnEmptyBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        await repository.MarkAllAsDoneAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/todos/mark_as_done",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Fact]
    public async Task CreateForIssueAsync_PostsToSingularTodoSegment_AndDeserializesTodo()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(TodoJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        GitLabTodo todo = await repository.CreateForIssueAsync(2, 40, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/2/issues/40/todo",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal(102, todo.Id);
        Assert.Equal("pending", todo.State);
    }

    [Fact]
    public async Task CreateForIssueAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(TodoJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        await repository.CreateForIssueAsync("gitlab-org/gitlab", 40, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/40/todo",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForMergeRequestAsync_PostsToSingularTodoSegment_AndDeserializesTodo()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(TodoJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        GitLabTodo todo = await repository.CreateForMergeRequestAsync("gitlab-org/gitlab", 7,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/7/todo",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("MergeRequest", todo.TargetType);
    }

    /// <summary>
    ///     GitLab answers 304 when a to-do item already exists for the user on that issuable. 304 is not a
    ///     success status, so the transport throws instead of returning silently; it has no dedicated derived
    ///     exception type, so the base type is what callers catch.
    /// </summary>
    [Fact]
    public async Task CreateForIssueAsync_WhenTodoAlreadyExists_ThrowsWithNotModified()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotModified));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabApiException>(() =>
            repository.CreateForIssueAsync(2, 40, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotModified, exception.StatusCode);
    }

    [Fact]
    public async Task MarkAsDoneAsync_OnMissingTodo_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Not found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.MarkAsDoneAsync(999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Not found", exception.Message);
    }

    [Fact]
    public async Task MarkAllAsDoneAsync_OnUnauthorized_ThrowsGitLabAuthenticationException()
    {
        const string Json = """{ "message": "401 Unauthorized" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TodosRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabAuthenticationException>(() =>
            repository.MarkAllAsDoneAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("401 Unauthorized", exception.Message);
    }
}