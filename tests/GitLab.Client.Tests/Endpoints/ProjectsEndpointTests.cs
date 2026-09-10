using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Models.Responses;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class ProjectsEndpointTests
{
    private const string ProjectJson = """
                                       {
                                         "id": 278964,
                                         "name": "GitLab",
                                         "path_with_namespace": "gitlab-org/gitlab",
                                         "visibility": "public",
                                         "web_url": "https://gitlab.com/gitlab-org/gitlab"
                                       }
                                       """;

    private static readonly string[] Topics = ["devops", "ruby"];

    private static readonly string[] SubClaimComponents = ["project_path", "ref_type"];

    private static readonly long[] SkippedUsers = [7, 9];

    [Fact]
    public async Task GetAsync_EncodesNamespacedPath_AndDeserializesProject()
    {
        using Harness harness = new(HttpStatusCode.OK, ProjectJson);

        GitLabProject project =
            await harness.Repository.GetAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken);

        Assert.Contains("/projects/gitlab-org%2Fgitlab", harness.RequestUri);
        Assert.Equal(278964, project.Id);
        Assert.Equal(GitLabVisibility.Public, project.Visibility);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        using Harness harness = new(HttpStatusCode.NotFound, """{ "message": "404 Project Not Found" }""");

        GitLabApiException exception =
            await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
                harness.Repository.GetAsync(1, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Project Not Found", exception.Message);
    }

    [Fact]
    public async Task ListAsync_ProjectsTheListFilters_AndDeserializesTheProject()
    {
        using Harness harness = new(HttpStatusCode.OK, $"[{ProjectJson}]");

        ProjectListOptions options = new()
        {
            Search = "gitlab",
            Visibility = GitLabVisibility.Public,
            Archived = false,
            OrderBy = "last_activity_at",
            Sort = "desc",
            Owned = true,
            PerPage = 20
        };

        List<GitLabProject> projects = [];
        await foreach (GitLabProject project in harness.Repository.ListAsync(options,
                           TestContext.Current.CancellationToken))
        {
            projects.Add(project);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects?search=gitlab&visibility=public&archived=false"
            + "&per_page=20&order_by=last_activity_at&sort=desc&owned=true",
            harness.RequestUri);

        GitLabProject project2 = Assert.Single(projects);
        Assert.Equal(278964, project2.Id);
        Assert.Equal("gitlab-org/gitlab", project2.PathWithNamespace);
    }

    /// <summary>
    ///     The create form carries ~90 fields whose wire names come from the context's snake_case policy
    ///     rather than from a per-property attribute. These representative scalar, nested and array fields keep a
    ///     mis-cased name (which GitLab would answer 200 to, and silently ignore) out of the library.
    /// </summary>
    [Fact]
    public async Task CreateAsync_PostsMultipartFormData_WithTheSourceGeneratedWireFieldNames()
    {
        using Harness harness = new(HttpStatusCode.Created, ProjectJson);

        CreateProjectRequest request = new()
        {
            Name = "GitLab",
            Path = "gitlab",
            IssuesAccessLevel = GitLabProjectFeatureAccessLevel.Enabled,
            PagesAccessLevel = GitLabProjectPublicFeatureAccessLevel.Public,
            ContainerExpirationPolicyAttributes =
                new ContainerExpirationPolicyAttributes { Cadence = "1d", Enabled = true, KeepN = 10 },
            Visibility = GitLabVisibility.Internal,
            Topics = Topics,
            MergeMethod = GitLabMergeMethod.RebaseMerge,
            NamespaceId = 42,
            ImportUrl = new Uri("https://example.com/repo.git")
        };

        await harness.Repository.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects", harness.RequestUri);
        Assert.Equal("multipart/form-data", harness.ContentType);
        AssertFormField(harness.SentBody, "name", "GitLab");
        AssertFormField(harness.SentBody, "path", "gitlab");
        AssertFormField(harness.SentBody, "issues_access_level", "enabled");
        AssertFormField(harness.SentBody, "pages_access_level", "public");
        AssertFormField(harness.SentBody, "container_expiration_policy_attributes[cadence]", "1d");
        AssertFormField(harness.SentBody, "container_expiration_policy_attributes[enabled]", "true");
        AssertFormField(harness.SentBody, "container_expiration_policy_attributes[keep_n]", "10");
        AssertFormField(harness.SentBody, "visibility", "internal");
        AssertFormField(harness.SentBody, "topics[0]", "devops");
        AssertFormField(harness.SentBody, "topics[1]", "ruby");
        AssertFormField(harness.SentBody, "merge_method", "rebase_merge");
        AssertFormField(harness.SentBody, "namespace_id", "42");
        AssertFormField(harness.SentBody, "import_url", "https://example.com/repo.git");
        Assert.DoesNotContain("name=avatar", NormalizeMultipartBody(harness.SentBody), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateForUserAsync_PostsMultipartFormData_ToTheAdministratorRoute()
    {
        using Harness harness = new(HttpStatusCode.Created, ProjectJson);

        await harness.Repository.CreateForUserAsync(9, new CreateProjectRequest { Name = "GitLab" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/user/9", harness.RequestUri);
        Assert.Equal("multipart/form-data", harness.ContentType);
        AssertFormField(harness.SentBody, "name", "GitLab");
    }

    [Fact]
    public async Task UpdateAsync_PutsOnlyTheSetFieldsAsMultipartFormData()
    {
        using Harness harness = new(HttpStatusCode.OK, ProjectJson);

        UpdateProjectRequest request = new()
        {
            Description = "Now with docs",
            AutoDevopsEnabled = true,
            MrDefaultTargetSelf = false,
            SppRepositoryPipelineAccess = true,
            CiPipelineVariablesMinimumOverrideRole = GitLabMinimumRole.Maintainer,
            CiIdTokenSubClaimComponents = SubClaimComponents,
            MergeTrainEnforcement = GitLabMergeTrainEnforcement.EnforceForAllUsers,
            ReviewerAssignmentStrategy = GitLabReviewerAssignmentStrategy.CodeOwners
        };

        await harness.Repository.UpdateAsync("gitlab-org/gitlab", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab", harness.RequestUri);
        Assert.Equal("multipart/form-data", harness.ContentType);
        AssertFormField(harness.SentBody, "description", "Now with docs");
        AssertFormField(harness.SentBody, "auto_devops_enabled", "true");
        AssertFormField(harness.SentBody, "mr_default_target_self", "false");
        AssertFormField(harness.SentBody, "spp_repository_pipeline_access", "true");
        AssertFormField(harness.SentBody, "ci_pipeline_variables_minimum_override_role", "maintainer");
        AssertFormField(harness.SentBody, "ci_id_token_sub_claim_components[0]", "project_path");
        AssertFormField(harness.SentBody, "ci_id_token_sub_claim_components[1]", "ref_type");
        AssertFormField(harness.SentBody, "merge_train_enforcement", "enforce_for_all_users");
        AssertFormField(harness.SentBody, "reviewer_assignment_strategy", "code_owners");
    }

    [Fact]
    public async Task SetAvatarAsync_PutsAMultipartBody_UnderGitLabsOwnFieldName()
    {
        using Harness harness = new(HttpStatusCode.OK, ProjectJson);
        using MemoryStream avatarBytes = new([0x89, 0x50, 0x4E, 0x47]);

        // FieldName is deliberately left at its "file" default: the repository must override it, because
        // GitLab silently ignores an avatar part sent under any other name.
        GitLabFileUpload avatar = new() { Content = avatarBytes, FileName = "logo.png", ContentType = "image/png" };

        await harness.Repository.SetAvatarAsync(42, avatar, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42", harness.RequestUri);
        Assert.Contains("name=avatar", harness.SentBody);
        Assert.Contains("filename=logo.png", harness.SentBody);
    }

    [Fact]
    public async Task DeleteAsync_AcceptsGitLabs202_AndSendsNoBody()
    {
        using Harness harness = new(HttpStatusCode.Accepted, null);

        await harness.Repository.DeleteAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab", harness.RequestUri);
    }

    [Theory]
    [InlineData("archive")]
    [InlineData("unarchive")]
    [InlineData("star")]
    [InlineData("unstar")]
    [InlineData("restore")]
    public async Task ProjectStateActions_PostToTheirOwnSubRoute(string action)
    {
        using Harness harness = new(HttpStatusCode.Created, ProjectJson);

        GitLabProject project;

        switch (action)
        {
            case "archive":
                project = await harness.Repository.ArchiveAsync(42, TestContext.Current.CancellationToken);
                break;
            case "unarchive":
                project = await harness.Repository.UnarchiveAsync(42, TestContext.Current.CancellationToken);
                break;
            case "star":
                project = await harness.Repository.StarAsync(42, TestContext.Current.CancellationToken);
                break;
            case "unstar":
                project = await harness.Repository.UnstarAsync(42, TestContext.Current.CancellationToken);
                break;
            default:
                project = await harness.Repository.RestoreAsync(42, TestContext.Current.CancellationToken);
                break;
        }

        Assert.Equal(HttpMethod.Post, harness.Method);
        Assert.Equal($"https://gitlab.example/api/v4/projects/42/{action}", harness.RequestUri);
        Assert.Equal(278964, project.Id);
    }

    [Fact]
    public async Task ForkAsync_WithNoRequest_PostsNoPayload()
    {
        using Harness harness = new(HttpStatusCode.Created, ProjectJson);

        GitLabProject project = await harness.Repository.ForkAsync("gitlab-org/gitlab", cancellationToken:
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/fork", harness.RequestUri);
        Assert.Null(harness.SentBody);
        Assert.Equal(278964, project.Id);
    }

    [Fact]
    public async Task ForkAsync_SendsTheTargetNamespaceAndVisibility()
    {
        using Harness harness = new(HttpStatusCode.Created, ProjectJson);

        ForkProjectRequest request = new()
        {
            NamespacePath = "my-group/sub", Path = "gitlab-fork", Visibility = GitLabVisibility.Private
        };

        await harness.Repository.ForkAsync(42, request, TestContext.Current.CancellationToken);

        Assert.Equal(
            """{"namespace_path":"my-group/sub","path":"gitlab-fork","visibility":"private"}""",
            harness.SentBody);
    }

    [Fact]
    public async Task ForkRelationship_UsesTheSameRouteForBothVerbs()
    {
        using Harness created = new(HttpStatusCode.Created, ProjectJson);

        await created.Repository.CreateForkRelationshipAsync("group/fork", "group/upstream",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, created.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/group%2Ffork/fork/group%2Fupstream",
            created.RequestUri);

        using Harness deleted = new(HttpStatusCode.NoContent, null);

        await deleted.Repository.DeleteForkRelationshipAsync("group/fork", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, deleted.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/group%2Ffork/fork", deleted.RequestUri);
    }

    [Fact]
    public async Task ListForksAsync_ProjectsTheSharedProjectFilters()
    {
        using Harness harness = new(HttpStatusCode.OK, "[]");

        ProjectListOptions options = new()
        {
            Search = "runner",
            OrderBy = "last_activity_at",
            Sort = "desc",
            MinAccessLevel = 30,
            IdAfter = 100,
            IncludeHidden = true,
            Simple = true,
            PerPage = 50
        };

        await foreach (GitLabProject _ in harness.Repository.ListForksAsync(42, options,
                           TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/forks?search=runner&per_page=50"
            + "&order_by=last_activity_at&sort=desc&min_access_level=30&id_after=100&include_hidden=true"
            + "&simple=true",
            harness.RequestUri);
    }

    [Fact]
    public async Task TransferAsync_PutsTheTargetNamespace()
    {
        using Harness harness = new(HttpStatusCode.OK, ProjectJson);

        await harness.Repository.TransferAsync(42, new TransferProjectRequest { Namespace = "new-group" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/transfer", harness.RequestUri);
        Assert.Equal("""{"namespace":"new-group"}""", harness.SentBody);
    }

    [Fact]
    public async Task ListTransferLocationsAsync_DeserializesTheTrimmedGroupShape()
    {
        const string Json = """
                            [
                              {
                                "id": 27,
                                "name": "Charts",
                                "web_url": "https://gitlab.example/groups/gitlab-org/charts",
                                "full_name": "GitLab.org / Charts",
                                "full_path": "gitlab-org/charts",
                                "avatar_url": null
                              }
                            ]
                            """;

        using Harness harness = new(HttpStatusCode.OK, Json);

        List<GitLabPublicGroupDetails> locations = [];

        await foreach (GitLabPublicGroupDetails location in harness.Repository.ListTransferLocationsAsync(42,
                           new ProjectTransferLocationListOptions { Search = "charts", PerPage = 20 },
                           TestContext.Current.CancellationToken))
        {
            locations.Add(location);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/transfer_locations?search=charts&per_page=20",
            harness.RequestUri);
        GitLabPublicGroupDetails only = Assert.Single(locations);
        Assert.Equal(27, only.Id);
        Assert.Equal("gitlab-org/charts", only.FullPath);
        Assert.Null(only.AvatarUrl);
    }

    [Fact]
    public async Task ListAncestorGroupsAsync_ProjectsTheSharedGroupFilters()
    {
        using Harness harness = new(HttpStatusCode.OK, "[]");

        ProjectAncestorGroupListOptions options = new()
        {
            Search = "org", SkipGroups = [3, 4], WithShared = true, SharedMinAccessLevel = 20
        };

        await foreach (GitLabPublicGroupDetails _ in harness.Repository.ListAncestorGroupsAsync(
                           "gitlab-org/gitlab", options, TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/groups?search=org&skip_groups=3,4"
            + "&with_shared=true&shared_min_access_level=20",
            harness.RequestUri);
    }

    [Fact]
    public async Task ListInvitedGroupsAsync_SendsTheRelationFilterCommaJoined()
    {
        using Harness harness = new(HttpStatusCode.OK, "[]");

        await foreach (GitLabGroup _ in harness.Repository.ListInvitedGroupsAsync(42,
                           new ProjectInvitedGroupListOptions { Relation = ["direct", "inherited"] },
                           TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/invited_groups?relation=direct,inherited",
            harness.RequestUri);
    }

    [Fact]
    public async Task ListShareLocationsAsync_TargetsTheShareLocationsRoute()
    {
        using Harness harness = new(HttpStatusCode.OK, "[]");

        await foreach (GitLabGroup _ in harness.Repository.ListShareLocationsAsync(42,
                           new ProjectShareLocationListOptions { Search = "platform" },
                           TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/share_locations?search=platform",
            harness.RequestUri);
    }

    [Fact]
    public async Task ShareAsync_PostsTheGroupLink_AndParsesItBack()
    {
        const string Json = """
                            {
                              "id": 11,
                              "project_id": 42,
                              "group_id": 27,
                              "group_access": 30,
                              "expires_at": "2026-12-31"
                            }
                            """;

        using Harness harness = new(HttpStatusCode.Created, Json);

        ShareProjectRequest request = new() { GroupId = 27, GroupAccess = 30, ExpiresAt = new DateOnly(2026, 12, 31) };

        GitLabProjectGroupLink link = await harness.Repository.ShareAsync(42, request,
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/share", harness.RequestUri);
        Assert.Equal("""{"group_id":27,"group_access":30,"expires_at":"2026-12-31"}""", harness.SentBody);
        Assert.Equal(11, link.Id);
        Assert.Equal(new DateOnly(2026, 12, 31), link.ExpiresAt);
    }

    [Fact]
    public async Task UnshareAsync_DeletesTheGroupsShareLink()
    {
        using Harness harness = new(HttpStatusCode.NoContent, null);

        await harness.Repository.UnshareAsync("gitlab-org/gitlab", 27, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/share/27", harness.RequestUri);
    }

    [Fact]
    public async Task ListUsersAsync_SendsSkipUsersAsAnIdList()
    {
        using Harness harness = new(HttpStatusCode.OK, "[]");

        await foreach (GitLabUser _ in harness.Repository.ListUsersAsync(42,
                           new ProjectUserListOptions { Search = "jane", SkipUsers = SkippedUsers, PerPage = 10 },
                           TestContext.Current.CancellationToken))
        {
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/users?search=jane&skip_users=7,9&per_page=10",
            harness.RequestUri);
    }

    /// <summary>
    ///     The spec declares this endpoint's response as the plain user entity, but GitLab actually answers
    ///     with the star - the user nested under a "user" key next to "starred_since". This pins the shape
    ///     the library is built against.
    /// </summary>
    [Fact]
    public async Task ListStarrersAsync_DeserializesTheStarWrapper_NotABareUser()
    {
        const string Json = """
                            [
                              {
                                "starred_since": "2026-01-28T14:47:30.642Z",
                                "user": {
                                  "id": 1,
                                  "username": "jane_smith",
                                  "name": "Jane Smith",
                                  "state": "active",
                                  "web_url": "https://gitlab.example/jane_smith"
                                }
                              }
                            ]
                            """;

        using Harness harness = new(HttpStatusCode.OK, Json);

        List<GitLabProjectStarrer> starrers = [];

        await foreach (GitLabProjectStarrer starrer in harness.Repository.ListStarrersAsync(42,
                           new ProjectStarrerListOptions { Search = "jane" },
                           TestContext.Current.CancellationToken))
        {
            starrers.Add(starrer);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/42/starrers?search=jane", harness.RequestUri);
        GitLabProjectStarrer only = Assert.Single(starrers);
        Assert.Equal("jane_smith", only.User?.Username);
        Assert.Equal(new DateTimeOffset(2026, 1, 28, 14, 47, 30, 642, TimeSpan.Zero), only.StarredSince);
    }

    [Fact]
    public async Task ImportMembersAsync_TargetsTheSourceProject()
    {
        using Harness harness = new(HttpStatusCode.OK, null);

        await harness.Repository.ImportMembersAsync("gitlab-org/gitlab", 32,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/import_project_members/32",
            harness.RequestUri);
    }

    [Fact]
    public async Task GetLanguagesAsync_DeserializesTheBareLanguageMap()
    {
        using Harness harness = new(HttpStatusCode.OK, """{"Ruby":66.63,"JavaScript":22.96,"HTML":7.9}""");

        IReadOnlyDictionary<string, double> languages =
            await harness.Repository.GetLanguagesAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/languages", harness.RequestUri);
        Assert.Equal(3, languages.Count);
        Assert.Equal(66.63, languages["Ruby"]);
    }

    [Fact]
    public async Task GetStatisticsAsync_DeserializesTheFetchBreakdown()
    {
        const string Json = """
                            {
                              "fetches": {
                                "total": 50,
                                "days": [
                                  { "count": 10, "date": "2026-01-10" },
                                  { "count": 40, "date": "2026-01-09" }
                                ]
                              }
                            }
                            """;

        using Harness harness = new(HttpStatusCode.OK, Json);

        GitLabProjectDailyStatistics statistics =
            await harness.Repository.GetStatisticsAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/statistics", harness.RequestUri);
        Assert.Equal(50, statistics.Fetches?.Total);
        Assert.Equal(2, statistics.Fetches?.Days?.Count);
        Assert.Equal(new DateOnly(2026, 1, 10), statistics.Fetches?.Days?[0].Date);
    }

    [Fact]
    public async Task GetStorageAsync_DeserializesTheGitalyShard()
    {
        const string Json = """
                            {
                              "project_id": 42,
                              "disk_path": "@hashed/6b/86/6b86b273.git",
                              "repository_storage": "default",
                              "created_at": "2026-01-02T03:04:05Z"
                            }
                            """;

        using Harness harness = new(HttpStatusCode.OK, Json);

        GitLabProjectStorage storage =
            await harness.Repository.GetStorageAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/storage", harness.RequestUri);
        Assert.Equal("default", storage.RepositoryStorage);
        Assert.Equal("@hashed/6b/86/6b86b273.git", storage.DiskPath);
    }

    [Fact]
    public async Task StartHousekeepingAsync_SendsTheBareTaskName_NotTheSpecsRubySymbol()
    {
        using Harness harness = new(HttpStatusCode.Created, null);

        await harness.Repository.StartHousekeepingAsync(42,
            new ProjectHousekeepingRequest { Task = GitLabHousekeepingTask.Prune },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/housekeeping", harness.RequestUri);
        Assert.Equal("""{"task":"prune"}""", harness.SentBody);
    }

    [Fact]
    public async Task StartHousekeepingAsync_WithNoRequest_PostsNoPayload()
    {
        using Harness harness = new(HttpStatusCode.Created, null);

        await harness.Repository.StartHousekeepingAsync(42, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/housekeeping", harness.RequestUri);
        Assert.Null(harness.SentBody);
    }

    [Fact]
    public async Task RecalculateRepositorySizeAsync_PostsToTheRepositorySizeRoute()
    {
        using Harness harness = new(HttpStatusCode.Created, null);

        await harness.Repository.RecalculateRepositorySizeAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository_size", harness.RequestUri);
    }

    [Fact]
    public async Task SecuritySettings_ReadAndWriteTheSameRoute()
    {
        const string Json = """
                            {
                              "project_id": 42,
                              "secret_push_protection_enabled": true,
                              "pre_receive_secret_detection_enabled": false
                            }
                            """;

        using Harness read = new(HttpStatusCode.OK, Json);

        GitLabProjectSecuritySettings settings =
            await read.Repository.GetSecuritySettingsAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/security_settings", read.RequestUri);
        Assert.True(settings.SecretPushProtectionEnabled);

        using Harness write = new(HttpStatusCode.OK, Json);

        await write.Repository.UpdateSecuritySettingsAsync(42,
            new UpdateProjectSecuritySettingsRequest { SecretPushProtectionEnabled = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, write.Method);
        Assert.Equal("""{"secret_push_protection_enabled":true}""", write.SentBody);
    }

    [Fact]
    public async Task CreateCiConfigMergeRequestAsync_ReturnsTheMergeRequestGitLabOpened()
    {
        const string Json = """
                            {
                              "id": 900,
                              "iid": 3,
                              "title": "Add .gitlab-ci.yml",
                              "state": "opened",
                              "source_branch": "add-gitlab-ci",
                              "target_branch": "main",
                              "web_url": "https://gitlab.example/gitlab-org/gitlab/-/merge_requests/3"
                            }
                            """;

        using Harness harness = new(HttpStatusCode.Created, Json);

        GitLabMergeRequest mergeRequest =
            await harness.Repository.CreateCiConfigMergeRequestAsync(42, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, harness.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/create_ci_config", harness.RequestUri);
        Assert.Equal(3, mergeRequest.Iid);
    }

    [Fact]
    public async Task ListAuditEventsAsync_SendsTheDateWindow_AndKeepsDetailsAsRawJson()
    {
        const string Json = """
                            [
                              {
                                "id": 5,
                                "author_id": 1,
                                "entity_id": 42,
                                "entity_type": "Project",
                                "event_name": "project_archived",
                                "details": { "custom_message": "Project archived", "target_id": 42 },
                                "created_at": "2026-02-03T04:05:06Z"
                              }
                            ]
                            """;

        using Harness harness = new(HttpStatusCode.OK, Json);

        List<GitLabProjectAuditEvent> events = [];

        await foreach (GitLabProjectAuditEvent auditEvent in harness.Repository.ListAuditEventsAsync(42,
                           new ProjectAuditEventListOptions
                           {
                               CreatedAfter = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), PerPage = 100
                           },
                           TestContext.Current.CancellationToken))
        {
            events.Add(auditEvent);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/audit_events?created_after=2026-01-01T00:00:00Z&per_page=100",
            harness.RequestUri);
        GitLabProjectAuditEvent only = Assert.Single(events);
        Assert.Equal("project_archived", only.EventName);
        Assert.Equal("Project archived", only.Details?.GetProperty("custom_message").GetString());
    }

    [Fact]
    public async Task GetAuditEventAsync_AddsTheEventIdAsAPathSegment()
    {
        const string Json = """{ "id": 5, "entity_type": "Project" }""";

        using Harness harness = new(HttpStatusCode.OK, Json);

        GitLabProjectAuditEvent auditEvent =
            await harness.Repository.GetAuditEventAsync("gitlab-org/gitlab", 5,
                TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/audit_events/5",
            harness.RequestUri);
        Assert.Equal(5, auditEvent.Id);
        Assert.Null(auditEvent.Details);
    }

    [Theory]
    [InlineData("projects")]
    [InlineData("starred_projects")]
    [InlineData("contributed_projects")]
    public async Task UserProjectListings_EscapeTheUserSegment(string route)
    {
        using Harness harness = new(HttpStatusCode.OK, "[]");

        ProjectListOptions options = new() { OrderBy = "star_count", Simple = true };

        switch (route)
        {
            case "projects":
                await foreach (GitLabProject _ in harness.Repository.ListForUserAsync("jane smith", options,
                                   TestContext.Current.CancellationToken))
                {
                }

                break;
            case "starred_projects":
                await foreach (GitLabProject _ in harness.Repository.ListStarredByUserAsync("jane smith", options,
                                   TestContext.Current.CancellationToken))
                {
                }

                break;
            default:
                await foreach (GitLabProject _ in harness.Repository.ListContributedByUserAsync("jane smith",
                                   options, TestContext.Current.CancellationToken))
                {
                }

                break;
        }

        Assert.Equal(
            $"https://gitlab.example/api/v4/users/jane%20smith/{route}?order_by=star_count&simple=true",
            harness.RequestUri);
    }

    private static void AssertFormField(string? body, string name, string value)
    {
        Assert.Contains($"name={name}\r\n\r\n{value}\r\n", NormalizeMultipartBody(body), StringComparison.Ordinal);
    }

    private static string NormalizeMultipartBody(string? body)
    {
        return (body ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Wires a <see cref="ProjectsClient" /> to a stubbed transport, capturing the one request it
    ///     makes so a test can assert the verb, the URL and the serialized body.
    /// </summary>
    private sealed class Harness : IDisposable
    {
        private readonly StubHttpMessageHandler _handler;
        private readonly HttpClient _httpClient;

        public Harness(HttpStatusCode status, string? json)
        {
            _handler = new StubHttpMessageHandler(request =>
            {
                SentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                ContentType = request.Content?.Headers.ContentType?.MediaType;

                HttpResponseMessage response = new(status);

                if (json is not null)
                {
                    response.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                return response;
            });

            _httpClient = new HttpClient(_handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
            Repository = new ProjectsClient(new GitLabApiConnection(_httpClient));
        }

        public ProjectsClient Repository { get; }

        public string? SentBody { get; private set; }

        public string? ContentType { get; private set; }

        public string? RequestUri => _handler.LastRequest?.RequestUri?.AbsoluteUri;

        public HttpMethod? Method => _handler.LastRequest?.Method;

        public void Dispose()
        {
            _httpClient.Dispose();
            _handler.Dispose();
        }
    }
}