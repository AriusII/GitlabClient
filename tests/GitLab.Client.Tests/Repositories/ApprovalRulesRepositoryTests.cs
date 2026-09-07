using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class ApprovalRulesRepositoryTests
{
    private const string ProjectRuleJson = """
                                           {
                                             "id": 12,
                                             "name": "Security",
                                             "rule_type": "report_approver",
                                             "approvals_required": 2,
                                             "contains_hidden_groups": false,
                                             "report_type": "code_coverage",
                                             "applies_to_all_protected_branches": false,
                                             "coverage_minimum_threshold": 82.5,
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
                                             ],
                                             "protected_branches": [
                                               {
                                                 "id": 3,
                                                 "name": "main",
                                                 "allow_force_push": false,
                                                 "push_access_levels": [
                                                   { "access_level": 40, "access_level_description": "Maintainers" }
                                                 ]
                                               }
                                             ]
                                           }
                                           """;

    private const string GroupRuleJson = """
                                         {
                                           "id": 21,
                                           "name": "Group policy",
                                           "rule_type": "regular",
                                           "approvals_required": 1,
                                           "contains_hidden_groups": false,
                                           "applies_to_all_protected_branches": true,
                                           "protected_branches": []
                                         }
                                         """;

    private const string SettingsRuleJson = """
                                            {
                                              "id": 3,
                                              "name": "Security",
                                              "rule_type": "report_approver",
                                              "approvals_required": 2,
                                              "contains_hidden_groups": false,
                                              "report_type": "code_coverage",
                                              "applies_to_all_protected_branches": false,
                                              "coverage_minimum_threshold": 82.5,
                                              "vulnerabilities_allowed": 0,
                                              "scanners": ["sast"],
                                              "severity_levels": ["high"],
                                              "vulnerability_states": ["detected"],
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
                                              ],
                                              "approvers": [
                                                {
                                                  "id": 8,
                                                  "username": "expanded",
                                                  "name": "Group Member",
                                                  "web_url": "https://gitlab.example/expanded"
                                                }
                                              ],
                                              "protected_branches": [
                                                { "id": 3, "name": "main", "allow_force_push": false }
                                              ]
                                            }
                                            """;

    private const string SettingsJson =
        $$"""
          { "rules": [{{SettingsRuleJson}}], "fallback_approvals_required": 1 }
          """;

    [Fact]
    public async Task ListForProjectAsync_BuildsProjectApprovalRulesRoute_AndDeserializesRules()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, "[" + ProjectRuleJson + "]"));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            List<GitLabApprovalRule> rules = [];
            await foreach (GitLabApprovalRule item in repository.ListForProjectAsync(1,
                               TestContext.Current.CancellationToken))
            {
                rules.Add(item);
            }

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/approval_rules",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            GitLabApprovalRule rule = Assert.Single(rules);
            Assert.Equal(12, rule.Id);
            Assert.Equal("Security", rule.Name);
            Assert.Equal(GitLabApprovalRuleType.ReportApprover, rule.RuleType);
            Assert.Equal(2, rule.ApprovalsRequired);
            Assert.Equal("code_coverage", rule.ReportType);
            Assert.False(rule.ContainsHiddenGroups);
            Assert.False(rule.AppliesToAllProtectedBranches);
            Assert.Equal(82.5d, rule.CoverageMinimumThreshold);
            Assert.Equal("approver", Assert.Single(rule.EligibleApprovers!).Username);
            Assert.Equal("approver", Assert.Single(rule.Users!).Username);
            Assert.Equal("Reviewers", Assert.Single(rule.Groups!).Name);

            GitLabProtectedBranch branch = Assert.Single(rule.ProtectedBranches!);
            Assert.Equal("main", branch.Name);
            Assert.Equal(40, Assert.Single(branch.PushAccessLevels!).AccessLevel);
        }
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, "[]"));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await foreach (GitLabApprovalRule _ in repository.ListForProjectAsync("gitlab-org/gitlab",
                               TestContext.Current.CancellationToken))
            {
                // Draining the sequence is what issues the request.
            }

            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/approval_rules",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetForProjectAsync_AddressesTheRuleById()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, ProjectRuleJson));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabApprovalRule rule =
                await repository.GetForProjectAsync("gitlab-org/gitlab", 12, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/approval_rules/12",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("Security", rule.Name);
        }
    }

    [Fact]
    public async Task GetForProjectAsync_OnNotFound_ThrowsGitLabNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.NotFound, """{ "message": "404 Approval Rule Not Found" }"""));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
                repository.GetForProjectAsync(1, 999, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal("404 Approval Rule Not Found", exception.Message);
        }
    }

    [Fact]
    public async Task CreateForProjectAsync_PostsSerializedBody_AndDeserializesCreatedRule()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, ProjectRuleJson);
        });

        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            CreateApprovalRuleRequest request = new()
            {
                Name = "Security",
                ApprovalsRequired = 2,
                RuleType = GitLabApprovalRuleType.ReportApprover,
                ReportType = "code_coverage",
                UserIds = [7],
                GroupIds = [90],
                Usernames = ["approver"],
                ProtectedBranchIds = [3],
                AppliesToAllProtectedBranches = false,
                CoverageMinimumThreshold = 82.5d,
                VulnerabilitiesAllowed = 0,
                Scanners = ["sast"],
                SeverityLevels = ["high"],
                VulnerabilityStates = ["detected"]
            };

            GitLabApprovalRule rule =
                await repository.CreateForProjectAsync(1, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/approval_rules",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
            Assert.Contains("\"name\":\"Security\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"approvals_required\":2", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"rule_type\":\"report_approver\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"protected_branch_ids\":[3]", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"coverage_minimum_threshold\":82.5", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"vulnerability_states\":[\"detected\"]", sentBody, StringComparison.Ordinal);
            Assert.Equal(12, rule.Id);
        }
    }

    [Fact]
    public async Task UpdateForProjectAsync_PutsSerializedBody_AndOmitsUnsetMembers()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.OK, ProjectRuleJson);
        });

        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            UpdateApprovalRuleRequest request = new()
            {
                ApprovalsRequired = 3, UserIds = [7], RemoveHiddenGroups = true
            };

            GitLabApprovalRule rule =
                await repository.UpdateForProjectAsync(1, 12, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/approval_rules/12",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Contains("\"approvals_required\":3", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"user_ids\":[7]", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"remove_hidden_groups\":true", sentBody, StringComparison.Ordinal);
            Assert.DoesNotContain("\"name\"", sentBody, StringComparison.Ordinal);
            Assert.DoesNotContain("\"group_ids\"", sentBody, StringComparison.Ordinal);
            Assert.Equal("Security", rule.Name);
        }
    }

    [Fact]
    public async Task DeleteForProjectAsync_SendsDeleteToTheRuleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeleteForProjectAsync(1, 12, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/approval_rules/12",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupRoute_AndLeavesCoverageThresholdNull()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, "[" + GroupRuleJson + "]"));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            List<GitLabApprovalRule> rules = [];
            await foreach (GitLabApprovalRule item in repository.ListForGroupAsync("gitlab-org/subgroup",
                               TestContext.Current.CancellationToken))
            {
                rules.Add(item);
            }

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/approval_rules",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            GitLabApprovalRule rule = Assert.Single(rules);
            Assert.Equal(21, rule.Id);
            Assert.Equal("Group policy", rule.Name);
            Assert.True(rule.AppliesToAllProtectedBranches);
            Assert.Empty(rule.ProtectedBranches!);
            Assert.Null(rule.CoverageMinimumThreshold);
        }
    }

    [Fact]
    public async Task CreateForGroupAsync_PostsToTheGroupRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, GroupRuleJson);
        });

        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            CreateApprovalRuleRequest request = new()
            {
                Name = "Group policy", ApprovalsRequired = 1, UserIds = [7], GroupIds = [90]
            };

            GitLabApprovalRule rule =
                await repository.CreateForGroupAsync(55, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/groups/55/approval_rules",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Contains("\"name\":\"Group policy\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"approvals_required\":1", sentBody, StringComparison.Ordinal);

            // The project-only members are simply absent from a group call.
            Assert.DoesNotContain("\"protected_branch_ids\"", sentBody, StringComparison.Ordinal);
            Assert.DoesNotContain("\"coverage_minimum_threshold\"", sentBody, StringComparison.Ordinal);
            Assert.Equal(21, rule.Id);
        }
    }

    [Fact]
    public async Task UpdateForGroupAsync_PutsToTheGroupRuleRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.OK, GroupRuleJson);
        });

        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            UpdateApprovalRuleRequest request = new() { Name = "Group policy", ApprovalsRequired = 2 };

            GitLabApprovalRule rule =
                await repository.UpdateForGroupAsync(55, 21, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/groups/55/approval_rules/21",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Contains("\"name\":\"Group policy\"", sentBody, StringComparison.Ordinal);
            Assert.Contains("\"approvals_required\":2", sentBody, StringComparison.Ordinal);
            Assert.Equal("Group policy", rule.Name);
        }
    }

    [Fact]
    public async Task CreateForGroupAsync_WhenNotAGroupAdministrator_ThrowsGitLabForbiddenException()
    {
        using StubHttpMessageHandler handler = new(_ =>
            Json(HttpStatusCode.Forbidden, """{ "message": "403 Forbidden" }"""));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            CreateApprovalRuleRequest request = new() { Name = "Group policy", ApprovalsRequired = 0 };

            GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
                repository.CreateForGroupAsync(55, request, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        }
    }

    [Fact]
    public async Task GetSettingsForProjectAsync_BuildsSettingsRoute_AndDeserializesRulesAndFallback()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, SettingsJson));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabProjectApprovalSettings settings =
                await repository.GetSettingsForProjectAsync(1, cancellationToken: TestContext.Current
                    .CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);

            // No target branch was supplied, so no query string is appended at all.
            Assert.Equal("https://gitlab.example/api/v4/projects/1/approval_settings",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            Assert.Equal(1, settings.FallbackApprovalsRequired);

            GitLabProjectApprovalSettingRule rule = Assert.Single(settings.Rules!);
            Assert.Equal(3, rule.Id);
            Assert.Equal("Security", rule.Name);
            Assert.Equal(GitLabApprovalRuleType.ReportApprover, rule.RuleType);
            Assert.Equal(2, rule.ApprovalsRequired);
            Assert.Equal("code_coverage", rule.ReportType);
            Assert.Equal(82.5d, rule.CoverageMinimumThreshold);
            Assert.Equal(0, rule.VulnerabilitiesAllowed);
            Assert.Equal("sast", Assert.Single(rule.Scanners!));
            Assert.Equal("high", Assert.Single(rule.SeverityLevels!));
            Assert.Equal("detected", Assert.Single(rule.VulnerabilityStates!));
            Assert.Equal("approver", Assert.Single(rule.Users!).Username);
            Assert.Equal("Reviewers", Assert.Single(rule.Groups!).Name);

            // "approvers" is this shape's name for the expanded approver list; the /approval_rules shape
            // calls the same idea "eligible_approvers".
            Assert.Equal("expanded", Assert.Single(rule.Approvers!).Username);
            Assert.Equal("main", Assert.Single(rule.ProtectedBranches!).Name);
        }
    }

    [Fact]
    public async Task GetSettingsForProjectAsync_EncodesNamespacedPathAndTargetBranch()
    {
        using StubHttpMessageHandler handler = new(_ => Json(HttpStatusCode.OK, SettingsJson));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.GetSettingsForProjectAsync("gitlab-org/gitlab", "release/1.0",
                TestContext.Current.CancellationToken);

            // The branch name carries a slash: it must be percent-encoded in the query, exactly as the
            // namespaced project path is in the path.
            Assert.Equal(
                "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/approval_settings"
                + "?target_branch=release%2F1.0",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task CreateSettingsRuleForProjectAsync_PostsToTheSettingsRulesRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.Created, SettingsRuleJson);
        });

        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            CreateApprovalRuleRequest request = new()
            {
                Name = "Security",
                ApprovalsRequired = 2,
                RuleType = GitLabApprovalRuleType.ReportApprover,
                ReportType = "code_coverage",
                UserIds = [7]
            };

            GitLabProjectApprovalSettingRule rule =
                await repository.CreateSettingsRuleForProjectAsync(1, request,
                    TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/approval_settings/rules",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            // The rule type is projected through its [JsonStringEnumMemberName], not its C# name.
            Assert.Contains("""
                            "rule_type":"report_approver"
                            """, sentBody, StringComparison.Ordinal);
            Assert.Equal(3, rule.Id);
        }
    }

    [Fact]
    public async Task UpdateSettingsRuleForProjectAsync_PutsToTheSettingsRuleRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return Json(HttpStatusCode.OK, SettingsRuleJson);
        });

        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            UpdateApprovalRuleRequest request = new() { ApprovalsRequired = 3, RemoveHiddenGroups = true };

            GitLabProjectApprovalSettingRule rule =
                await repository.UpdateSettingsRuleForProjectAsync(1, 3, request,
                    TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/1/approval_settings/rules/3",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Contains("""
                            "approvals_required":3
                            """, sentBody, StringComparison.Ordinal);
            Assert.DoesNotContain("\"name\"", sentBody, StringComparison.Ordinal);
            Assert.Equal("Security", rule.Name);
        }
    }

    [Fact]
    public async Task DeleteSettingsRuleForProjectAsync_SendsDeleteToTheSettingsRuleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        ApprovalRulesRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeleteSettingsRuleForProjectAsync("gitlab-org/gitlab", 3,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/approval_settings/rules/3",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static ApprovalRulesRepository CreateRepository(StubHttpMessageHandler handler, out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        return new ApprovalRulesRepository(new GitLabApiConnection(httpClient));
    }
}