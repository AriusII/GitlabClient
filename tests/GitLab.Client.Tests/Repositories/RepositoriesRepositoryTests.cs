using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class RepositoriesRepositoryTests
{
    private static readonly string[] TwoRefs = ["main", "feature/login"];

    private static readonly string[] OneRef = ["main"];

    private static readonly string[] ExcludedArchivePaths = ["vendor", "node_modules"];

    [Fact]
    public async Task ListTreeAsync_BuildsTreeRoute_WithQueryOptions_AndDeserializesEntries()
    {
        const string Json = """
                            [
                              {
                                "id": "a1b2c3d4e5f60718293a4b5c6d7e8f9012345678",
                                "name": "README.md",
                                "type": "blob",
                                "path": "doc/README.md",
                                "mode": "100644",
                                "last_commit": {
                                  "id": "2695effb5807a22ff3d138d593fd856244e155e7",
                                  "short_id": "2695effb",
                                  "title": "Document the tree endpoint",
                                  "author_name": "Ada Lovelace",
                                  "committed_date": "2024-03-04T05:06:07.000Z",
                                  "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/2695effb5807a22ff3d138d593fd856244e155e7"
                                }
                              },
                              {
                                "id": "b2c3d4e5f60718293a4b5c6d7e8f901234567890",
                                "name": "api",
                                "type": "tree",
                                "path": "doc/api",
                                "mode": "040000"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        TreeListOptions options = new()
        {
            Ref = "release/1.0",
            Path = "doc",
            Recursive = false,
            WithLastCommit = true,
            PerPage = 20
        };

        List<GitLabTreeItem> entries = new();
        await foreach (GitLabTreeItem entry in repository.ListTreeAsync(1, options,
                           TestContext.Current.CancellationToken))
        {
            entries.Add(entry);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/repository/tree", requestUri);
        Assert.Contains("ref=release%2F1.0", requestUri);
        Assert.Contains("path=doc", requestUri);
        Assert.Contains("recursive=false", requestUri);
        Assert.Contains("with_last_commit=true", requestUri);
        Assert.Contains("per_page=20", requestUri);

        Assert.Equal(2, entries.Count);
        Assert.Equal("a1b2c3d4e5f60718293a4b5c6d7e8f9012345678", entries[0].Id);
        Assert.Equal("README.md", entries[0].Name);
        Assert.Equal("blob", entries[0].Type);
        Assert.Equal("doc/README.md", entries[0].Path);
        Assert.Equal("100644", entries[0].Mode);
        Assert.NotNull(entries[0].LastCommit);
        Assert.Equal("2695effb", entries[0].LastCommit!.ShortId);
        Assert.Equal("Ada Lovelace", entries[0].LastCommit!.AuthorName);
        Assert.Equal("tree", entries[1].Type);
        Assert.Null(entries[1].LastCommit);
    }

    [Fact]
    public async Task ListTreeAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        await foreach (GitLabTreeItem _ in repository.ListTreeAsync("gitlab-org/gitlab", null,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/tree",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CompareAsync_EscapesRefsInQuery_AndDeserializesCommitsAndDiffs()
    {
        const string Json = """
                            {
                              "commit": {
                                "id": "12d65c8dd2b2676fa3ac47d955accc085a37a9c1",
                                "short_id": "12d65c8d",
                                "title": "Merge feature/login",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/12d65c8dd2b2676fa3ac47d955accc085a37a9c1"
                              },
                              "commits": [
                                {
                                  "id": "12d65c8dd2b2676fa3ac47d955accc085a37a9c1",
                                  "short_id": "12d65c8d",
                                  "title": "Merge feature/login",
                                  "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/12d65c8dd2b2676fa3ac47d955accc085a37a9c1"
                                }
                              ],
                              "diffs": [
                                {
                                  "diff": "@@ -71,6 +71,8 @@",
                                  "new_path": "doc/update/5.4-to-6.0.md",
                                  "old_path": "doc/update/5.4-to-6.0.md",
                                  "a_mode": "100755",
                                  "b_mode": "100644",
                                  "new_file": false,
                                  "renamed_file": false,
                                  "deleted_file": false,
                                  "generated_file": false,
                                  "collapsed": false,
                                  "too_large": false
                                }
                              ],
                              "compare_timeout": false,
                              "compare_same_ref": false,
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/compare/main...feature/login"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        GitLabCompare compare = await repository.CompareAsync("gitlab-org/gitlab", "main", "feature/login", 7, true,
            false, TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/gitlab-org%2Fgitlab/repository/compare", requestUri);
        Assert.Contains("from=main", requestUri);
        Assert.Contains("to=feature%2Flogin", requestUri);
        Assert.Contains("from_project_id=7", requestUri);
        Assert.Contains("straight=true", requestUri);
        Assert.Contains("unidiff=false", requestUri);

        Assert.NotNull(compare.Commit);
        Assert.Equal("12d65c8d", compare.Commit!.ShortId);
        Assert.Equal("12d65c8dd2b2676fa3ac47d955accc085a37a9c1", Assert.Single(compare.Commits!).Id);
        GitLabDiff diff = Assert.Single(compare.Diffs!);
        Assert.Equal("100755", diff.AMode);
        Assert.Equal("100644", diff.BMode);
        Assert.Equal("doc/update/5.4-to-6.0.md", diff.NewPath);
        Assert.False(diff.RenamedFile);
        Assert.False(compare.CompareTimeout);
        Assert.False(compare.CompareSameRef);
        Assert.Equal("https://gitlab.example/gitlab-org/gitlab/-/compare/main...feature/login",
            compare.WebUrl?.AbsoluteUri);
    }

    [Fact]
    public async Task ListContributorsAsync_BuildsContributorsRoute_AndDeserializesMetrics()
    {
        const string Json = """
                            [
                              {
                                "name": "Ada Lovelace",
                                "email": "ada@example.com",
                                "commits": 117,
                                "additions": 900,
                                "deletions": 42
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        ContributorListOptions options = new()
        {
            Ref = "release/1.0", OrderBy = "commits", Sort = "desc", PerPage = 50
        };

        List<GitLabContributor> contributors = new();
        await foreach (GitLabContributor contributor in repository.ListContributorsAsync(1, options,
                           TestContext.Current.CancellationToken))
        {
            contributors.Add(contributor);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/1/repository/contributors", requestUri);
        Assert.Contains("ref=release%2F1.0", requestUri);
        Assert.Contains("order_by=commits", requestUri);
        Assert.Contains("sort=desc", requestUri);
        Assert.Contains("per_page=50", requestUri);

        GitLabContributor single = Assert.Single(contributors);
        Assert.Equal("Ada Lovelace", single.Name);
        Assert.Equal("ada@example.com", single.Email);
        Assert.Equal(117, single.Commits);
        Assert.Equal(900, single.Additions);
        Assert.Equal(42, single.Deletions);
    }

    [Fact]
    public async Task GetMergeBaseAsync_RepeatsEachRef_AndDeserializesTheCommonAncestor()
    {
        const string Json = """
                            {
                              "id": "1a0b36b3cdad1d2ee32457c102a8c0b7056fa863",
                              "short_id": "1a0b36b3",
                              "title": "Initial commit",
                              "author_name": "Ada Lovelace",
                              "committed_date": "2018-09-20T09:06:12.000+03:00",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/1a0b36b3cdad1d2ee32457c102a8c0b7056fa863"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        GitLabCommit mergeBase = await repository.GetMergeBaseAsync(1, TwoRefs, TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/1/repository/merge_base", requestUri);
        Assert.Contains("refs[]=main", requestUri);
        Assert.Contains("refs[]=feature%2Flogin", requestUri);

        Assert.Equal("1a0b36b3", mergeBase.ShortId);
        Assert.Equal("Initial commit", mergeBase.Title);
        Assert.Equal("Ada Lovelace", mergeBase.AuthorName);
    }

    [Fact]
    public async Task GetMergeBaseAsync_WithFewerThanTwoRefs_ThrowsBeforeAnyRequest()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        ArgumentOutOfRangeException exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            repository.GetMergeBaseAsync(1, OneRef, TestContext.Current.CancellationToken));

        Assert.Equal("refs", exception.ParamName);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task ListDiffStatsAsync_BuildsDiffStatsRoute_AndDeserializesLineCounts()
    {
        const string Json = """
                            [
                              {
                                "path": "doc/api/repositories.md",
                                "old_path": "doc/api/repository.md",
                                "additions": 12,
                                "deletions": 3
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        List<GitLabDiffStat> stats = new();
        await foreach (GitLabDiffStat stat in repository.ListDiffStatsAsync("gitlab-org/gitlab", "release/1.0", "main",
                           TestContext.Current.CancellationToken))
        {
            stats.Add(stat);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/gitlab-org%2Fgitlab/repository/diff_stats", requestUri);
        Assert.Contains("from=release%2F1.0", requestUri);
        Assert.Contains("to=main", requestUri);

        GitLabDiffStat single = Assert.Single(stats);
        Assert.Equal("doc/api/repositories.md", single.Path);
        Assert.Equal("doc/api/repository.md", single.OldPath);
        Assert.Equal(12, single.Additions);
        Assert.Equal(3, single.Deletions);
    }

    [Fact]
    public async Task ListChangedPathsAsync_SendsFindRenames_AndDeserializesStatuses()
    {
        const string Json = """
                            [
                              {
                                "path": "doc/api/repositories.md",
                                "status": "renamed",
                                "old_path": "doc/api/repository.md",
                                "old_mode": "100644",
                                "new_mode": "100755"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        List<GitLabChangedPath> paths = new();
        await foreach (GitLabChangedPath path in repository.ListChangedPathsAsync(1, "main", "feature/login", true,
                           TestContext.Current.CancellationToken))
        {
            paths.Add(path);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/1/repository/changed_paths", requestUri);
        Assert.Contains("from=main", requestUri);
        Assert.Contains("to=feature%2Flogin", requestUri);
        Assert.Contains("find_renames=true", requestUri);

        GitLabChangedPath single = Assert.Single(paths);
        Assert.Equal("doc/api/repositories.md", single.Path);
        Assert.Equal("renamed", single.Status);
        Assert.Equal("doc/api/repository.md", single.OldPath);
        Assert.Equal("100644", single.OldMode);
        Assert.Equal("100755", single.NewMode);
    }

    [Fact]
    public async Task GetDivergingCommitCountsAsync_SendsMaxCount_AndDeserializesCounts()
    {
        const string Json = """{ "behind": 3, "ahead": 5 }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        GitLabDivergingCommitCount counts = await repository.GetDivergingCommitCountsAsync(1, "main", "feature/login",
            100, TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/1/repository/diverging_commits", requestUri);
        Assert.Contains("from=main", requestUri);
        Assert.Contains("to=feature%2Flogin", requestUri);
        Assert.Contains("max_count=100", requestUri);

        Assert.Equal(3, counts.Behind);
        Assert.Equal(5, counts.Ahead);
    }

    [Fact]
    public async Task GenerateChangelogAsync_SendsEveryQueryParameter_AndDeserializesNotes()
    {
        const string Json = """{ "notes": "## 1.0.0 (2024-03-04)" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        GitLabChangelog changelog = await repository.GenerateChangelogAsync(1, "1.0.0", "v0.9.0", "release/1.0",
            new DateTimeOffset(2024, 3, 4, 5, 6, 7, TimeSpan.Zero), "Changelog", ".gitlab/changelog_config.yml",
            "main", TestContext.Current.CancellationToken);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Contains("/projects/1/repository/changelog", requestUri);
        Assert.Contains("version=1.0.0", requestUri);
        Assert.Contains("from=v0.9.0", requestUri);
        Assert.Contains("to=release%2F1.0", requestUri);
        Assert.Contains("date=2024-03-04T05:06:07Z", requestUri);
        Assert.Contains("trailer=Changelog", requestUri);
        Assert.Contains("config_file=.gitlab%2Fchangelog_config.yml", requestUri);
        Assert.Contains("config_file_ref=main", requestUri);

        Assert.Equal("## 1.0.0 (2024-03-04)", changelog.Notes);
    }

    [Fact]
    public async Task CompareAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Project Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.CompareAsync(404, "main", "missing", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Project Not Found", exception.Message);
    }

    [Fact]
    public async Task ListTreeAsync_OnForbidden_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabTreeItem _ in repository.ListTreeAsync(1, null, TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                Assert.Fail("The stub never returns a successful page.");
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task DownloadArchiveAsync_SendsEveryOption_AndSurfacesTheContentDispositionFileName()
    {
        byte[] archive = [0x1F, 0x8B, 0x08, 0x00];

        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new ByteArrayContent(archive) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/gzip");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "\"gitlab-main-abc123.tar.gz\""
            };
            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        RepositoryArchiveOptions options = new()
        {
            Sha = "main",
            RefType = GitLabArchiveRefType.Heads,
            Format = "tar.gz",
            Path = "doc",
            IncludeLfsBlobs = false,
            ExcludePaths = ExcludedArchivePaths
        };

        using GitLabFileResponse file =
            await repository.DownloadArchiveAsync(42, options, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.StartsWith("https://gitlab.example/api/v4/projects/42/repository/archive?", requestUri,
            StringComparison.Ordinal);
        Assert.Contains("sha=main", requestUri, StringComparison.Ordinal);
        Assert.Contains("ref_type=heads", requestUri, StringComparison.Ordinal);
        Assert.Contains("format=tar.gz", requestUri, StringComparison.Ordinal);
        Assert.Contains("path=doc", requestUri, StringComparison.Ordinal);
        Assert.Contains("include_lfs_blobs=false", requestUri, StringComparison.Ordinal);
        Assert.Contains("exclude_paths=vendor,node_modules", requestUri, StringComparison.Ordinal);

        Assert.Equal("application/gzip", file.ContentType);
        Assert.Equal("gitlab-main-abc123.tar.gz", file.FileName);
        Assert.Equal(archive.Length, file.ContentLength);
    }

    [Fact]
    public async Task DownloadArchiveAsync_WithoutOptions_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([0x00])
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadArchiveAsync(
            "gitlab-org/gitlab", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/archive",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(file.FileName);
    }

    [Fact]
    public async Task DownloadSnapshotAsync_BuildsTheProjectScopedSnapshotRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([0x00])
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        using GitLabFileResponse file =
            await repository.DownloadSnapshotAsync(42, true, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/snapshot?wiki=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(file.Content);
    }

    [Fact]
    public async Task GetBlobAsync_BuildsBlobRoute_AndDeserializesBase64Content()
    {
        const string Json = """
                            {
                              "size": 1476,
                              "encoding": "base64",
                              "content": "IyEvdXNyL2Jpbi9lbnYgcnVieQo=",
                              "sha": "2f2b3ecb3d2e0b1c1234567890abcdef12345678"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        GitLabBlob blob = await repository.GetBlobAsync(
            42, "2f2b3ecb3d2e0b1c1234567890abcdef12345678", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/blobs/2f2b3ecb3d2e0b1c1234567890abcdef12345678",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(1476, blob.Size);
        Assert.Equal("base64", blob.Encoding);
        Assert.Equal("IyEvdXNyL2Jpbi9lbnYgcnVieQo=", blob.Content);
    }

    [Fact]
    public async Task GetRawBlobAsync_StreamsTheBody_FromTheRawRoute()
    {
        const string Contents = "#!/usr/bin/env ruby\n";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Contents, Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        using GitLabFileResponse file =
            await repository.GetRawBlobAsync(42, "2f2b3ecb", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/blobs/2f2b3ecb/raw",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using StreamReader reader = new(file.Content, Encoding.UTF8);
        Assert.Equal(Contents, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetBlobsAsync_PostsTheFileList_AndMaterializesTheAnswer()
    {
        const string Json = """
                            [
                              {
                                "path": "README.md",
                                "ref": "main",
                                "size": 25,
                                "truncated": false,
                                "encoding": "base64",
                                "content": "IyBSRUFETUUK"
                              },
                              {
                                "path": "src/App.cs",
                                "ref": "main",
                                "size": 2097152,
                                "truncated": true,
                                "encoding": "base64",
                                "content": "cHVibGlj"
                              }
                            ]
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
        RepositoriesRepository repository = new(connection);

        BlobBatchRequest request = new()
        {
            Files =
            [
                new BlobBatchFile { Path = "README.md", Ref = "main" },
                new BlobBatchFile { Path = "src/App.cs", Ref = "main" }
            ]
        };

        IReadOnlyList<GitLabBatchBlob> blobs =
            await repository.GetBlobsAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/blobs/batch",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"files\":[", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"path\":\"src/App.cs\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"ref\":\"main\"", sentBody, StringComparison.Ordinal);

        Assert.Equal(2, blobs.Count);
        Assert.False(blobs[0].Truncated);
        Assert.True(blobs[1].Truncated);
        Assert.Equal(2097152, blobs[1].Size);
    }

    [Fact]
    public async Task AddChangelogAsync_PostsTheRequest_AndAcceptsAnEmptyBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        AddChangelogRequest request = new()
        {
            Version = "1.0.0",
            Branch = "main",
            File = "CHANGELOG.md",
            Trailer = "Changelog",
            Date = new DateTimeOffset(2024, 3, 4, 5, 6, 7, TimeSpan.Zero)
        };

        await repository.AddChangelogAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/changelog",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"version\":\"1.0.0\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"branch\":\"main\"", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("\"from\"", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetHealthAsync_SendsGenerate_AndDeserializesTheNestedStatistics()
    {
        const string Json = """
                            {
                              "size": 42000,
                              "references": { "loose_count": 3, "packed_size": 1024, "reference_backend": "files" },
                              "objects": {
                                "size": 39000,
                                "recent_size": 1000,
                                "stale_size": 38000,
                                "keep_size": 0,
                                "packfile_count": 2,
                                "reverse_index_count": 1,
                                "cruft_count": 0,
                                "keep_count": 0,
                                "loose_objects_count": 7,
                                "stale_loose_objects_count": 1,
                                "loose_objects_garbage_count": 0
                              },
                              "commit_graph": {
                                "commit_graph_chain_length": 1,
                                "has_bloom_filters": true,
                                "has_generation_data": true,
                                "has_generation_data_overflow": false
                              },
                              "bitmap": { "has_hash_cache": true, "has_lookup_table": false, "version": 1 },
                              "multi_pack_index": { "packfile_count": 2, "version": 1 },
                              "multi_pack_index_bitmap": { "has_hash_cache": false, "has_lookup_table": true, "version": 1 },
                              "alternates": [],
                              "is_object_pool": false,
                              "last_full_repack": { "seconds": 1710000000, "nanos": 500 },
                              "updated_at": "2024-03-04T05:06:07.000Z"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoriesRepository repository = new(connection);

        GitLabRepositoryHealth health =
            await repository.GetHealthAsync(42, true, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/health?generate=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(42000, health.Size);
        Assert.Equal("files", health.References?.ReferenceBackend);
        Assert.Equal(7, health.Objects?.LooseObjectsCount);
        Assert.True(health.CommitGraph?.HasBloomFilters);
        Assert.True(health.Bitmap?.HasHashCache);
        Assert.True(health.MultiPackIndexBitmap?.HasLookupTable);
        Assert.Equal(2, health.MultiPackIndex?.PackfileCount);
        Assert.False(health.IsObjectPool);
        Assert.Equal(1710000000, health.LastFullRepack?.Seconds);
        Assert.Equal(new DateTimeOffset(2024, 3, 4, 5, 6, 7, TimeSpan.Zero), health.UpdatedAt);
    }

    [Fact]
    public async Task UpdateSubmoduleAsync_EscapesTheSubmodulePath_AndPutsTheBody()
    {
        const string Json = """
                            {
                              "id": "ed899a2f4b50b4370feeea94676502b42383c746",
                              "short_id": "ed899a2f4b5",
                              "title": "Update submodule",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/ed899a2f4b50b4370feeea94676502b42383c746"
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
        RepositoriesRepository repository = new(connection);

        GitLabCommit commit = await repository.UpdateSubmoduleAsync(
            42,
            "lib/vendored/gitlab-ui",
            new UpdateSubmoduleRequest
            {
                CommitSha = "3ddec28ea23acc5bcb80bc5a1e5e8bcb1f8c0f4a",
                Branch = "main",
                CommitMessage = "Bump submodule"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/submodules/lib%2Fvendored%2Fgitlab-ui",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("\"commit_sha\":\"3ddec28ea23acc5bcb80bc5a1e5e8bcb1f8c0f4a\"", sentBody,
            StringComparison.Ordinal);
        Assert.Contains("\"commit_message\":\"Bump submodule\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("ed899a2f4b5", commit.ShortId);
    }
}