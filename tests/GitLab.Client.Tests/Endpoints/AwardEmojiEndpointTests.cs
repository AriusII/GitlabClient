using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class AwardEmojiEndpointTests
{
    /// <summary>A single reaction, shaped like GitLab's <c>APIEntitiesAwardEmoji</c>.</summary>
    private const string AwardJson = """
                                     {
                                       "id": 344,
                                       "name": "thumbsup",
                                       "user": {
                                         "id": 1,
                                         "username": "root",
                                         "name": "Administrator",
                                         "state": "active",
                                         "avatar_url": "https://gitlab.example/uploads/user/avatar/1/avatar.png",
                                         "web_url": "https://gitlab.example/root"
                                       },
                                       "created_at": "2016-06-15T10:09:34.206Z",
                                       "updated_at": "2016-06-15T10:09:34.206Z",
                                       "awardable_id": 80,
                                       "awardable_type": "Issue",
                                       "url": null
                                     }
                                     """;

    private const string AwardListJson = $"[{AwardJson}]";

    /// <summary>A reaction on a note, which reports <c>awardable_type: "Note"</c> and a custom-emoji URL.</summary>
    private const string NoteAwardJson = """
                                         {
                                           "id": 1,
                                           "name": "party_parrot",
                                           "user": {
                                             "id": 1,
                                             "username": "root",
                                             "name": "Administrator",
                                             "web_url": "https://gitlab.example/root"
                                           },
                                           "created_at": "2016-12-19T09:53:39.850Z",
                                           "updated_at": "2016-12-19T09:53:39.850Z",
                                           "awardable_id": 1,
                                           "awardable_type": "Note",
                                           "url": "https://gitlab.example/-/custom_emoji/party_parrot.gif"
                                         }
                                         """;

    private const string NoteAwardListJson = $"[{NoteAwardJson}]";

    /// <summary>A reaction on a project snippet, which is addressed by id rather than by iid.</summary>
    private const string SnippetAwardJson = """
                                            {
                                              "id": 500,
                                              "name": "tada",
                                              "user": {
                                                "id": 1,
                                                "username": "root",
                                                "name": "Administrator",
                                                "web_url": "https://gitlab.example/root"
                                              },
                                              "created_at": "2024-05-02T08:00:00.000Z",
                                              "updated_at": "2024-05-02T08:00:00.000Z",
                                              "awardable_id": 11,
                                              "awardable_type": "Snippet",
                                              "url": null
                                            }
                                            """;

    private const string SnippetAwardListJson = $"[{SnippetAwardJson}]";

    /// <summary>A reaction on a group epic - the one awardable rooted at a group, not a project.</summary>
    private const string EpicAwardJson = """
                                         {
                                           "id": 700,
                                           "name": "rocket",
                                           "user": {
                                             "id": 1,
                                             "username": "root",
                                             "name": "Administrator",
                                             "web_url": "https://gitlab.example/root"
                                           },
                                           "created_at": "2024-05-03T09:30:00.000Z",
                                           "updated_at": "2024-05-03T09:30:00.000Z",
                                           "awardable_id": 42,
                                           "awardable_type": "Epic",
                                           "url": null
                                         }
                                         """;

    private const string EpicAwardListJson = $"[{EpicAwardJson}]";

    [Fact]
    public async Task ListForIssueAsync_BuildsSingularAwardEmojiRoute_AndDeserializesReactions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AwardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        List<GitLabAwardEmoji> awards = new();
        await foreach (GitLabAwardEmoji item in repository.ListForIssueAsync(1, 80,
                           TestContext.Current.CancellationToken))
        {
            awards.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/80/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabAwardEmoji award = Assert.Single(awards);
        Assert.Equal(344, award.Id);
        Assert.Equal("thumbsup", award.Name);
        Assert.Equal(80, award.AwardableId);
        Assert.Equal("Issue", award.AwardableType);
        Assert.Null(award.Url);
        Assert.Equal(new DateTimeOffset(2016, 6, 15, 10, 9, 34, 206, TimeSpan.Zero), award.CreatedAt);
        Assert.Equal(new DateTimeOffset(2016, 6, 15, 10, 9, 34, 206, TimeSpan.Zero), award.UpdatedAt);
        Assert.NotNull(award.User);
        Assert.Equal("root", award.User!.Username);
        Assert.Equal("Administrator", award.User.Name);
    }

    [Fact]
    public async Task ListForIssueAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AwardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await foreach (GitLabAwardEmoji _ in repository.ListForIssueAsync("gitlab-org/gitlab", 80,
                           TestContext.Current.CancellationToken))
        {
            // Enumerated only to force the request; the payload itself is asserted elsewhere.
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/80/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForIssueAsync_BuildsAwardRoute_AndDeserializesReaction()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AwardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.GetForIssueAsync(1, 80, 344,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/80/award_emoji/344",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(344, award.Id);
        Assert.Equal("thumbsup", award.Name);
        Assert.Equal("Issue", award.AwardableType);
    }

    [Fact]
    public async Task AddToIssueAsync_PostsNameWithoutColons_AndDeserializesCreatedReaction()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(AwardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.AddToIssueAsync(1, 80,
            new CreateAwardEmojiRequest { Name = "thumbsup" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/80/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"name":"thumbsup"}""", sentBody);
        Assert.Equal(344, award.Id);
        Assert.Equal("thumbsup", award.Name);
    }

    [Fact]
    public async Task DeleteFromIssueAsync_SendsDeleteToAwardRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await repository.DeleteFromIssueAsync(1, 80, 344, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/80/award_emoji/344",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForIssueNoteAsync_BuildsIssueNoteRoute_AndDeserializesReactions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NoteAwardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        List<GitLabAwardEmoji> awards = new();
        await foreach (GitLabAwardEmoji item in repository.ListForIssueNoteAsync(1, 80, 1,
                           TestContext.Current.CancellationToken))
        {
            awards.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/80/notes/1/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabAwardEmoji award = Assert.Single(awards);
        Assert.Equal(1, award.Id);
        Assert.Equal("party_parrot", award.Name);
        Assert.Equal("Note", award.AwardableType);
        Assert.Equal(new Uri("https://gitlab.example/-/custom_emoji/party_parrot.gif"), award.Url);
    }

    [Fact]
    public async Task GetForIssueNoteAsync_BuildsIssueNoteAwardRoute_AndDeserializesReaction()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NoteAwardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.GetForIssueNoteAsync(1, 80, 1, 2,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/80/notes/1/award_emoji/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("party_parrot", award.Name);
        Assert.Equal(1, award.AwardableId);
    }

    [Fact]
    public async Task AddToIssueNoteAsync_PostsToIssueNoteRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(NoteAwardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.AddToIssueNoteAsync(1, 80, 1,
            new CreateAwardEmojiRequest { Name = "party_parrot" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/issues/80/notes/1/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"party_parrot"}""", sentBody);
        Assert.Equal("party_parrot", award.Name);
    }

    [Fact]
    public async Task DeleteFromIssueNoteAsync_SendsDeleteToIssueNoteAwardRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await repository.DeleteFromIssueNoteAsync("gitlab-org/gitlab", 80, 1, 2,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/issues/80/notes/1/award_emoji/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForMergeRequestAsync_BuildsMergeRequestRoute_AndDeserializesReactions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AwardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        List<GitLabAwardEmoji> awards = new();
        await foreach (GitLabAwardEmoji item in repository.ListForMergeRequestAsync(1, 105,
                           TestContext.Current.CancellationToken))
        {
            awards.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/105/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("thumbsup", Assert.Single(awards).Name);
    }

    [Fact]
    public async Task GetForMergeRequestAsync_BuildsMergeRequestAwardRoute_AndDeserializesReaction()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(AwardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.GetForMergeRequestAsync(1, 105, 344,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/105/award_emoji/344",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(344, award.Id);
        Assert.Equal("thumbsup", award.Name);
    }

    [Fact]
    public async Task AddToMergeRequestAsync_PostsToMergeRequestRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(AwardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.AddToMergeRequestAsync(1, 105,
            new CreateAwardEmojiRequest { Name = "thumbsup" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/105/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"thumbsup"}""", sentBody);
        Assert.Equal("thumbsup", award.Name);
    }

    [Fact]
    public async Task DeleteFromMergeRequestAsync_SendsDeleteToMergeRequestAwardRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await repository.DeleteFromMergeRequestAsync(1, 105, 344, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/105/award_emoji/344",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForMergeRequestNoteAsync_BuildsMergeRequestNoteRoute_AndDeserializesReactions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NoteAwardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        List<GitLabAwardEmoji> awards = new();
        await foreach (GitLabAwardEmoji item in repository.ListForMergeRequestNoteAsync(1, 105, 35,
                           TestContext.Current.CancellationToken))
        {
            awards.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/105/notes/35/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabAwardEmoji award = Assert.Single(awards);
        Assert.Equal("party_parrot", award.Name);
        Assert.Equal("Note", award.AwardableType);
    }

    [Fact]
    public async Task GetForMergeRequestNoteAsync_BuildsMergeRequestNoteAwardRoute_AndDeserializesReaction()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NoteAwardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.GetForMergeRequestNoteAsync(1, 105, 35, 2,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/105/notes/35/award_emoji/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("party_parrot", award.Name);
        Assert.Equal(new Uri("https://gitlab.example/-/custom_emoji/party_parrot.gif"), award.Url);
    }

    [Fact]
    public async Task AddToMergeRequestNoteAsync_PostsToMergeRequestNoteRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(NoteAwardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.AddToMergeRequestNoteAsync(1, 105, 35,
            new CreateAwardEmojiRequest { Name = "party_parrot" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/105/notes/35/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"party_parrot"}""", sentBody);
        Assert.Equal(1, award.Id);
    }

    [Fact]
    public async Task DeleteFromMergeRequestNoteAsync_SendsDeleteToMergeRequestNoteAwardRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await repository.DeleteFromMergeRequestNoteAsync(1, 105, 35, 2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/105/notes/35/award_emoji/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForIssueAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Award Emoji Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetForIssueAsync(1, 80, 999, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Award Emoji Not Found", exception.Message);
    }

    [Fact]
    public async Task DeleteFromIssueAsync_WhenNotTheAuthor_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.DeleteFromIssueAsync(1, 80, 344, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task ListForSnippetAsync_BuildsSnippetRoute_AndDeserializesReactions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SnippetAwardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        List<GitLabAwardEmoji> awards = new();
        await foreach (GitLabAwardEmoji item in repository.ListForSnippetAsync(1, 11,
                           TestContext.Current.CancellationToken))
        {
            awards.Add(item);
        }

        // Snippets are addressed by id, not by iid - the only awardable in this tag that is.
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/snippets/11/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabAwardEmoji award = Assert.Single(awards);
        Assert.Equal(500, award.Id);
        Assert.Equal("tada", award.Name);
        Assert.Equal(11, award.AwardableId);
        Assert.Equal("Snippet", award.AwardableType);
        Assert.Equal("root", award.User?.Username);
    }

    [Fact]
    public async Task GetForSnippetAsync_BuildsSnippetAwardRoute_AndDeserializesReaction()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SnippetAwardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.GetForSnippetAsync(1, 11, 500,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/snippets/11/award_emoji/500",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(500, award.Id);
        Assert.Equal("tada", award.Name);
    }

    [Fact]
    public async Task AddToSnippetAsync_PostsToSnippetRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SnippetAwardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.AddToSnippetAsync(1, 11,
            new CreateAwardEmojiRequest { Name = "tada" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/snippets/11/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"tada"}""", sentBody);
        Assert.Equal("tada", award.Name);
    }

    [Fact]
    public async Task DeleteFromSnippetAsync_SendsDeleteToSnippetAwardRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await repository.DeleteFromSnippetAsync(1, 11, 500, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/snippets/11/award_emoji/500",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForSnippetNoteAsync_BuildsSnippetNoteRoute_AndDeserializesReactions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NoteAwardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        List<GitLabAwardEmoji> awards = new();
        await foreach (GitLabAwardEmoji item in repository.ListForSnippetNoteAsync(1, 11, 3,
                           TestContext.Current.CancellationToken))
        {
            awards.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/1/snippets/11/notes/3/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Note", Assert.Single(awards).AwardableType);
    }

    [Fact]
    public async Task GetForSnippetNoteAsync_BuildsSnippetNoteAwardRoute_AndDeserializesReaction()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NoteAwardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.GetForSnippetNoteAsync(1, 11, 3, 2,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/1/snippets/11/notes/3/award_emoji/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("party_parrot", award.Name);
    }

    [Fact]
    public async Task AddToSnippetNoteAsync_PostsToSnippetNoteRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(NoteAwardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.AddToSnippetNoteAsync(1, 11, 3,
            new CreateAwardEmojiRequest { Name = "party_parrot" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/1/snippets/11/notes/3/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"party_parrot"}""", sentBody);
        Assert.Equal("party_parrot", award.Name);
    }

    [Fact]
    public async Task DeleteFromSnippetNoteAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await repository.DeleteFromSnippetNoteAsync("gitlab-org/gitlab", 11, 3, 2,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/snippets/11/notes/3/award_emoji/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForEpicAsync_BuildsGroupEpicRoute_AndDeserializesReactions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(EpicAwardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        List<GitLabAwardEmoji> awards = new();
        await foreach (GitLabAwardEmoji item in repository.ListForEpicAsync(5, 42,
                           TestContext.Current.CancellationToken))
        {
            awards.Add(item);
        }

        // Epics are the one awardable rooted at a GROUP rather than a project.
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/42/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabAwardEmoji award = Assert.Single(awards);
        Assert.Equal(700, award.Id);
        Assert.Equal("rocket", award.Name);
        Assert.Equal("Epic", award.AwardableType);
    }

    [Fact]
    public async Task ListForEpicAsync_EncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await foreach (GitLabAwardEmoji _ in repository.ListForEpicAsync("gitlab-org/subgroup", 42,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/epics/42/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetForEpicAsync_BuildsEpicAwardRoute_AndDeserializesReaction()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(EpicAwardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.GetForEpicAsync(5, 42, 700,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/42/award_emoji/700",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(700, award.Id);
        Assert.Equal("rocket", award.Name);
    }

    [Fact]
    public async Task AddToEpicAsync_PostsToEpicRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(EpicAwardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.AddToEpicAsync(5, 42,
            new CreateAwardEmojiRequest { Name = "rocket" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/42/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"rocket"}""", sentBody);
        Assert.Equal("rocket", award.Name);
    }

    [Fact]
    public async Task DeleteFromEpicAsync_SendsDeleteToEpicAwardRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await repository.DeleteFromEpicAsync(5, 42, 700, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/42/award_emoji/700",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForEpicNoteAsync_BuildsEpicNoteRoute_AndDeserializesReactions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NoteAwardListJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        List<GitLabAwardEmoji> awards = new();
        await foreach (GitLabAwardEmoji item in repository.ListForEpicNoteAsync(5, 42, 9,
                           TestContext.Current.CancellationToken))
        {
            awards.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/42/notes/9/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Note", Assert.Single(awards).AwardableType);
    }

    [Fact]
    public async Task GetForEpicNoteAsync_BuildsEpicNoteAwardRoute_AndDeserializesReaction()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(NoteAwardJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.GetForEpicNoteAsync(5, 42, 9, 2,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/42/notes/9/award_emoji/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("party_parrot", award.Name);
    }

    [Fact]
    public async Task AddToEpicNoteAsync_PostsToEpicNoteRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(NoteAwardJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        GitLabAwardEmoji award = await repository.AddToEpicNoteAsync(5, 42, 9,
            new CreateAwardEmojiRequest { Name = "party_parrot" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/5/epics/42/notes/9/award_emoji",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"party_parrot"}""", sentBody);
        Assert.Equal(1, award.Id);
    }

    [Fact]
    public async Task DeleteFromEpicNoteAsync_EncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        AwardEmojiClient repository = new(connection);

        await repository.DeleteFromEpicNoteAsync("gitlab-org/subgroup", 42, 9, 2,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/epics/42/notes/9/award_emoji/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}