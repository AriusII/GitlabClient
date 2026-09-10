using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class GroupsEndpointTests
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

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task GetAsync_EncodesNamespacedGroupPath_AndDeserializesGroup()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabGroup group = await repository.GetAsync("parent-group/subgroup", TestContext.Current.CancellationToken);

        Assert.Contains("/groups/parent-group%2Fsubgroup", handler.LastRequest?.RequestUri?.AbsoluteUri,
            StringComparison.Ordinal);
        Assert.Equal(9970, group.Id);
        Assert.Equal("Subgroup", group.Name);
        Assert.Equal("subgroup", group.Path);
        Assert.Equal("public", group.Visibility);
        Assert.Equal("parent-group/subgroup", group.FullPath);
    }

    [Fact]
    public async Task GetAsync_WithOptions_ProjectsTheOfficialDetailQueryParameters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(GroupJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabGroup group = await repository.GetAsync(
            "parent/child",
            new GroupGetOptions { WithCustomAttributes = true, WithProjects = false },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent%2Fchild?with_custom_attributes=true&with_projects=false",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(9970, group.Id);
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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
                                "allowed_to_push": [{ "access_level": 30 }],
                                "allowed_to_merge": [{ "access_level": 40 }],
                                "allow_force_push": false,
                                "developer_can_initial_push": true,
                                "code_owner_approval_required": true
                              },
                              "parent_id": "42",
                              "file_template_project_id": "812",
                              "auto_devops_enabled": "enabled",
                              "mentions_disabled": "disabled",
                              "membership_lock": "enabled",
                              "emails_disabled": true,
                              "math_rendering_limits_enabled": true,
                              "lock_math_rendering_limits_enabled": false,
                              "resource_access_token_notify_inherited": true,
                              "lock_resource_access_token_notify_inherited": false,
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
                                "pipeline_artifacts_size": 81,
                                "packages_size": 0,
                                "snippets_size": 50,
                                "uploads_size": 0
                              },
                              "root_storage_statistics": {
                                "build_artifacts_size": 1,
                                "container_registry_size": 2,
                                "container_registry_size_is_estimated": true,
                                "dependency_proxy_size": 3,
                                "lfs_objects_size": 4,
                                "packages_size": 5,
                                "pipeline_artifacts_size": 6,
                                "repository_size": 7,
                                "snippets_size": 8,
                                "storage_size": 9,
                                "uploads_size": 10,
                                "wiki_size": 11
                              },
                              "custom_attributes": { "key": "cost_center", "value": "42" },
                              "ldap_access": "40",
                              "ldap_group_links": {
                                "cn": "developers",
                                "group_access": 40,
                                "provider": "ldapmain",
                                "filter": "memberOf=developers",
                                "member_role_id": 15
                              },
                              "saml_group_links": {
                                "name": "maintainers",
                                "access_level": 40,
                                "member_role_id": 16,
                                "provider": "saml"
                              },
                              "duo_core_features_enabled": true,
                              "duo_features_enabled": "enabled",
                              "lock_duo_features_enabled": "disabled",
                              "auto_duo_code_review_enabled": "enabled",
                              "web_based_commit_signing_enabled": "disabled",
                              "allow_personal_snippets": "enabled",
                              "duo_namespace_access_rules": "enabled",
                              "built_in_project_templates_enabled": true,
                              "lock_built_in_project_templates_enabled": false,
                              "shared_with_groups": [
                                {
                                  "unmodelled_share_field": "Ops"
                                }
                              ],
                              "shared_runners_minutes_limit": "400",
                              "extra_shared_runners_minutes_limit": "40",
                              "prevent_forking_outside_group": "enabled",
                              "service_access_tokens_expiration_enforced": "disabled",
                              "experiment_features_enabled": "enabled",
                              "ai_settings": {
                                "duo_agent_platform_enabled": true,
                                "duo_workflow_mcp_enabled": false,
                                "foundational_agents_default_enabled": true,
                                "ai_catalog_restricted_to_group_hierarchy": false,
                                "ai_usage_data_collection_enabled": true,
                                "prompt_injection_protection_level": "no_checks",
                                "include_recommended_allowed": true,
                                "allow_all_unix_sockets": false,
                                "allow_project_extension": true,
                                "minimum_access_level_execute": "developer",
                                "minimum_access_level_execute_async": "developer",
                                "minimum_access_level_manage": "maintainer",
                                "minimum_access_level_enable_on_projects": "maintainer"
                              },
                              "ip_restriction_ranges": "10.0.0.0/8",
                              "allowed_email_domains_list": "example.com",
                              "only_allow_merge_if_pipeline_succeeds": "enabled",
                              "allow_merge_on_skipped_pipeline": "disabled",
                              "only_allow_merge_if_all_discussions_are_resolved": "enabled",
                              "unique_project_download_limit": "100",
                              "unique_project_download_limit_interval_in_seconds": "60",
                              "unique_project_download_limit_allowlist": "release-bot",
                              "unique_project_download_limit_alertlist": "7",
                              "auto_ban_user_on_excessive_projects_download": "disabled",
                              "step_up_auth_required_oauth_provider": "saml"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabGroup group = await repository.GetAsync(9970, TestContext.Current.CancellationToken);

        Assert.True(group.Archived);
        Assert.Equal(new DateOnly(2026, 1, 31), group.MarkedForDeletionOn);
        Assert.Equal(1, group.OrganizationId);
        Assert.Equal("maintainer", group.ProjectCreationLevel);
        Assert.Equal("disabled_and_overridable", group.SharedRunnersSetting);
        Assert.Equal(48, group.TwoFactorGracePeriod);
        Assert.Equal("ABCDEF1234567890", group.RunnersToken);
        GitLabDefaultBranchProtectionDefaults defaultBranchProtectionDefaults =
            Assert.IsType<GitLabDefaultBranchProtectionDefaults>(
                group.DefaultBranchProtectionDefaults);
        Assert.Equal(30, Assert.Single(defaultBranchProtectionDefaults.AllowedToPush ?? []).AccessLevel);
        Assert.Equal(40, Assert.Single(defaultBranchProtectionDefaults.AllowedToMerge ?? []).AccessLevel);
        Assert.False(defaultBranchProtectionDefaults.AllowForcePush);
        Assert.True(defaultBranchProtectionDefaults.DeveloperCanInitialPush);
        Assert.True(defaultBranchProtectionDefaults.CodeOwnerApprovalRequired);
        Assert.Equal("42", group.ParentId);
        Assert.Equal("812", group.FileTemplateProjectId);
        Assert.Equal("enabled", group.AutoDevopsEnabled);
        Assert.Equal("disabled", group.MentionsDisabled);
        Assert.Equal("enabled", group.MembershipLock);
        Assert.True(group.EmailsDisabled);
        Assert.True(group.MathRenderingLimitsEnabled);
        Assert.False(group.LockMathRenderingLimitsEnabled);
        Assert.True(group.ResourceAccessTokenNotifyInherited);
        Assert.False(group.LockResourceAccessTokenNotifyInherited);
        Assert.Equal(212L, group.Statistics?.StorageSize);
        Assert.Equal(81L, group.Statistics?.PipelineArtifactsSize);
        Assert.Equal(2, group.RootStorageStatistics?.ContainerRegistrySize);
        Assert.True(group.RootStorageStatistics?.ContainerRegistrySizeIsEstimated);
        Assert.Equal("cost_center", group.CustomAttributes?.Key);
        Assert.Equal("42", group.CustomAttributes?.Value);
        Assert.Equal("40", group.LdapAccess);
        Assert.Equal("developers", group.LdapGroupLinks?.Cn);
        Assert.Equal(15, group.LdapGroupLinks?.MemberRoleId);
        Assert.Equal("maintainers", group.SamlGroupLinks?.Name);
        Assert.Equal(16, group.SamlGroupLinks?.MemberRoleId);
        Assert.True(group.DuoCoreFeaturesEnabled);
        Assert.Equal("enabled", group.DuoFeaturesEnabled);
        Assert.Equal("disabled", group.LockDuoFeaturesEnabled);
        Assert.Equal("enabled", group.AutoDuoCodeReviewEnabled);
        Assert.Equal("disabled", group.WebBasedCommitSigningEnabled);
        Assert.Equal("enabled", group.AllowPersonalSnippets);
        Assert.Equal("enabled", group.DuoNamespaceAccessRules);
        Assert.True(group.BuiltInProjectTemplatesEnabled);
        Assert.False(group.LockBuiltInProjectTemplatesEnabled);
        Assert.Equal("Ops",
            Assert.Single(group.SharedWithGroups ?? []).GetProperty("unmodelled_share_field").GetString());
        Assert.Equal("400", group.SharedRunnersMinutesLimit);
        Assert.Equal("40", group.ExtraSharedRunnersMinutesLimit);
        Assert.Equal("enabled", group.PreventForkingOutsideGroup);
        Assert.Equal("disabled", group.ServiceAccessTokensExpirationEnforced);
        Assert.Equal("enabled", group.ExperimentFeaturesEnabled);
        Assert.True(group.AiSettings?.DuoAgentPlatformEnabled);
        Assert.Equal("no_checks", group.AiSettings?.PromptInjectionProtectionLevel);
        Assert.Equal("maintainer", group.AiSettings?.MinimumAccessLevelManage);
        Assert.Equal("10.0.0.0/8", group.IpRestrictionRanges);
        Assert.Equal("example.com", group.AllowedEmailDomainsList);
        Assert.Equal("enabled", group.OnlyAllowMergeIfPipelineSucceeds);
        Assert.Equal("disabled", group.AllowMergeOnSkippedPipeline);
        Assert.Equal("enabled", group.OnlyAllowMergeIfAllDiscussionsAreResolved);
        Assert.Equal("100", group.UniqueProjectDownloadLimit);
        Assert.Equal("60", group.UniqueProjectDownloadLimitIntervalInSeconds);
        Assert.Equal("release-bot", group.UniqueProjectDownloadLimitAllowlist);
        Assert.Equal("7", group.UniqueProjectDownloadLimitAlertlist);
        Assert.Equal("disabled", group.AutoBanUserOnExcessiveProjectsDownload);
        Assert.Equal("saml", group.StepUpAuthRequiredOauthProvider);
    }

    [Fact]
    public async Task ListAsync_BuildsTheGroupsRoute_AndProjectsEveryFilter()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{GroupJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        GroupListOptions options = new()
        {
            Search = "ops", Visibility = GitLabVisibility.Private, SkipGroups = [21, 22], PerPage = 50
        };

        List<GitLabGroup> groups = [];
        await foreach (GitLabGroup group in repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            groups.Add(group);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups?search=ops&visibility=private&skip_groups=21,22&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(9970, Assert.Single(groups).Id);
    }

    [Fact]
    public async Task ListAsync_ProjectsTheRemainingOfficialGroupFilters()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{GroupJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        GroupListOptions options = new()
        {
            Statistics = true,
            Archived = false,
            AllAvailable = true,
            Owned = true,
            OrderBy = "similarity",
            Sort = "desc",
            MinAccessLevel = 40,
            TopLevelOnly = true,
            MarkedForDeletionOn = new DateOnly(2026, 9, 10),
            Active = true,
            RepositoryStorage = "default",
            WithCustomAttributes = true
        };

        List<GitLabGroup> groups = [];
        await foreach (GitLabGroup group in repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            groups.Add(group);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups?statistics=true&archived=false&all_available=true&owned=true"
            + "&order_by=similarity&sort=desc&min_access_level=40&top_level_only=true"
            + "&marked_for_deletion_on=2026-09-10&active=true&repository_storage=default"
            + "&with_custom_attributes=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(9970, Assert.Single(groups).Id);
    }

    [Fact]
    public async Task CreateAsync_PostsMultipartFormData_WithTheSourceGeneratedWireFieldNames()
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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        Assert.Equal("multipart/form-data", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        AssertFormField(sentBody, "name", "Subgroup");
        AssertFormField(sentBody, "path", "subgroup");
        AssertFormField(sentBody, "parent_id", "42");
        AssertFormField(sentBody, "visibility", "internal");
        AssertFormField(sentBody, "require_two_factor_authentication", "true");
        AssertFormField(sentBody, "project_creation_level", "noone");
        AssertFormField(sentBody, "subgroup_creation_level", "owner");
        AssertFormField(sentBody, "enabled_git_access_protocol", "ssh");
        AssertFormField(sentBody, "wiki_access_level", "private");
        AssertFormField(sentBody, "duo_availability", "default_off");
        Assert.DoesNotContain("name=avatar", NormalizeMultipartBody(sentBody), StringComparison.Ordinal);
        Assert.Equal(9970, group.Id);
    }

    [Fact]
    public async Task UpdateAsync_PutsMultipartFormData_ToTheEncodedGroupRoute()
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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        Assert.Equal("multipart/form-data", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        AssertFormField(sentBody, "description", "Renamed");
        AssertFormField(sentBody, "shared_runners_setting", "disabled_and_unoverridable");
        AssertFormField(sentBody, "unique_project_download_limit_allowlist[0]", "release-bot");
        AssertFormField(sentBody, "unique_project_download_limit_alertlist[0]", "7");
        AssertFormField(sentBody, "unique_project_download_limit_alertlist[1]", "9");
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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        await repository.TransferToOrganizationAsync(
            9970,
            new TransferGroupToOrganizationRequest { OrganizationId = 3 },
            TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/groups/9970/transfer_to_organization",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"organization_id":3}""", sentBody);
    }

    [Theory]
    [InlineData("Any", "None")]
    [InlineData("None", "Any")]
    public async Task ListIssuesAsync_ProjectsTheOpenApiNegatedFiltersAndAssigneeSentinels(
        string assigneeId,
        string notIterationId)
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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabIssue> issues = [];
        await foreach (GitLabIssue issue in repository.ListIssuesAsync(
                           "parent/child",
                           new GroupIssueListOptions
                           {
                               State = "opened",
                               Labels = ["bug"],
                               NotLabels = ["wontfix"],
                               NotAuthorId = 5,
                               AssigneeId = assigneeId,
                               NotAssigneeUsername = ["alice", "bob"],
                               SearchIn = "title,description",
                               Search = "crash",
                               NotWeight = 3,
                               NotIterationId = notIterationId,
                               NotIterationTitle = "Iteration 2",
                               NonArchived = true
                           },
                           TestContext.Current.CancellationToken))
        {
            issues.Add(issue);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/parent%2Fchild/issues"
            + "?state=opened&labels=bug&not[labels]=wontfix&search=crash&in=title%2Cdescription"
            + $"&not[author_id]=5&assignee_id={assigneeId}&not[assignee_username]=alice,bob"
            + $"&not[weight]=3&not[iteration_id]={notIterationId}&not[iteration_title]=Iteration%202"
            + "&non_archived=true",
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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabBillableMember> members = [];
        await foreach (GitLabBillableMember member in repository.ListBillableMembersAsync(
                           9970,
                           new GroupBillableMemberListOptions
                           {
                               Sort = "last_activity_on_desc", Page = 2, PerPage = 100
                           },
                           TestContext.Current.CancellationToken))
        {
            members.Add(member);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/groups/9970/billable_members?sort=last_activity_on_desc&page=2&per_page=100",
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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
    public async Task AuthorizeUploadAsync_PostsToTheAuthorizeRoute_AndIgnoresWorkhorsesBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"TempPath":"/var/opt/gitlab/uploads/tmp"}""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        await repository.AuthorizeUploadAsync(9970, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/uploads/authorize",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DownloadUploadAsync_StreamsTheRawBody_ByUploadId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent("PNG-BYTES"u8.ToArray())
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

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
        GroupsClient repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdateSecuritySettingsAsync(9970,
            new UpdateGroupSecuritySettingsRequest { SecretPushProtectionEnabled = true, ProjectsToExclude = [1, 2] },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/9970/security_settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"secret_push_protection_enabled":true,"projects_to_exclude":[1,2]}""", sentBody);
    }

    private static void AssertFormField(string? body, string name, string value)
    {
        Assert.Contains($"name={name}\r\n\r\n{value}\r\n", NormalizeMultipartBody(body), StringComparison.Ordinal);
    }

    private static string NormalizeMultipartBody(string? body)
    {
        return (body ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal);
    }
}