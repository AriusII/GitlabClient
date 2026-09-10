using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class MergeRequestApprovalsEndpointTests
{
    private const string ApprovalConfigurationJson = """
                                                     {
                                                       "approvers": [
                                                         {
                                                           "user": {
                                                             "id": 7,
                                                             "username": "approver",
                                                             "name": "Ada Approver",
                                                             "web_url": "https://gitlab.example/approver"
                                                           }
                                                         }
                                                       ],
                                                       "approver_groups": [
                                                         {
                                                           "group": {
                                                             "id": 90,
                                                             "name": "Reviewers",
                                                             "path": "reviewers",
                                                             "visibility": "private",
                                                             "web_url": "https://gitlab.example/groups/reviewers"
                                                           }
                                                         }
                                                       ],
                                                       "approvals_before_merge": 2,
                                                       "reset_approvals_on_push": true,
                                                       "selective_code_owner_removals": false,
                                                       "disable_overriding_approvers_per_merge_request": true,
                                                       "merge_requests_author_approval": false,
                                                       "merge_requests_disable_committers_approval": true,
                                                       "require_password_to_approve": false,
                                                       "require_reauthentication_to_approve": true
                                                     }
                                                     """;

    private const string ApprovalSettingJson = """
                                               {
                                                 "allow_author_approval": true,
                                                 "allow_committer_approval": false,
                                                 "allow_overrides_to_approver_list_per_merge_request": true,
                                                 "retain_approvals_on_push": false,
                                                 "selective_code_owner_removals": true,
                                                 "require_password_to_approve": false,
                                                 "require_reauthentication_to_approve": true
                                               }
                                               """;

    private const string ApprovalsJson = """
                                         {
                                           "user_has_approved": true,
                                           "user_can_approve": false,
                                           "approved": true,
                                           "approved_by": [
                                             {
                                               "user": {
                                                 "id": 7,
                                                 "username": "approver",
                                                 "name": "Ada Approver",
                                                 "state": "active",
                                                 "web_url": "https://gitlab.example/approver"
                                               },
                                               "approved_at": "2025-01-01T10:00:00Z"
                                             }
                                           ]
                                         }
                                         """;

    private const string RuleJson = """
                                    {
                                      "id": 55,
                                      "name": "QA",
                                      "rule_type": "regular",
                                      "approvals_required": 2,
                                      "contains_hidden_groups": false,
                                      "report_type": null,
                                      "section": "Backend",
                                      "overridden": true,
                                      "source_rule": { "approvals_required": 3 },
                                      "eligible_approvers": [
                                        {
                                          "id": 7,
                                          "username": "approver",
                                          "name": "Ada Approver",
                                          "web_url": "https://gitlab.example/approver"
                                        }
                                      ],
                                      "users": [
                                        {
                                          "id": 7,
                                          "username": "approver",
                                          "name": "Ada Approver",
                                          "web_url": "https://gitlab.example/approver"
                                        }
                                      ],
                                      "groups": [
                                        {
                                          "id": 90,
                                          "name": "Reviewers",
                                          "path": "reviewers",
                                          "visibility": "private",
                                          "web_url": "https://gitlab.example/groups/reviewers"
                                        }
                                      ]
                                    }
                                    """;

    [Fact]
    public async Task GetApprovalsAsync_BuildsApprovalsRoute_AndDeserializesApprovers()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, ApprovalsJson));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabMergeRequestApprovals approvals =
                await repository.GetApprovalsAsync(1, 12, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/12/approvals",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            Assert.True(approvals.UserHasApproved);
            Assert.False(approvals.UserCanApprove);
            Assert.True(approvals.Approved);

            GitLabApproval approval = Assert.Single(approvals.ApprovedBy!);
            Assert.Equal(7, approval.User?.Id);
            Assert.Equal("approver", approval.User?.Username);
            Assert.Equal(new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero), approval.ApprovedAt);
        }
    }

    [Fact]
    public async Task GetApprovalsAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, ApprovalsJson));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetApprovalsAsync("gitlab-org/gitlab", 12, TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/12/approvals",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetApprovalsAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.NotFound, """{ "message": "404 Merge Request Not Found" }"""));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
                repository.GetApprovalsAsync(1, 404, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal("404 Merge Request Not Found", exception.Message);
        }
    }

    [Fact]
    public async Task ApproveAsync_WithRequest_PostsApproveRoute_WithSerializedBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, ApprovalsJson);
        });

        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            ApproveMergeRequestRequest request = new()
            {
                Sha = "9a3f2b1c", PublishReview = true, ApprovalPassword = "s3cret"
            };

            GitLabMergeRequestApprovals approvals =
                await repository.ApproveAsync(7, 12, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/7/merge_requests/12/approve",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
            Assert.Contains("\"sha\":\"9a3f2b1c\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"publish_review\":true", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"approval_password\":\"s3cret\"", sentBody, StringComparison.Ordinal);
            Assert.True(approvals.Approved);
        }
    }

    [Fact]
    public async Task ApproveAsync_WithoutRequest_SendsNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.Created, ApprovalsJson));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.ApproveAsync(7, 12, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/7/merge_requests/12/approve",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Null(handler.LastRequest?.Content);
        }
    }

    [Fact]
    public void ApproveMergeRequestRequest_ToString_DoesNotLeakTheApprovalPassword()
    {
        ApproveMergeRequestRequest request = new() { Sha = "9a3f2b1c", ApprovalPassword = "s3cret" };

        Assert.DoesNotContain("s3cret", request.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnapproveAsync_PostsUnapproveRoute_WithNoBody()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.Created, """
            { "user_has_approved": false, "user_can_approve": true, "approved": false, "approved_by": [] }
            """));

        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabMergeRequestApprovals approvals =
                await repository.UnapproveAsync(7, 12, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/7/merge_requests/12/unapprove",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Null(handler.LastRequest?.Content);
            Assert.False(approvals.UserHasApproved);
            Assert.Empty(approvals.ApprovedBy!);
        }
    }

    [Fact]
    public async Task ResetApprovalsAsync_PutsResetRoute_WithNoBody_AndToleratesAnEmptyResponse()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.ResetApprovalsAsync(7, 12, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/7/merge_requests/12/reset_approvals",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Null(handler.LastRequest?.Content);
        }
    }

    [Fact]
    public async Task ResetApprovalsAsync_WhenCalledByAHumanToken_ThrowsGitLabAuthenticationException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.Unauthorized, """{ "message": "401 Unauthorized" }"""));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabApiException exception = await Assert.ThrowsAsync<GitLabAuthenticationException>(() =>
                repository.ResetApprovalsAsync(7, 12, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        }
    }

    [Fact]
    public async Task GetApprovalStateAsync_BuildsApprovalStateRoute_AndDeserializesStateOnlyMembers()
    {
        const string StateJson = """
                                 {
                                   "approval_rules_overwritten": true,
                                   "rules": [
                                     {
                                       "id": 55,
                                       "name": "QA",
                                       "rule_type": "code_owner",
                                       "approvals_required": 1,
                                       "code_owner": true,
                                       "approved": false,
                                       "approved_by": [
                                         {
                                           "id": 7,
                                           "username": "approver",
                                           "name": "Ada Approver",
                                           "web_url": "https://gitlab.example/approver"
                                         }
                                       ]
                                     }
                                   ]
                                 }
                                 """;

        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, StateJson));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabMergeRequestApprovalState state =
                await repository.GetApprovalStateAsync(1, 12, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/12/approval_state",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            Assert.True(state.ApprovalRulesOverwritten);
            GitLabMergeRequestApprovalRule rule = Assert.Single(state.Rules!);
            Assert.Equal(55, rule.Id);
            Assert.Equal(GitLabApprovalRuleType.CodeOwner, rule.RuleType);
            Assert.True(rule.CodeOwner);
            Assert.False(rule.Approved);
            Assert.Equal("approver", Assert.Single(rule.ApprovedBy!).Username);
        }
    }

    [Fact]
    public async Task ListRulesAsync_BuildsApprovalRulesRoute_AndDeserializesRules()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, "[" + RuleJson + "]"));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            List<GitLabMergeRequestApprovalRule> rules = [];
            await foreach (GitLabMergeRequestApprovalRule item in repository.ListRulesAsync(1, 12,
                               TestContext.Current.CancellationToken))
            {
                rules.Add(item);
            }

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/12/approval_rules",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            GitLabMergeRequestApprovalRule rule = Assert.Single(rules);
            Assert.Equal(55, rule.Id);
            Assert.Equal("QA", rule.Name);
            Assert.Equal(GitLabApprovalRuleType.Regular, rule.RuleType);
            Assert.Equal(2, rule.ApprovalsRequired);
            Assert.Equal("Backend", rule.Section);
            Assert.Null(rule.ReportType);
            Assert.True(rule.Overridden);
            Assert.False(rule.ContainsHiddenGroups);
            Assert.Equal(3, rule.SourceRule?.ApprovalsRequired);
            Assert.Equal("approver", Assert.Single(rule.EligibleApprovers!).Username);
            Assert.Equal("approver", Assert.Single(rule.Users!).Username);
            Assert.Equal("Reviewers", Assert.Single(rule.Groups!).Name);

            // The three approval-state-only members stay null on the rule endpoints.
            Assert.Null(rule.CodeOwner);
            Assert.Null(rule.Approved);
            Assert.Null(rule.ApprovedBy);
        }
    }

    [Fact]
    public async Task GetRuleAsync_AddressesTheRuleById()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, RuleJson));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabMergeRequestApprovalRule rule =
                await repository.GetRuleAsync("gitlab-org/gitlab", 12, 55, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_requests/12/approval_rules/55",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("QA", rule.Name);
        }
    }

    [Fact]
    public async Task CreateRuleAsync_PostsSerializedBody_AndDeserializesCreatedRule()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, RuleJson);
        });

        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            CreateMergeRequestApprovalRuleRequest request = new()
            {
                Name = "QA",
                ApprovalsRequired = 2,
                ApprovalProjectRuleId = 9,
                UserIds = [7, 8],
                GroupIds = [90],
                Usernames = ["approver"]
            };

            GitLabMergeRequestApprovalRule rule =
                await repository.CreateRuleAsync(1, 12, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/12/approval_rules",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Contains("\"name\":\"QA\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"approvals_required\":2", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"approval_project_rule_id\":9", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"user_ids\":[7,8]", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"group_ids\":[90]", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"usernames\":[\"approver\"]", sentBody, StringComparison.Ordinal);
            Assert.Equal(55, rule.Id);
        }
    }

    [Fact]
    public async Task CreateRuleAsync_OnValidationFailure_ThrowsGitLabValidationException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.BadRequest, """{ "message": { "name": ["has already been taken"] } }"""));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            CreateMergeRequestApprovalRuleRequest request = new() { Name = "QA", ApprovalsRequired = 0 };

            GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
                repository.CreateRuleAsync(1, 12, request, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        }
    }

    [Fact]
    public async Task UpdateRuleAsync_PutsSerializedBody_AndOmitsUnsetMembers()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.OK, RuleJson);
        });

        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            UpdateMergeRequestApprovalRuleRequest request = new()
            {
                ApprovalsRequired = 3, UserIds = [7], RemoveHiddenGroups = true
            };

            GitLabMergeRequestApprovalRule rule =
                await repository.UpdateRuleAsync(1, 12, 55, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/12/approval_rules/55",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Contains("\"approvals_required\":3", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"user_ids\":[7]", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"remove_hidden_groups\":true", sentBody, StringComparison.Ordinal);
            Assert.DoesNotContain("\"name\"", sentBody, StringComparison.Ordinal);
            Assert.Equal(2, rule.ApprovalsRequired);
        }
    }

    [Fact]
    public async Task DeleteRuleAsync_SendsDeleteToTheRuleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeleteRuleAsync(1, 12, 55, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_requests/12/approval_rules/55",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task DeleteRuleAsync_WhenForbidden_ThrowsGitLabForbiddenException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.Forbidden, """{ "message": "403 Forbidden" }"""));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
                repository.DeleteRuleAsync(1, 12, 55, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        }
    }

    [Fact]
    public async Task GetApprovalConfigurationAsync_BuildsProjectApprovalsRoute_AndDeserializesConfiguration()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, ApprovalConfigurationJson));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabProjectApprovalConfiguration configuration =
                await repository.GetApprovalConfigurationAsync("gitlab-org/gitlab",
                    TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/approvals",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            Assert.Equal(2, configuration.ApprovalsBeforeMerge);
            Assert.True(configuration.ResetApprovalsOnPush);
            Assert.False(configuration.SelectiveCodeOwnerRemovals);
            Assert.True(configuration.DisableOverridingApproversPerMergeRequest);
            Assert.False(configuration.MergeRequestsAuthorApproval);
            Assert.True(configuration.MergeRequestsDisableCommittersApproval);
            Assert.False(configuration.RequirePasswordToApprove);
            Assert.True(configuration.RequireReauthenticationToApprove);

            // Both collections wrap their payload in a single-member object rather than projecting it.
            Assert.Equal("approver", Assert.Single(configuration.Approvers!).User?.Username);
            Assert.Equal("Reviewers", Assert.Single(configuration.ApproverGroups!).Group?.Name);
        }
    }

    [Fact]
    public async Task SetApprovalConfigurationAsync_PostsOnlyTheFieldsThatWereSet()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, ApprovalConfigurationJson);
        });

        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            SetApprovalConfigurationRequest request = new()
            {
                ApprovalsBeforeMerge = 2, ResetApprovalsOnPush = true, MergeRequestsAuthorApproval = false
            };

            GitLabProjectApprovalConfiguration configuration =
                await repository.SetApprovalConfigurationAsync(1, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/approvals",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
            Assert.Contains("""
                            "approvals_before_merge":2
                            """, sentBody, StringComparison.Ordinal);
            Assert.Contains("""
                            "reset_approvals_on_push":true
                            """, sentBody, StringComparison.Ordinal);

            // false is a value, not an absence - it must survive DefaultIgnoreCondition.WhenWritingNull.
            Assert.Contains("""
                            "merge_requests_author_approval":false
                            """, sentBody, StringComparison.Ordinal);
            Assert.DoesNotContain("require_password_to_approve", sentBody, StringComparison.Ordinal);
            Assert.Equal(2, configuration.ApprovalsBeforeMerge);
        }
    }

    [Fact]
    public async Task GetSettingsForProjectAsync_BuildsSingularSettingRoute_AndDeserializesSettings()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, ApprovalSettingJson));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabMergeRequestApprovalSetting setting =
                await repository.GetSettingsForProjectAsync(1, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/merge_request_approval_setting",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            Assert.True(setting.AllowAuthorApproval);
            Assert.False(setting.AllowCommitterApproval);
            Assert.True(setting.AllowOverridesToApproverListPerMergeRequest);
            Assert.False(setting.RetainApprovalsOnPush);
            Assert.True(setting.SelectiveCodeOwnerRemovals);
            Assert.False(setting.RequirePasswordToApprove);
            Assert.True(setting.RequireReauthenticationToApprove);
        }
    }

    [Fact]
    public async Task UpdateSettingsForProjectAsync_PutsOnlyTheFieldsThatWereSet()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.OK, ApprovalSettingJson);
        });

        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            UpdateMergeRequestApprovalSettingRequest request = new()
            {
                AllowAuthorApproval = true, RetainApprovalsOnPush = false
            };

            GitLabMergeRequestApprovalSetting setting =
                await repository.UpdateSettingsForProjectAsync("gitlab-org/gitlab", request,
                    TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_request_approval_setting",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("""{"allow_author_approval":true,"retain_approvals_on_push":false}""", sentBody);
            Assert.True(setting.AllowAuthorApproval);
        }
    }

    [Fact]
    public async Task GetSettingsForGroupAsync_BuildsGroupSettingRoute_AndEncodesNamespacedPath()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, ApprovalSettingJson));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabMergeRequestApprovalSetting setting =
                await repository.GetSettingsForGroupAsync("gitlab-org/subgroup",
                    TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal(
                "https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/merge_request_approval_setting",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.True(setting.SelectiveCodeOwnerRemovals);
        }
    }

    [Fact]
    public async Task UpdateSettingsForGroupAsync_PutsToTheGroupSettingRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.OK, ApprovalSettingJson);
        });

        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            UpdateMergeRequestApprovalSettingRequest request = new()
            {
                AllowOverridesToApproverListPerMergeRequest = false, RequireReauthenticationToApprove = true
            };

            await repository.UpdateSettingsForGroupAsync(55, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/groups/55/merge_request_approval_setting",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Contains("""
                            "allow_overrides_to_approver_list_per_merge_request":false
                            """, sentBody, StringComparison.Ordinal);
            Assert.Contains("""
                            "require_reauthentication_to_approve":true
                            """, sentBody, StringComparison.Ordinal);
            Assert.DoesNotContain("allow_author_approval", sentBody, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task UpdateSettingsForGroupAsync_WhenNotAGroupAdministrator_ThrowsGitLabForbiddenException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.Forbidden, """{ "message": "403 Forbidden" }"""));
        MergeRequestApprovalsClient repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
                repository.UpdateSettingsForGroupAsync(55, new UpdateMergeRequestApprovalSettingRequest(),
                    TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        }
    }

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static MergeRequestApprovalsClient CreateRepository(StubHttpMessageHandler handler,
        out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        return new MergeRequestApprovalsClient(new GitLabApiConnection(httpClient));
    }
}