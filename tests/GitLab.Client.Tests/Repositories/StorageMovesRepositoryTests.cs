using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class StorageMovesRepositoryTests
{
    private const string ProjectMoveJson = """
                                           {
                                             "id": 1,
                                             "created_at": "2020-05-07T04:27:17.234Z",
                                             "state": "scheduled",
                                             "source_storage_name": "default",
                                             "destination_storage_name": "storage2",
                                             "error_message": null,
                                             "project": {
                                               "id": 1,
                                               "description": "Project description",
                                               "name": "gitlab",
                                               "name_with_namespace": "GitLab Org / GitLab",
                                               "path": "gitlab",
                                               "path_with_namespace": "gitlab-org/gitlab",
                                               "created_at": "2020-05-07T04:27:17.234Z"
                                             }
                                           }
                                           """;

    private const string GroupMoveJson = """
                                         {
                                           "id": 3,
                                           "created_at": "2020-05-07T04:27:17.234Z",
                                           "state": "cleanup_failed",
                                           "source_storage_name": "default",
                                           "destination_storage_name": "storage2",
                                           "error_message": "Failed to remove the source repository",
                                           "group": {
                                             "id": 283,
                                             "web_url": "https://gitlab.example/groups/gitlab-org",
                                             "name": "GitLab Org"
                                           }
                                         }
                                         """;

    private const string SnippetMoveJson = """
                                           {
                                             "id": 5,
                                             "created_at": "2020-05-07T04:27:17.234Z",
                                             "state": "finished",
                                             "source_storage_name": "default",
                                             "destination_storage_name": "storage3",
                                             "error_message": null,
                                             "snippet": {
                                               "id": 65,
                                               "title": "Some snippet",
                                               "description": null,
                                               "visibility": "internal",
                                               "author": {
                                                 "id": 7,
                                                 "username": "root",
                                                 "name": "Administrator",
                                                 "state": "active",
                                                 "avatar_url": "https://gitlab.example/uploads/avatar.png",
                                                 "web_url": "https://gitlab.example/root"
                                               },
                                               "created_at": "2020-05-06T12:00:00.000Z",
                                               "updated_at": "2020-05-06T12:30:00.000Z",
                                               "project_id": null,
                                               "web_url": "https://gitlab.example/-/snippets/65",
                                               "raw_url": "https://gitlab.example/-/snippets/65/raw",
                                               "ssh_url_to_repo": "git@gitlab.example:snippets/65.git",
                                               "http_url_to_repo": "https://gitlab.example/snippets/65.git"
                                             }
                                           }
                                           """;

    [Fact]
    public async Task ListAllProjectMovesAsync_BuildsTheInstanceWideRoute_AndDeserializesTheEmbeddedProject()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{ProjectMoveJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        List<GitLabProjectRepositoryStorageMove> moves = [];
        await foreach (GitLabProjectRepositoryStorageMove move in
                       repository.ListAllProjectMovesAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            moves.Add(move);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/project_repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabProjectRepositoryStorageMove single = Assert.Single(moves);
        Assert.Equal(1, single.Id);
        Assert.Equal(GitLabRepositoryStorageMoveState.Scheduled, single.State);
        Assert.Equal("default", single.SourceStorageName);
        Assert.Equal("storage2", single.DestinationStorageName);
        Assert.Null(single.ErrorMessage);
        Assert.Equal(new DateTimeOffset(2020, 5, 7, 4, 27, 17, 234, TimeSpan.Zero), single.CreatedAt);

        Assert.NotNull(single.Project);
        Assert.Equal(1, single.Project!.Id);
        Assert.Equal("gitlab", single.Project.Name);
        Assert.Equal("gitlab-org/gitlab", single.Project.PathWithNamespace);
    }

    [Fact]
    public async Task ListAllProjectMovesAsync_ProjectsPerPageOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        await foreach (GitLabProjectRepositoryStorageMove _ in repository.ListAllProjectMovesAsync(
                           new StorageMoveListOptions { PerPage = 100 }, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/project_repository_storage_moves?per_page=100",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetProjectMoveAsync_AddressesTheMoveById_NotTheProject()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProjectMoveJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        GitLabProjectRepositoryStorageMove move =
            await repository.GetProjectMoveAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/project_repository_storage_moves/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabRepositoryStorageMoveState.Scheduled, move.State);
    }

    [Fact]
    public async Task ScheduleAllProjectMovesAsync_PostsTheShardPair_AndToleratesAnEmpty202()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        await repository.ScheduleAllProjectMovesAsync(
            new ScheduleStorageShardMovesRequest { SourceStorageName = "default" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/project_repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // destination_storage_name is unset, so it must be omitted rather than sent as null - GitLab
        // reads its absence as "pick a destination from the configured weights".
        Assert.Equal("""{"source_storage_name":"default"}""", sentBody);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        await foreach (GitLabProjectRepositoryStorageMove _ in repository.ListForProjectAsync("gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsTheProjectScopedRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ProjectMoveJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        GitLabProjectRepositoryStorageMove move =
            await repository.GetForProjectAsync(1, 42, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/repository_storage_moves/42",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, move.Id);
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsTheDestinationShard_AndReadsBack201()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(ProjectMoveJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        GitLabProjectRepositoryStorageMove move = await repository.CreateForProjectAsync(
            "gitlab-org/gitlab",
            new CreateStorageMoveRequest { DestinationStorageName = "storage2" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"destination_storage_name":"storage2"}""", sentBody);
        Assert.Equal("storage2", move.DestinationStorageName);
    }

    [Fact]
    public async Task ListAllGroupMovesAsync_DeserializesTheBasicGroupEmbedding_AndAFailedCleanup()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{GroupMoveJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        List<GitLabGroupRepositoryStorageMove> moves = [];
        await foreach (GitLabGroupRepositoryStorageMove move in
                       repository.ListAllGroupMovesAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            moves.Add(move);
        }

        Assert.Equal("https://gitlab.example/api/v4/group_repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabGroupRepositoryStorageMove single = Assert.Single(moves);
        Assert.Equal(GitLabRepositoryStorageMoveState.CleanupFailed, single.State);
        Assert.Equal("Failed to remove the source repository", single.ErrorMessage);

        // The embedding is APIEntitiesBasicGroupDetails - id/name/web_url only. GitLabGroup would have
        // thrown here: it marks path and visibility required and neither is on the wire.
        Assert.NotNull(single.Group);
        Assert.Equal(283, single.Group!.Id);
        Assert.Equal("GitLab Org", single.Group.Name);
        Assert.Equal(new Uri("https://gitlab.example/groups/gitlab-org"), single.Group.WebUrl);
    }

    [Fact]
    public async Task ListForGroupAsync_EncodesNamespacedGroupPath_AndProjectsPerPage()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        await foreach (GitLabGroupRepositoryStorageMove _ in repository.ListForGroupAsync("parent-group/subgroup",
                           new StorageMoveListOptions { PerPage = 50 }, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/repository_storage_moves?per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetGroupMoveAsync_AndGetForGroupAsync_UseTheTwoDifferentRoots()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupMoveJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        await repository.GetGroupMoveAsync(3, TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/group_repository_storage_moves/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        await repository.GetForGroupAsync(283, 3, TestContext.Current.CancellationToken);
        Assert.Equal("https://gitlab.example/api/v4/groups/283/repository_storage_moves/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForGroupAsync_OmitsAnUnsetDestination_SoGitLabPicksTheShard()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(GroupMoveJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        GitLabGroupRepositoryStorageMove move = await repository.CreateForGroupAsync(283,
            new CreateStorageMoveRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/283/repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
        Assert.Equal(3, move.Id);
    }

    [Fact]
    public async Task ScheduleAllGroupMovesAsync_PostsToTheInstanceWideGroupRoot()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        await repository.ScheduleAllGroupMovesAsync(
            new ScheduleStorageShardMovesRequest { SourceStorageName = "default" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/group_repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAllSnippetMovesAsync_BuildsTheInstanceWideSnippetRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{SnippetMoveJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        List<GitLabSnippetRepositoryStorageMove> moves = [];
        await foreach (GitLabSnippetRepositoryStorageMove move in
                       repository.ListAllSnippetMovesAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            moves.Add(move);
        }

        Assert.Equal("https://gitlab.example/api/v4/snippet_repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5, Assert.Single(moves).Id);
    }

    [Fact]
    public async Task GetForSnippetAsync_BuildsTheSnippetScopedRetrieveRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SnippetMoveJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        GitLabSnippetRepositoryStorageMove move =
            await repository.GetForSnippetAsync(65, 5, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/snippets/65/repository_storage_moves/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabRepositoryStorageMoveState.Finished, move.State);
    }

    [Fact]
    public async Task ScheduleAllSnippetMovesAsync_SendsBothShardNames()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        await repository.ScheduleAllSnippetMovesAsync(
            new ScheduleStorageShardMovesRequest { SourceStorageName = "default", DestinationStorageName = "storage3" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/snippet_repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"source_storage_name":"default","destination_storage_name":"storage3"}""", sentBody);
    }

    [Fact]
    public async Task ListForSnippetAsync_BuildsTheSnippetScopedRoute_AndDeserializesTheSnippetEmbedding()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{SnippetMoveJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        List<GitLabSnippetRepositoryStorageMove> moves = [];
        await foreach (GitLabSnippetRepositoryStorageMove move in repository.ListForSnippetAsync(65,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            moves.Add(move);
        }

        Assert.Equal("https://gitlab.example/api/v4/snippets/65/repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabSnippetRepositoryStorageMove single = Assert.Single(moves);
        Assert.Equal(GitLabRepositoryStorageMoveState.Finished, single.State);

        Assert.NotNull(single.Snippet);
        Assert.Equal(65, single.Snippet!.Id);
        Assert.Equal("Some snippet", single.Snippet.Title);
        Assert.Equal("internal", single.Snippet.Visibility);
        Assert.Null(single.Snippet.ProjectId);
        // Kept as a string on purpose: git's SCP-like remote is not a valid absolute URI.
        Assert.Equal("git@gitlab.example:snippets/65.git", single.Snippet.SshUrlToRepo);
        Assert.Equal(new Uri("https://gitlab.example/snippets/65.git"), single.Snippet.HttpUrlToRepo);
        Assert.Equal("root", single.Snippet.Author?.Username);
    }

    [Fact]
    public async Task GetSnippetMoveAsync_UsesTheInstanceWideSnippetRoot()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SnippetMoveJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        GitLabSnippetRepositoryStorageMove move =
            await repository.GetSnippetMoveAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/snippet_repository_storage_moves/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5, move.Id);
    }

    [Fact]
    public async Task CreateForSnippetAsync_BuildsTheSnippetScopedRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(SnippetMoveJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        GitLabSnippetRepositoryStorageMove move = await repository.CreateForSnippetAsync(65,
            new CreateStorageMoveRequest { DestinationStorageName = "storage3" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/snippets/65/repository_storage_moves",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("storage3", move.DestinationStorageName);
    }

    [Fact]
    public async Task ListAllProjectMovesAsync_FollowsTheLinkHeaderAcrossPages()
    {
        using RecordingHttpMessageHandler handler = new((_, index) =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent($"[{ProjectMoveJson}]", Encoding.UTF8, "application/json")
            };

            if (index == 0)
            {
                response.Headers.TryAddWithoutValidation(
                    "Link",
                    "<https://gitlab.example/api/v4/project_repository_storage_moves?page=2>; rel=\"next\"");
            }

            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        List<GitLabProjectRepositoryStorageMove> moves = [];
        await foreach (GitLabProjectRepositoryStorageMove move in
                       repository.ListAllProjectMovesAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            moves.Add(move);
        }

        Assert.Equal(2, moves.Count);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("https://gitlab.example/api/v4/project_repository_storage_moves?page=2",
            handler.Requests[1].RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetProjectMoveAsync_On403_ThrowsForbidden_BecauseTheseAreAdminOnlyEndpoints()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"message":"403 Forbidden"}""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        StorageMovesRepository repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.GetProjectMoveAsync(1, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}