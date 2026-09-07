using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class GroupsRepositoryTests
{
    private const string GroupJson = """
                                     {
                                       "id": 9970,
                                       "name": "Subgroup",
                                       "path": "subgroup",
                                       "visibility": "public",
                                       "web_url": "https://gitlab.com/groups/parent-group/subgroup",
                                       "full_name": "Parent Group / Subgroup",
                                       "full_path": "parent-group/subgroup"
                                     }
                                     """;

    /// <summary>
    ///     The enum members are the point: each one must reach the wire as the string GitLab's schema
    ///     enumerates, and the unset members must be absent rather than sent as null.
    /// </summary>
    private const string ExpectedCreateBody =
        """{"name":"Subgroup","path":"subgroup","parent_id":42,"visibility":"internal","require_two_factor_authentication":true,"project_creation_level":"noone","subgroup_creation_level":"owner","enabled_git_access_protocol":"ssh","wiki_access_level":"private","duo_availability":"default_off"}""";

    private const string ExpectedUpdateBody =
        """{"description":"Renamed","shared_runners_setting":"disabled_and_unoverridable","unique_project_download_limit_allowlist":["release-bot"],"unique_project_download_limit_alertlist":[7,9]}""";

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetAsync_EncodesNamespacedGroupPath_AndDeserializesGroup()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabGroup group = await repository.GetAsync("parent-group/subgroup", TestContext.Current.CancellationToken);

        Assert.Contains("/groups/parent-group%2Fsubgroup", handler.LastRequest?.RequestUri?.AbsoluteUri,
            StringComparison.Ordinal);
        Assert.Equal(9970, group.Id);
        Assert.Equal("Subgroup", group.Name);
        Assert.Equal("subgroup", group.Path);
        Assert.Equal(GitLabVisibility.Public, group.Visibility);
        Assert.Equal("parent-group/subgroup", group.FullPath);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 Group Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync("missing-group", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Group Not Found", exception.Message);
    }

    [Fact]
    public async Task GetAsync_DeserializesTheSettingsAndNestedObjects_AddedForTheDepthPass()
    {
        const string Json = """
                            {
                              "id": 9970,
                              "name": "Twitter",
                              "path": "twitter",
                              "visibility": "public",
                              "web_url": "https://gitlab.example/groups/twitter",
                              "archived": true,
                              "marked_for_deletion_on": "2026-01-31",
                              "organization_id": 1,
                              "project_creation_level": "maintainer",
                              "subgroup_creation_level": "owner",
                              "shared_runners_setting": "disabled_and_overridable",
                              "wiki_access_level": "enabled",
                              "default_branch": "main",
                              "default_branch_protection": 2,
                              "default_branch_protection_defaults": {
                                "allowed_to_push": [{ "access_level": 40 }],
                                "allowed_to_merge": [{ "access_level": 30 }],
                                "allow_force_push": false,
                                "developer_can_initial_push": true,
                                "code_owner_approval_required": false
                              },
                              "lfs_enabled": true,
                              "emails_enabled": false,
                              "two_factor_grace_period": 48,
                              "runners_token": "ABCDEF1234567890",
                              "statistics": {
                                "storage_size": 212,
                                "repository_size": 33,
                                "wiki_size": 100,
                                "lfs_objects_size": 123,
                                "job_artifacts_size": 57,
                                "packages_size": 0,
                                "snippets_size": 50,
                                "uploads_size": 0
                              },
                              "shared_with_groups": [
                                {
                                  "group_id": 28,
                                  "group_name": "Ops",
                                  "group_full_path": "ops",
                                  "group_access_level": 30,
                                  "expires_at": "2026-12-31"
                                }
                              ]
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabGroup group = await repository.GetAsync(9970, TestContext.Current.CancellationToken);

        Assert.True(group.Archived);
        Assert.Equal(new DateOnly(2026, 1, 31), group.MarkedForDeletionOn);
        Assert.Equal(1, group.OrganizationId);
        Assert.Equal("maintainer", group.ProjectCreationLevel);
        Assert.Equal("disabled_and_overridable", group.SharedRunnersSetting);
        Assert.Equal(48, group.TwoFactorGracePeriod);
        Assert.Equal("ABCDEF1234567890", group.RunnersToken);
        Assert.Equal(212, group.Statistics?.StorageSize);
        Assert.Equal(40, group.DefaultBranchProtectionDefaults?.AllowedToPush?[0].AccessLevel);
        Assert.True(group.DefaultBranchProtectionDefaults?.DeveloperCanInitialPush);
        GitLabSharedGroupLink link = Assert.Single(group.SharedWithGroups ?? []);
        Assert.Equal(28, link.GroupId);
        Assert.Equal(new DateOnly(2026, 12, 31), link.ExpiresAt);
    }

    [Fact]
    public async Task CreateAsync_PostsToGroups_AndSendsOnlyTheMembersThatWereSet()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabGroup group = await repository.CreateAsync(
            new CreateGroupRequest
            {
                Name = "Subgroup",
                Path = "subgroup",
                ParentId = 42,
                Visibility = GitLabVisibility.Internal,
                ProjectCreationLevel = GitLabGroupProjectCreationLevel.NoOne,
                SubgroupCreationLevel = GitLabGroupSubgroupCreationLevel.Owner,
                WikiAccessLevel = GitLabGroupWikiAccessLevel.Private,
                EnabledGitAccessProtocol = GitLabGroupGitAccessProtocol.Ssh,
                DuoAvailability = GitLabGroupDuoAvailability.DefaultOff,
                RequireTwoFactorAuthentication = true
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal(ExpectedCreateBody, sentBody);
        Assert.Equal(9970, group.Id);
    }

    [Fact]
    public async Task UpdateAsync_PutsToTheEncodedGroupRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdateAsync(
            "parent/child",
            new UpdateGroupRequest
            {
                Description = "Renamed",
                SharedRunnersSetting = GitLabGroupSharedRunnersSetting.DisabledAndUnoverridable,
                UniqueProjectDownloadLimitAllowlist = ["release-bot"],
                UniqueProjectDownloadLimitAlertlist = [7, 9]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent%2Fchild",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(ExpectedUpdateBody, sentBody);
    }

    [Fact]
    public async Task SetAvatarAsync_PutsMultipart_UnderTheAvatarFieldNameGitLabExpects()
    {
        string? sentBody = null;
        string? contentType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            contentType = request.Content?.Headers.ContentType?.MediaType;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());

        // FieldName is left at its "file" default on purpose: the repository must override it, because
        // GitLab answers 200 and silently ignores an avatar part sent under any other name.
        await repository.SetAvatarAsync(
            9970,
            new GitLabFileUpload { Content = content, FileName = "logo.png", ContentType = "image/png" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", contentType);
        string body = (sentBody ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=avatar", body, StringComparison.Ordinal);
        Assert.DoesNotContain("name=file;", body, StringComparison.Ordinal);
        Assert.Contains("filename=logo.png", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteAndAcceptsThe202GitLabAnswersWith()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteAsync("parent/child", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent%2Fchild",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RestoreAsync_PostsToRestore_WithNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.RestoreAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/restore",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
    }

    [Theory]
    [InlineData("archive")]
    [InlineData("unarchive")]
    public async Task ArchiveAndUnarchive_PostToTheirOwnRoute_AndReturnTheUpdatedGroup(string action)
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabGroup group = action == "archive"
            ? await repository.ArchiveAsync(9970, TestContext.Current.CancellationToken)
            : await repository.UnarchiveAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal($"https://gitlab.example/api/v4/groups/9970/{action}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(9970, group.Id);
    }

    [Fact]
    public async Task ListSubgroupsAsync_EncodesTheNamespacedPath_AndProjectsEveryFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{GroupJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GroupHierarchyListOptions options = new()
        {
            Statistics = true,
            SkipGroups = [21, 22],
            AllAvailable = false,
            Visibility = GitLabVisibility.Private,
            Search = "ops",
            OrderBy = "path",
            Sort = "desc",
            MinAccessLevel = 30,
            MarkedForDeletionOn = new DateOnly(2026, 1, 31),
            PerPage = 50
        };

        List<GitLabGroup> groups = [];
        await foreach (GitLabGroup group in repository.ListSubgroupsAsync("parent/child", options,
                           TestContext.Current.CancellationToken))
        {
            groups.Add(group);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent%2Fchild/subgroups"
            + "?statistics=true&skip_groups=21,22&all_available=false&visibility=private&search=ops"
            + "&order_by=path&sort=desc&min_access_level=30&marked_for_deletion_on=2026-01-31&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Single(groups);
    }

    [Theory]
    [InlineData("descendant_groups")]
    [InlineData("invited_groups")]
    [InlineData("transfer_locations")]
    [InlineData("groups/shared")]
    public async Task TheGroupListingRoutes_AreBuiltFromLiteralSegments(string route)
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{GroupJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        IAsyncEnumerable<GitLabGroup> results = route switch
        {
            "descendant_groups" => repository.ListDescendantGroupsAsync(9970, null,
                TestContext.Current.CancellationToken),
            "invited_groups" => repository.ListInvitedGroupsAsync(9970, null, TestContext.Current.CancellationToken),
            "transfer_locations" => repository.ListTransferLocationsAsync(9970, null,
                TestContext.Current.CancellationToken),
            _ => repository.ListSharedGroupsAsync(9970, null, TestContext.Current.CancellationToken)
        };

        await foreach (GitLabGroup _ in results.ConfigureAwait(false))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal($"https://gitlab.example/api/v4/groups/9970/{route}",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListInvitedGroupsAsync_JoinsTheRelationFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabGroup _ in repository.ListInvitedGroupsAsync(
                           9970,
                           new InvitedGroupListOptions { Relation = ["direct", "inherited"], MinAccessLevel = 10 },
                           TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/invited_groups?relation=direct,inherited&min_access_level=10",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListProjectsAsync_BuildsTheProjectsRoute_AndDeserializesEachProject()
    {
        const string Json = """
                            [
                              {
                                "id": 4,
                                "name": "Diaspora Client",
                                "path_with_namespace": "diaspora/diaspora-client",
                                "visibility": "private",
                                "web_url": "https://gitlab.example/diaspora/diaspora-client",
                                "archived": false
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabProject> projects = [];
        await foreach (GitLabProject project in repository.ListProjectsAsync(
                           9970,
                           new GroupProjectListOptions { IncludeSubgroups = true, WithShared = false, Simple = true },
                           TestContext.Current.CancellationToken))
        {
            projects.Add(project);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/projects?simple=true&with_shared=false&include_subgroups=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(4, Assert.Single(projects).Id);
    }

    [Fact]
    public async Task ListSharedProjectsAsync_BuildsTheNestedSharedRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabProject _ in repository.ListSharedProjectsAsync(
                           9970,
                           new GroupSharedProjectListOptions { Starred = true },
                           TestContext.Current.CancellationToken))
        {
            // Draining the sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/projects/shared?starred=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task TransferProjectAsync_PostsTheProjectUnderTheGroup_WithNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabGroup group = await repository.TransferProjectAsync(9970, "gitlab-org/gitlab",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/projects/gitlab-org%2Fgitlab",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(9970, group.Id);
    }

    [Fact]
    public async Task RemoveSharedProjectAsync_DeletesFromSharedProjects()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.RemoveSharedProjectAsync(9970, 17, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/shared_projects/17",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ShareAsync_PostsTheShareBody_WithTheExpiryAsAPlainDate()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.ShareAsync(
            9970,
            new ShareGroupRequest { GroupId = 28, GroupAccess = 30, ExpiresAt = new DateOnly(2026, 12, 31) },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/share",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"group_id":28,"group_access":30,"expires_at":"2026-12-31"}""", sentBody);
    }

    [Fact]
    public async Task UnshareAsync_DeletesTheShareByTheInvitedGroupId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.UnshareAsync(9970, 28, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/share/28",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task TransferAsync_WithNoParent_PostsAnEmptyObject_WhichPromotesTheGroupToTopLevel()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.TransferAsync(9970, new TransferGroupRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/transfer",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
    }

    [Fact]
    public async Task TransferToOrganizationAsync_PostsTheOrganizationId()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.TransferToOrganizationAsync(
            9970,
            new TransferGroupToOrganizationRequest { OrganizationId = 3 },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/transfer_to_organization",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"organization_id":3}""", sentBody);
    }

    [Fact]
    public async Task ListIssuesAsync_SpellsTheNegatedFiltersTheWayGitLabDoes()
    {
        const string Json = """
                            [
                              {
                                "id": 41,
                                "iid": 1,
                                "project_id": 4,
                                "title": "Ut commodi ullam eos dolores perferendis nihil sunt.",
                                "state": "opened",
                                "web_url": "https://gitlab.example/group/project/-/issues/1"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabIssue> issues = [];
        await foreach (GitLabIssue issue in repository.ListIssuesAsync(
                           "parent/child",
                           new GroupIssueListOptions
                           {
                               State = "opened",
                               Labels = ["bug"],
                               NotLabels = ["wontfix"],
                               NotAuthorId = 5,
                               SearchIn = "title,description",
                               Search = "crash",
                               NonArchived = true
                           },
                           TestContext.Current.CancellationToken))
        {
            issues.Add(issue);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent%2Fchild/issues"
            + "?state=opened&labels=bug&not[labels]=wontfix&search=crash&in=title%2Cdescription"
            + "&not[author_id]=5&non_archived=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(41, Assert.Single(issues).Id);
    }

    [Fact]
    public async Task GetIssueStatisticsAsync_DeserializesGitLabsDoublyNestedCounts()
    {
        const string Json = """{ "statistics": { "counts": { "all": 20, "closed": 5, "opened": 15 } } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabGroupIssueStatistics statistics = await repository.GetIssueStatisticsAsync(
            9970,
            new GroupIssueStatisticsOptions { Confidential = false, MilestoneId = "Upcoming" },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/issues_statistics"
                     + "?milestone_id=Upcoming&confidential=false",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(20, statistics.Statistics?.Counts?.All);
        Assert.Equal(5, statistics.Statistics?.Counts?.Closed);
        Assert.Equal(15, statistics.Statistics?.Counts?.Opened);
    }

    [Fact]
    public async Task ListBillableMembersAsync_BuildsTheRoute_AndDeserializesTheBillingMetadata()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "username": "raymond_smith",
                                "name": "Raymond Smith",
                                "state": "active",
                                "avatar_url": "https://gitlab.example/uploads/-/system/user/avatar/1/avatar.png",
                                "web_url": "https://gitlab.example/raymond_smith",
                                "email": "raymond@example.com",
                                "last_activity_on": "2026-01-27",
                                "membership_type": "group_member",
                                "removable": true,
                                "created_at": "2026-01-03T12:00:00.000Z",
                                "is_last_owner": false,
                                "last_login_at": "2026-01-27T09:00:00.000Z"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabBillableMember> members = [];
        await foreach (GitLabBillableMember member in repository.ListBillableMembersAsync(
                           9970,
                           new GroupBillableMemberListOptions { Sort = "last_activity_on_desc", PerPage = 100 },
                           TestContext.Current.CancellationToken))
        {
            members.Add(member);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/billable_members?sort=last_activity_on_desc&per_page=100",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabBillableMember only = Assert.Single(members);
        Assert.Equal("raymond_smith", only.Username);
        Assert.Equal(new DateOnly(2026, 1, 27), only.LastActivityOn);
        Assert.Equal("group_member", only.MembershipType);
        Assert.True(only.Removable);
        Assert.False(only.IsLastOwner);
    }

    [Theory]
    [InlineData("provisioned_users")]
    [InlineData("saml_users")]
    public async Task TheGroupUserListings_ShareOneFilterSet(string route)
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "username": "raymond_smith",
                                "name": "Raymond Smith",
                                "web_url": "https://gitlab.example/raymond_smith"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GroupUserListOptions options = new() { Active = true, Search = "smith" };

        List<GitLabUser> users = [];
        IAsyncEnumerable<GitLabUser> results = route == "saml_users"
            ? repository.ListSamlUsersAsync(9970, options, TestContext.Current.CancellationToken)
            : repository.ListProvisionedUsersAsync(9970, options, TestContext.Current.CancellationToken);

        await foreach (GitLabUser user in results.ConfigureAwait(false))
        {
            users.Add(user);
        }

        Assert.Equal($"https://gitlab.example/api/v4/groups/9970/{route}?search=smith&active=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("raymond_smith", Assert.Single(users).Username);
    }

    [Fact]
    public async Task ListUploadsAsync_BuildsTheUploadsRoute_AndDeserializesEachUpload()
    {
        const string Json = """
                            [
                              {
                                "id": 1,
                                "size": 1024,
                                "filename": "img.png",
                                "created_at": "2026-01-02T03:04:05.000Z",
                                "uploaded_by": {
                                  "id": 18,
                                  "username": "raymond_smith",
                                  "name": "Raymond Smith",
                                  "web_url": "https://gitlab.example/raymond_smith"
                                }
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabGroupUpload> uploads = [];
        await foreach (GitLabGroupUpload upload in repository.ListUploadsAsync(9970,
                           TestContext.Current.CancellationToken))
        {
            uploads.Add(upload);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/uploads",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        GitLabGroupUpload only = Assert.Single(uploads);
        Assert.Equal("img.png", only.Filename);
        Assert.Equal(1024, only.Size);
        Assert.Equal("raymond_smith", only.UploadedBy?.Username);
    }

    [Fact]
    public async Task UploadFileAsync_PostsMultipart_AndReturnsTheMarkdownSnippet()
    {
        const string Json = """
                            {
                              "id": 5,
                              "alt": "img",
                              "url": "/uploads/66dbcd21ec5d24ed6ea225176098d52b/img.png",
                              "full_path": "/-/system/-/group/9970/66dbcd21ec5d24ed6ea225176098d52b/img.png",
                              "markdown": "![img](/uploads/66dbcd21ec5d24ed6ea225176098d52b/img.png)"
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

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());

        GitLabGroupUploadedFile uploaded = await repository.UploadFileAsync(
            9970,
            new GitLabFileUpload { Content = content, FileName = "img.png", ContentType = "image/png" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/uploads",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Contains("name=file", (sentBody ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal),
            StringComparison.Ordinal);
        Assert.Equal(5, uploaded.Id);
        Assert.Equal("/uploads/66dbcd21ec5d24ed6ea225176098d52b/img.png", uploaded.Url?.OriginalString);
        Assert.Equal("![img](/uploads/66dbcd21ec5d24ed6ea225176098d52b/img.png)", uploaded.Markdown);
    }

    [Fact]
    public async Task DownloadUploadAsync_StreamsTheRawBody_ByUploadId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent("PNG-BYTES"u8.ToArray())
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        using GitLabFileResponse file =
            await repository.DownloadUploadAsync(9970, 5, TestContext.Current.CancellationToken);

        using StreamReader reader = new(file.Content);
        Assert.Equal("PNG-BYTES", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/uploads/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadUploadBySecretAsync_PercentEncodesTheSecretAndFileName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent("PNG-BYTES"u8.ToArray())
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        using GitLabFileResponse file = await repository.DownloadUploadBySecretAsync(
            "parent/child",
            "66dbcd21ec5d24ed6ea225176098d52b",
            "release notes.png",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent%2Fchild/uploads"
            + "/66dbcd21ec5d24ed6ea225176098d52b/release%20notes.png",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteUploadAsync_DeletesByUploadId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteUploadAsync(9970, 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/uploads/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteUploadBySecretAsync_DeletesByTheSecretAndFileNameFromTheMarkdownUrl()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteUploadBySecretAsync(9970, "66dbcd21ec5d24ed6ea225176098d52b", "img.png",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/uploads/66dbcd21ec5d24ed6ea225176098d52b/img.png",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadPlaceholderReassignmentsAsync_StreamsTheCsvRatherThanParsingIt()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("source_host,source_user_identifier\r\nhttps://gitlab.example,alice\r\n",
                Encoding.UTF8, "text/csv")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        using GitLabFileResponse file =
            await repository.DownloadPlaceholderReassignmentsAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/placeholder_reassignments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("text/csv", file.ContentType);

        using StreamReader reader = new(file.Content);
        Assert.StartsWith("source_host", await reader.ReadToEndAsync(TestContext.Current.CancellationToken),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReassignPlaceholdersAsync_PostsTheCsvAsMultipartFormData()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream content = new(Encoding.UTF8.GetBytes(
            "source_host,source_user_identifier\r\nhttps://gitlab.example,alice\r\n"));
        GitLabFileUpload file = new() { Content = content, FileName = "reassignments.csv", ContentType = "text/csv" };

        await repository.ReassignPlaceholdersAsync(9970, file, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/placeholder_reassignments",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.Contains("reassignments.csv", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task AuthorizePlaceholderReassignmentsAsync_PostsToTheAuthorizeRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.AuthorizePlaceholderReassignmentsAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/placeholder_reassignments/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAuditEventAsync_BuildsTheGroupAndAuditEventRoute_AndDeserializesTheDetails()
    {
        const string Json = """
                            {
                              "id": 5,
                              "author_id": 1,
                              "entity_id": 9970,
                              "entity_type": "Group",
                              "event_name": "group_archived",
                              "details": { "custom_message": "Archived the group" },
                              "created_at": "2023-03-09T10:00:00Z"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabGroupAuditEvent auditEvent =
            await repository.GetAuditEventAsync("parent-group/subgroup", 5, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/audit_events/5",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(5, auditEvent.Id);
        Assert.Equal("Group", auditEvent.EntityType);
        Assert.Equal("group_archived", auditEvent.EventName);
        Assert.Equal("Archived the group",
            auditEvent.Details?.GetProperty("custom_message").GetString());
    }

    [Fact]
    public async Task UpdateSecuritySettingsAsync_PutsTheFlagAndExcludedProjects()
    {
        string? sentBody = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdateSecuritySettingsAsync(9970,
            new UpdateGroupSecuritySettingsRequest { SecretPushProtectionEnabled = true, ProjectsToExclude = [1, 2] },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/security_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"secret_push_protection_enabled":true,"projects_to_exclude":[1,2]}""", sentBody);
    }
}