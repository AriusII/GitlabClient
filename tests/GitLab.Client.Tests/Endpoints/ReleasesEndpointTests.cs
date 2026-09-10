using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ReleasesEndpointTests
{
    [Fact]
    public async Task ListAsync_BuildsProjectScopedRoute_AndDeserializesReleases()
    {
        const string Json = """
                            [
                              {
                                "tag_name": "v1.0.0",
                                "name": "Awesome app v1.0.0",
                                "description": "First stable release.",
                                "created_at": "2024-05-01T10:00:00.000Z",
                                "released_at": "2024-05-01T10:05:00.000Z",
                                "author": {
                                  "id": 1,
                                  "username": "root",
                                  "name": "Administrator",
                                  "state": "active",
                                  "web_url": "https://gitlab.example/root"
                                }
                              },
                              {
                                "tag_name": "v0.9.0",
                                "name": "Beta release"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        List<GitLabRelease> releases = new();
        await foreach (GitLabRelease release in repository.ListAsync("gitlab-org/gitlab",
                           TestContext.Current.CancellationToken))
        {
            releases.Add(release);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/gitlab-org%2Fgitlab/releases", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, releases.Count);
        Assert.Equal("v1.0.0", releases[0].TagName);
        Assert.Equal("Awesome app v1.0.0", releases[0].Name);
        Assert.NotNull(releases[0].Author);
        Assert.Equal("root", releases[0].Author!.Username);
        Assert.Null(releases[0].WebUrl);
        Assert.Equal("v0.9.0", releases[1].TagName);
        Assert.Null(releases[1].Author);
    }

    [Fact]
    public async Task ListAsync_WithReleaseListOptions_ProjectsEveryGitLabFilterOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);
        ReleaseListOptions options = new()
        {
            OrderBy = "created_at",
            Sort = "asc",
            IncludeHtmlDescription = true,
            UpdatedBefore = new DateTimeOffset(2025, 2, 3, 4, 5, 6, TimeSpan.FromHours(2)),
            UpdatedAfter = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero),
            PerPage = 50
        };

        await foreach (GitLabRelease _ in repository.ListAsync(42, options, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/releases?order_by=created_at&sort=asc&include_html_description=true&updated_before=2025-02-03T02:05:06Z&updated_after=2025-01-02T03:04:05Z&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_EscapesSlashInTagName_AndDeserializesRelease()
    {
        const string Json = """
                            {
                              "tag_name": "release/1.0",
                              "name": "Release 1.0",
                              "description": "Notes.",
                              "created_at": "2024-06-01T00:00:00.000Z",
                              "released_at": "2024-06-01T00:00:00.000Z",
                              "author": {
                                "id": 7,
                                "username": "jdoe",
                                "name": "Jane Doe",
                                "web_url": "https://gitlab.example/jdoe"
                              },
                              "_links": {
                                "self": "https://gitlab.example/gitlab-org/gitlab/-/releases/release%2F1.0"
                              }
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        GitLabRelease release = await repository.GetAsync(1, "release/1.0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/1/releases/release%2F1.0", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("release/1.0", release.TagName);
        Assert.Equal("Release 1.0", release.Name);
        Assert.NotNull(release.Author);
        Assert.Equal("Jane Doe", release.Author!.Name);
        Assert.NotNull(release.WebUrl);
        Assert.Equal("https://gitlab.example/gitlab-org/gitlab/-/releases/release%2F1.0", release.WebUrl!.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_WithReleaseGetOptions_ProjectsRenderedDescriptionAndUsesGeneratedWireName()
    {
        const string Json = """
                            {
                              "tag_name": "v2.0.0",
                              "description_html": "<p>Release notes</p>"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);
        ReleaseGetOptions options = new() { IncludeHtmlDescription = true };

        GitLabRelease release = await repository.GetAsync(42, "v2.0.0", options, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/releases/v2.0.0?include_html_description=true",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("<p>Release notes</p>", release.DescriptionHtml);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Release Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(1, "missing-tag", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Release Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_PostsToReleasesRoute_WithSerializedBody_AndDeserializesCreatedRelease()
    {
        const string Json = """
                            {
                              "tag_name": "v2.0.0",
                              "name": "v2.0.0",
                              "description": "New major release.",
                              "created_at": "2024-07-01T00:00:00.000Z"
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
        ReleasesClient repository = new(connection);

        CreateReleaseRequest request = new() { TagName = "v2.0.0", Ref = "main", Description = "New major release." };

        GitLabRelease release = await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/releases",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("\"tag_name\":\"v2.0.0\"", sentBody, StringComparison.Ordinal);
        Assert.Contains("\"ref\":\"main\"", sentBody, StringComparison.Ordinal);
        Assert.Equal("v2.0.0", release.TagName);
        Assert.Equal("New major release.", release.Description);
    }

    [Fact]
    public async Task CreateAsync_PostsTagMessageMilestonesAndReleasedAtWhenSet()
    {
        const string Json = """{ "tag_name": "v2.0.0" }""";

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
        ReleasesClient repository = new(connection);

        CreateReleaseRequest request = new()
        {
            TagName = "v2.0.0",
            TagMessage = "Signed release",
            ReleasedAt = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero),
            Milestones = ["v2.0"]
        };

        await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        // Unset members (Ref, Description, MilestoneIds) are omitted rather than sent as null.
        Assert.Equal(
            """{"tag_name":"v2.0.0","tag_message":"Signed release","released_at":"2024-07-01T00:00:00+00:00","milestones":["v2.0"]}""",
            sentBody);
    }

    [Fact]
    public async Task CreateAsync_SerializesInitialAssetLinksAndTheCatalogCompatibilityFlag()
    {
        const string Json = """{ "tag_name": "v2.1.0" }""";

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
        ReleasesClient repository = new(connection);
        CreateReleaseRequest request = new()
        {
            TagName = "v2.1.0",
            LegacyCatalogPublish = true,
            Assets = new CreateReleaseAssetsRequest
            {
                Links =
                [
                    new CreateReleaseLinkRequest
                    {
                        Name = "linux-amd64",
                        Url = new Uri("https://downloads.example/linux-amd64"),
                        DirectAssetPath = "/binaries/linux-amd64",
                        LinkType = GitLabReleaseLinkType.Package
                    }
                ]
            }
        };

        await repository.CreateAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            """{"tag_name":"v2.1.0","assets":{"links":[{"name":"linux-amd64","url":"https://downloads.example/linux-amd64","direct_asset_path":"/binaries/linux-amd64","link_type":"package"}]},"legacy_catalog_publish":true}""",
            sentBody);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToEscapedTagRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        await repository.DeleteAsync(42, "release/1.0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/releases/release%2F1.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Release Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.DeleteAsync(1, "missing-tag", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Release Not Found", exception.Message);
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupRoute_AndProjectsTheListOptionsOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        GroupReleaseListOptions options = new() { Sort = "asc", Simple = true, PerPage = 25 };

        await foreach (GitLabRelease _ in repository.ListForGroupAsync("gitlab-org/subgroup", options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/groups/gitlab-org%2Fsubgroup/releases?", requestUri, StringComparison.Ordinal);
        Assert.Contains("sort=asc", requestUri, StringComparison.Ordinal);
        Assert.Contains("simple=true", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=25", requestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAsync_DeserializesTheNestedAssetsEvidencesCommitAndMilestones()
    {
        const string Json = """
                            {
                              "tag_name": "v1.0.0",
                              "name": "v1.0.0",
                              "description": "Notes.",
                              "description_html": "<p>Notes.</p>",
                              "upcoming_release": false,
                              "commit_path": "/gitlab-org/gitlab/-/commit/abcdef01",
                              "tag_path": "/gitlab-org/gitlab/-/tags/v1.0.0",
                              "commit": {
                                "id": "abcdef0123456789",
                                "short_id": "abcdef01",
                                "title": "Ship it",
                                "web_url": "https://gitlab.example/gitlab-org/gitlab/-/commit/abcdef01"
                              },
                              "milestones": [
                                {
                                  "id": 51,
                                  "iid": 1,
                                  "project_id": 12,
                                  "title": "v1.0",
                                  "state": "closed",
                                  "web_url": "https://gitlab.example/gitlab-org/gitlab/-/milestones/1"
                                }
                              ],
                              "assets": {
                                "count": 3,
                                "sources": [
                                  { "format": "zip", "url": "https://gitlab.example/archive/v1.0.0.zip" }
                                ],
                                "links": [
                                  {
                                    "id": 2,
                                    "name": "installer",
                                    "url": "https://downloads.example/app.msi",
                                    "direct_asset_url": "https://gitlab.example/-/releases/v1.0.0/downloads/app.msi",
                                    "link_type": "package",
                                    "external": true
                                  }
                                ]
                              },
                              "evidences": [
                                {
                                  "sha": "760d6cdfb0879c3ffedec13af470e0f71cf52c6c",
                                  "filepath": "https://gitlab.example/-/releases/v1.0.0/evidences/1.json",
                                  "collected_at": "2024-05-01T10:05:00.000Z"
                                }
                              ],
                              "_links": {
                                "self": "https://gitlab.example/gitlab-org/gitlab/-/releases/v1.0.0",
                                "edit_url": "https://gitlab.example/gitlab-org/gitlab/-/releases/v1.0.0/edit",
                                "closed_issues_url": "https://gitlab.example/closed-issues"
                              }
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        GitLabRelease release = await repository.GetAsync(1, "v1.0.0", TestContext.Current.CancellationToken);

        Assert.False(release.UpcomingRelease);
        Assert.Equal("<p>Notes.</p>", release.DescriptionHtml);
        Assert.Equal("/gitlab-org/gitlab/-/tags/v1.0.0", release.TagPath);
        Assert.Equal("abcdef01", release.Commit?.ShortId);
        GitLabMilestone milestone = Assert.Single(release.Milestones!);
        Assert.Equal("v1.0", milestone.Title);

        Assert.Equal(3, release.Assets?.Count);
        GitLabReleaseSource source = Assert.Single(release.Assets!.Sources!);
        Assert.Equal("zip", source.Format);
        GitLabReleaseLink link = Assert.Single(release.Assets.Links!);
        Assert.Equal(2, link.Id);
        Assert.Equal(GitLabReleaseLinkType.Package, link.LinkType);
        Assert.True(link.External);

        GitLabReleaseEvidence evidence = Assert.Single(release.Evidences!);
        Assert.Equal("760d6cdfb0879c3ffedec13af470e0f71cf52c6c", evidence.Sha);
        Assert.Equal(new DateTimeOffset(2024, 5, 1, 10, 5, 0, TimeSpan.Zero), evidence.CollectedAt);

        Assert.Equal("https://gitlab.example/gitlab-org/gitlab/-/releases/v1.0.0/edit",
            release.Links?.EditUrl?.AbsoluteUri);
        Assert.Equal("https://gitlab.example/closed-issues", release.Links?.ClosedIssuesUrl?.AbsoluteUri);
    }

    [Fact]
    public async Task UpdateAsync_PutsToEscapedTagRoute_WithOnlyTheSpecifiedFields()
    {
        const string Json = """{ "tag_name": "v1.0/rc1", "name": "Release candidate 1" }""";

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
        ReleasesClient repository = new(connection);

        UpdateReleaseRequest request = new() { Name = "Release candidate 1", Milestones = ["v1.0"] };

        GitLabRelease release =
            await repository.UpdateAsync(42, "v1.0/rc1", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/releases/v1.0%2Frc1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // Unset members are omitted rather than sent as null, so a partial update cannot clear the description.
        Assert.Equal("""{"name":"Release candidate 1","milestones":["v1.0"]}""", sentBody);
        Assert.Equal("Release candidate 1", release.Name);
    }

    [Fact]
    public async Task UpdateAsync_PostsReleasedAtAndMilestoneIds_MutuallyExclusiveWithMilestones()
    {
        const string Json = """{ "tag_name": "v1.0/rc1" }""";

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
        ReleasesClient repository = new(connection);

        UpdateReleaseRequest request = new()
        {
            ReleasedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero), MilestoneIds = [51, 52]
        };

        await repository.UpdateAsync(42, "v1.0/rc1", request, TestContext.Current.CancellationToken);

        Assert.Equal(
            """{"released_at":"2024-06-01T00:00:00+00:00","milestone_ids":[51,52]}""",
            sentBody);
    }

    [Fact]
    public async Task GenerateEvidenceAsync_PostsToTheEvidenceRoute_WithNoBody()
    {
        const string Json = """{ "tag_name": "v1.0/rc1" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        GitLabRelease release =
            await repository.GenerateEvidenceAsync(42, "v1.0/rc1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/releases/v1.0%2Frc1/evidence",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal("v1.0/rc1", release.TagName);
    }

    /// <summary>
    ///     The classic failure in this resource: a release tag is caller-supplied free text carrying both
    ///     '/' and '.', so it must be percent-encoded into a single path segment rather than appended
    ///     verbatim - an unescaped "v1.0/rc1" reads as a 404 on a route that does not exist.
    /// </summary>
    [Fact]
    public async Task ListLinksAsync_EscapesTheSlashBearingTagName_AndDeserializesEachLink()
    {
        const string Json = """
                            [
                              {
                                "id": 2,
                                "name": "awesome-v0.2.msi",
                                "url": "https://downloads.example/msi",
                                "direct_asset_url": "https://gitlab.example/-/releases/v1.0/rc1/downloads/msi",
                                "link_type": "package"
                              },
                              {
                                "id": 1,
                                "name": "runbook",
                                "url": "https://downloads.example/runbook",
                                "link_type": "runbook"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        List<GitLabReleaseLink> links = new();
        await foreach (GitLabReleaseLink link in repository.ListLinksAsync("gitlab-org/gitlab", "v1.0/rc1",
                           new ReleaseLinkListOptions { PerPage = 50 }, TestContext.Current.CancellationToken))
        {
            links.Add(link);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/releases/v1.0%2Frc1/assets/links?per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(2, links.Count);
        Assert.Equal("awesome-v0.2.msi", links[0].Name);
        Assert.Equal(GitLabReleaseLinkType.Package, links[0].LinkType);
        Assert.Equal(GitLabReleaseLinkType.Runbook, links[1].LinkType);
        Assert.Null(links[1].DirectAssetUrl);
    }

    [Fact]
    public async Task GetLinkAsync_BuildsTheLinkRoute_AndDeserializesTheLink()
    {
        const string Json = """
                            {
                              "id": 2,
                              "name": "installer",
                              "url": "https://downloads.example/app.msi",
                              "link_type": "image"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        GitLabReleaseLink link =
            await repository.GetLinkAsync(42, "v1.0/rc1", 2, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/releases/v1.0%2Frc1/assets/links/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(GitLabReleaseLinkType.Image, link.LinkType);
        Assert.Equal("https://downloads.example/app.msi", link.Url?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateLinkAsync_PostsSerializedBody_WithTheLinkTypeAsItsWireString()
    {
        const string Json = """
                            {
                              "id": 3,
                              "name": "installer",
                              "url": "https://downloads.example/app.msi",
                              "link_type": "package"
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
        ReleasesClient repository = new(connection);

        CreateReleaseLinkRequest request = new()
        {
            Name = "installer",
            Url = new Uri("https://downloads.example/app.msi"),
            DirectAssetPath = "/binaries/app.msi",
            LinkType = GitLabReleaseLinkType.Package
        };

        GitLabReleaseLink link =
            await repository.CreateLinkAsync(42, "v1.0/rc1", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/releases/v1.0%2Frc1/assets/links",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """{"name":"installer","url":"https://downloads.example/app.msi","direct_asset_path":"/binaries/app.msi","link_type":"package"}""",
            sentBody);
        Assert.Equal(3, link.Id);
    }

    [Fact]
    public async Task UpdateLinkAsync_PutsToTheLinkRoute_WithOnlyTheSpecifiedFields()
    {
        const string Json = """
                            {
                              "id": 2,
                              "name": "renamed",
                              "url": "https://downloads.example/app.msi",
                              "link_type": "other"
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
        ReleasesClient repository = new(connection);

        UpdateReleaseLinkRequest request = new() { Name = "renamed" };

        GitLabReleaseLink link =
            await repository.UpdateLinkAsync(42, "v1.0/rc1", 2, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/releases/v1.0%2Frc1/assets/links/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"name":"renamed"}""", sentBody);
        Assert.Equal(GitLabReleaseLinkType.Other, link.LinkType);
    }

    [Fact]
    public async Task DeleteLinkAsync_SendsDeleteToTheLinkRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "id": 2, "name": "installer" }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        await repository.DeleteLinkAsync(42, "v1.0/rc1", 2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/releases/v1.0%2Frc1/assets/links/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetLatestReleaseAsync_BuildsThePermalinkRoute_AndStreamsTheBody()
    {
        const string Json = """{"tag_name":"v2.0.0","name":"Latest"}""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        using GitLabFileResponse file =
            await repository.GetLatestReleaseAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/releases/permalink/latest",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using StreamReader reader = new(file.Content, Encoding.UTF8);
        Assert.Equal(Json, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLatestReleaseSuffixPathAsync_EscapesAndAppendsTheSuffix()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([0x00])
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        using GitLabFileResponse file = await repository.GetLatestReleaseSuffixPathAsync(
            "gitlab-org/gitlab", "downloads/binary.zip", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/releases/permalink/latest/downloads%2Fbinary.zip",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(file.Content);
    }

    [Fact]
    public async Task DownloadReleaseAssetAsync_EscapesTagNameAndAssetPath_AndStreamsTheBody()
    {
        const string Contents = "binary-content";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Contents, Encoding.UTF8, "application/octet-stream")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        ReleasesClient repository = new(connection);

        using GitLabFileResponse file = await repository.DownloadReleaseAssetAsync(
            42, "v1.0/rc1", "bin/app.exe", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/releases/v1.0%2Frc1/downloads/bin%2Fapp.exe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        using StreamReader reader = new(file.Content, Encoding.UTF8);
        Assert.Equal(Contents, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }
}