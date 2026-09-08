using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class CommitsRepositoryTests
{
    [Fact]
    public async Task GetAsync_BuildsExpectedRoute_AndDeserializesCommit()
    {
        const string Json = """
                            {
                              "id": "6104942438c14ec7bd21c6cd5bd995272b3faff6",
                              "short_id": "6104942438c",
                              "title": "Sanitize for network graph",
                              "author_name": "randx",
                              "web_url": "https://gitlab.example.com/janedoe/gitlab-foss/-/commit/6104942438c14ec7bd21c6cd5bd995272b3faff6"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        GitLabCommit commit = await repository.GetAsync(42, "6104942438c14ec7bd21c6cd5bd995272b3faff6",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/6104942438c14ec7bd21c6cd5bd995272b3faff6",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("6104942438c14ec7bd21c6cd5bd995272b3faff6", commit.Id);
        Assert.Equal("6104942438c", commit.ShortId);
        Assert.Equal("Sanitize for network graph", commit.Title);
    }

    [Fact]
    public async Task GetAsync_EscapesSlashInRefName_AndDeserializesCommit()
    {
        const string Json = """
                            {
                              "id": "6104942438c14ec7bd21c6cd5bd995272b3faff6",
                              "short_id": "6104942438c",
                              "title": "Sanitize for network graph",
                              "web_url": "https://gitlab.example.com/janedoe/gitlab-foss/-/commit/6104942438c14ec7bd21c6cd5bd995272b3faff6"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        await repository.GetAsync(42, "release/1.0", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/release%2F1.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_WithStatsFalse_AddsStatsQueryParameter()
    {
        const string Json = """
                            {
                              "id": "6104942438c14ec7bd21c6cd5bd995272b3faff6",
                              "short_id": "6104942438c",
                              "title": "Sanitize for network graph",
                              "web_url": "https://gitlab.example.com/janedoe/gitlab-foss/-/commit/6104942438c14ec7bd21c6cd5bd995272b3faff6"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        await repository.GetAsync(42, "6104942438c14ec7bd21c6cd5bd995272b3faff6", false,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/"
            + "6104942438c14ec7bd21c6cd5bd995272b3faff6?stats=false",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Commit Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(42, "deadbeef", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Commit Not Found", exception.Message);
    }

    [Fact]
    public async Task ListAsync_BuildsExpectedRouteWithQuery_AndDeserializesCommits()
    {
        const string Json = """
                            [
                              { "id": "aaa111", "short_id": "aaa111", "title": "First commit", "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/aaa111" },
                              { "id": "bbb222", "short_id": "bbb222", "title": "Second commit", "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/bbb222" }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        List<GitLabCommit> commits = new();
        await foreach (GitLabCommit commit in repository.ListAsync(
                           7,
                           new CommitListOptions { RefName = "main", PerPage = 20 },
                           TestContext.Current.CancellationToken))
        {
            commits.Add(commit);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/7/repository/commits?ref_name=main&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, commits.Count);
        Assert.Equal("First commit", commits[0].Title);
        Assert.Equal("Second commit", commits[1].Title);
    }

    [Fact]
    public async Task CreateAsync_PostsActionsArray_WithEnumWireValues_AndDeserializesDetailedCommit()
    {
        const string Json = """
                            {
                              "id": "ed899a2f4b50b4370feeea94676502b42383c746",
                              "short_id": "ed899a2f4b5",
                              "title": "Feature added",
                              "message": "Feature added\n\nSigned-off-by: Dmitriy <dmitriy@example.com>\n",
                              "parent_ids": ["ae1d9fb46aa2b07ee9836d49862ec4e2c46fbbba"],
                              "created_at": "2016-09-20T09:26:24.000-07:00",
                              "trailers": { "Signed-off-by": "Dmitriy <dmitriy@example.com>" },
                              "extended_trailers": { "Signed-off-by": ["Dmitriy <dmitriy@example.com>"] },
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/ed899a2f4b50b4370feeea94676502b42383c746",
                              "stats": { "additions": 10, "deletions": 2, "total": 12 },
                              "status": "success",
                              "project_id": 42
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
        CommitsRepository repository = new(connection);

        CreateCommitRequest request = new()
        {
            Branch = "main",
            CommitMessage = "Feature added",
            Actions =
            [
                new CommitAction
                {
                    Action = GitLabCommitActionType.Create,
                    FilePath = "foo/bar",
                    Content = "c29tZSBjb250ZW50",
                    Encoding = GitLabCommitActionEncoding.Base64
                },
                new CommitAction
                {
                    Action = GitLabCommitActionType.Move, FilePath = "foo/bar2", PreviousPath = "foo/bar"
                }
            ]
        };

        GitLabCommit commit = await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/commits",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"branch\":\"main\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"commit_message\":\"Feature added\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"action\":\"create\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"encoding\":\"base64\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"action\":\"move\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"previous_path\":\"foo/bar\"", sentBody, StringComparison.Ordinal);

        Assert.Equal("success", commit.Status);
        Assert.Equal(42, commit.ProjectId);
        Assert.Equal(12, commit.Stats?.Total);
        Assert.Equal("ae1d9fb46aa2b07ee9836d49862ec4e2c46fbbba", Assert.Single(commit.ParentIds!));
        Assert.Equal("Dmitriy <dmitriy@example.com>", commit.Trailers?["Signed-off-by"]);
        Assert.Equal("Dmitriy <dmitriy@example.com>", Assert.Single(commit.ExtendedTrailers!["Signed-off-by"]));
    }

    [Fact]
    public async Task CherryPickAsync_EscapesSlashBearingRef_AndPostsBody()
    {
        const string Json = """
                            {
                              "id": "8b090c1b79a14f2bd9e8a738f717824ff53aebad",
                              "short_id": "8b090c1b",
                              "title": "Feature added",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/8b090c1b79a14f2bd9e8a738f717824ff53aebad"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        await repository.CherryPickAsync(
            42,
            "release/1.0",
            new CherryPickCommitRequest { Branch = "main", DryRun = true, Message = "Backport" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/commits/release%2F1.0/cherry_pick",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"branch\":\"main\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"dry_run\":true", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RevertAsync_BuildsRevertRoute_AndPostsBody()
    {
        const string Json = """
                            {
                              "id": "8b090c1b79a14f2bd9e8a738f717824ff53aebad",
                              "short_id": "8b090c1b",
                              "title": "Revert \"Feature added\"",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/8b090c1b79a14f2bd9e8a738f717824ff53aebad"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        GitLabCommit commit = await repository.RevertAsync(
            "gitlab-org/gitlab",
            "a738f717824ff53aebad8b090c1b79a14f2bd9e8",
            new RevertCommitRequest { Branch = "main" },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/commits/"
            + "a738f717824ff53aebad8b090c1b79a14f2bd9e8/revert",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"branch\":\"main\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("dry_run", sentBody, StringComparison.Ordinal);
        Assert.Equal("8b090c1b", commit.ShortId);
    }

    [Fact]
    public async Task ListCommentsAsync_BuildsCommentsRoute_AndDeserializesComments()
    {
        const string Json = """
                            [
                              {
                                "note": "this code is really nice",
                                "path": "files/ruby/popen.rb",
                                "line": 14,
                                "line_type": "new",
                                "created_at": "2016-01-19T09:44:55.000Z",
                                "author": {
                                  "id": 28,
                                  "username": "user1",
                                  "name": "User1",
                                  "web_url": "https://gitlab.example/user1"
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        List<GitLabCommitComment> comments = [];
        await foreach (GitLabCommitComment comment in repository.ListCommentsAsync(
                           42, "deadbeef", TestContext.Current.CancellationToken))
        {
            comments.Add(comment);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/commits/deadbeef/comments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabCommitComment single = Assert.Single(comments);
        Assert.Equal("this code is really nice", single.Note);
        Assert.Equal("files/ruby/popen.rb", single.Path);
        Assert.Equal(14, single.Line);
        Assert.Equal("new", single.LineType);
        Assert.Equal("user1", single.Author?.Username);
    }

    [Fact]
    public async Task CreateCommentAsync_SerializesLineTypeAsItsWireValue()
    {
        const string Json = """
                            {
                              "note": "nice",
                              "path": "files/ruby/popen.rb",
                              "line": 14,
                              "line_type": "new"
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
        CommitsRepository repository = new(connection);

        GitLabCommitComment comment = await repository.CreateCommentAsync(
            42,
            "deadbeef",
            new CreateCommitCommentRequest
            {
                Note = "nice", Path = "files/ruby/popen.rb", Line = 14, LineType = GitLabCommitLineType.New
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/commits/deadbeef/comments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"line_type\":\"new\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("nice", comment.Note);
    }

    [Fact]
    public async Task ListDiffsAsync_BuildsDiffRouteWithQuery_AndDeserializesDiffs()
    {
        const string Json = """
                            [
                              {
                                "diff": "@@ -71,6 +71,8 @@\n",
                                "new_path": "doc/update/5.4-to-6.0.md",
                                "old_path": "doc/update/5.4-to-6.0.md",
                                "a_mode": null,
                                "b_mode": "100644",
                                "new_file": false,
                                "renamed_file": false,
                                "deleted_file": false
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        List<GitLabDiff> diffs = [];
        await foreach (GitLabDiff diff in repository.ListDiffsAsync(
                           42,
                           "release/1.0",
                           new CommitDiffOptions { Unidiff = true, PerPage = 20 },
                           TestContext.Current.CancellationToken))
        {
            diffs.Add(diff);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/release%2F1.0/diff?unidiff=true&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("100644", Assert.Single(diffs).BMode);
    }

    [Fact]
    public async Task ListMergeRequestsAsync_BuildsMergeRequestsRouteWithState()
    {
        const string Json = """
                            [
                              {
                                "id": 45,
                                "iid": 1,
                                "title": "Add feature",
                                "state": "merged",
                                "source_branch": "feature",
                                "target_branch": "main",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/merge_requests/1"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        List<GitLabMergeRequest> mergeRequests = [];
        await foreach (GitLabMergeRequest mergeRequest in repository.ListMergeRequestsAsync(
                           42,
                           "deadbeef",
                           new CommitMergeRequestListOptions { State = "merged", PerPage = 20 },
                           TestContext.Current.CancellationToken))
        {
            mergeRequests.Add(mergeRequest);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/deadbeef/merge_requests"
            + "?state=merged&per_page=20",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1, Assert.Single(mergeRequests).Iid);
    }

    [Fact]
    public async Task ListRefsAsync_ProjectsTypeFilterAsItsWireValue()
    {
        const string Json = """[ { "type": "branch", "name": "main" } ]""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        List<GitLabCommitRef> refs = [];
        await foreach (GitLabCommitRef commitRef in repository.ListRefsAsync(
                           42,
                           "deadbeef",
                           new CommitRefListOptions { Type = GitLabCommitRefScope.Branch },
                           TestContext.Current.CancellationToken))
        {
            refs.Add(commitRef);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/commits/deadbeef/refs?type=branch",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("main", Assert.Single(refs).Name);
    }

    [Fact]
    public async Task GetSequenceAsync_BuildsSequenceRouteWithFirstParent()
    {
        const string Json = """{ "count": 632 }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        GitLabCommitSequence sequence =
            await repository.GetSequenceAsync(42, "deadbeef", true, TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/commits/deadbeef/sequence?first_parent=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(632, sequence.Count);
    }

    [Fact]
    public async Task GetSignatureAsync_DeserializesTheFlattenedGpgPayload()
    {
        const string Json = """
                            {
                              "signature_type": "PGP",
                              "verification_status": "verified",
                              "gpg_key_id": 1,
                              "gpg_key_primary_keyid": "8254AAB3FBD54AC9",
                              "gpg_key_user_name": "John Doe",
                              "gpg_key_user_email": "johndoe@example.com",
                              "gpg_key_subkey_id": null,
                              "commit_source": "gitaly"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        GitLabCommitSignature signature =
            await repository.GetSignatureAsync(42, "deadbeef", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/commits/deadbeef/signature",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("PGP", signature.SignatureType);
        Assert.Equal("verified", signature.VerificationStatus);
        Assert.Equal(1, signature.GpgKeyId);
        Assert.Equal("8254AAB3FBD54AC9", signature.GpgKeyPrimaryKeyid);
        Assert.Null(signature.GpgKeySubkeyId);
        Assert.Equal("gitaly", signature.CommitSource);
    }

    [Fact]
    public async Task GetSignatureAsync_OnUnsignedCommit_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 GPG Signature Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetSignatureAsync(42, "deadbeef", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetWebCommitsPublicKeyAsync_BuildsTheInstanceScopedRoute()
    {
        const string Json = """{ "public_key": "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIExample" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        CommitsRepository repository = new(connection);

        GitLabWebCommitsPublicKey key =
            await repository.GetWebCommitsPublicKeyAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/web_commits/public_key",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIExample", key.PublicKey);
    }
}