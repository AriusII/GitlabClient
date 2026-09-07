using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class MemberRolesRepositoryTests
{
    // Every ability GitLab's MemberRole entity declares, set to true. Deserializing this and asserting that no
    // member came back null is what proves each C# property name snake_cases onto the wire name the spec uses:
    // a mismatch surfaces as a silent null, never as an exception.
    private const string MemberRoleJson = """
                                          {
                                            "id": 2,
                                            "group_id": 345,
                                            "name": "Compliance auditor",
                                            "description": "Reads code and compliance data",
                                            "base_access_level": 20,
                                            "apply_security_scan_profiles": true,
                                            "admin_merge_request": true,
                                            "archive_project": true,
                                            "admin_ai_catalog_item_consumer": true,
                                            "create_security_scan_profiles": true,
                                            "destroy_package": true,
                                            "remove_project": true,
                                            "delete_security_scan_profiles": true,
                                            "remove_group": true,
                                            "manage_security_policy_link": true,
                                            "admin_ai_catalog_item": true,
                                            "admin_compliance_framework": true,
                                            "admin_cicd_variables": true,
                                            "manage_deploy_tokens": true,
                                            "manage_group_access_tokens": true,
                                            "admin_group_member": true,
                                            "admin_integrations": true,
                                            "manage_merge_request_settings": true,
                                            "manage_project_access_tokens": true,
                                            "admin_protected_branch": true,
                                            "admin_protected_environments": true,
                                            "admin_push_rules": true,
                                            "admin_runners": true,
                                            "admin_security_attributes": true,
                                            "admin_terraform_state": true,
                                            "admin_vulnerability": true,
                                            "admin_web_hook": true,
                                            "read_agent_artifacts": true,
                                            "read_compliance_dashboard": true,
                                            "read_security_scan_profiles": true,
                                            "read_virtual_registry": true,
                                            "update_sec_ai_workflow_settings": true,
                                            "update_security_scan_profiles": true,
                                            "read_admin_cicd": true,
                                            "read_crm_contact": true,
                                            "read_dependency": true,
                                            "read_admin_groups": true,
                                            "read_admin_projects": true,
                                            "read_code": true,
                                            "read_runners": true,
                                            "read_security_attribute": true,
                                            "read_admin_subscription": true,
                                            "read_admin_monitoring": true,
                                            "read_admin_users": true,
                                            "read_vulnerability": true
                                          }
                                          """;

    [Fact]
    public async Task ListAsync_BuildsTheInstanceRoute_AndMapsEveryAbilityFlag()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{MemberRoleJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabMemberRole> roles = [];
        await foreach (GitLabMemberRole item in repository.ListAsync(TestContext.Current.CancellationToken))
        {
            roles.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/member_roles", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabMemberRole role = Assert.Single(roles);
        Assert.Equal(2, role.Id);
        Assert.Equal(345, role.GroupId);
        Assert.Equal("Compliance auditor", role.Name);
        Assert.Equal("Reads code and compliance data", role.Description);

        // base_access_level is GitLab's numeric role ladder and stays numeric on the wire: 20 is Reporter.
        Assert.Equal(20, role.BaseAccessLevel);

        (string Wire, bool? Value)[] abilities =
        [
            ("apply_security_scan_profiles", role.ApplySecurityScanProfiles),
            ("admin_merge_request", role.AdminMergeRequest),
            ("archive_project", role.ArchiveProject),
            ("admin_ai_catalog_item_consumer", role.AdminAiCatalogItemConsumer),
            ("create_security_scan_profiles", role.CreateSecurityScanProfiles),
            ("destroy_package", role.DestroyPackage),
            ("remove_project", role.RemoveProject),
            ("delete_security_scan_profiles", role.DeleteSecurityScanProfiles),
            ("remove_group", role.RemoveGroup),
            ("manage_security_policy_link", role.ManageSecurityPolicyLink),
            ("admin_ai_catalog_item", role.AdminAiCatalogItem),
            ("admin_compliance_framework", role.AdminComplianceFramework),
            ("admin_cicd_variables", role.AdminCicdVariables),
            ("manage_deploy_tokens", role.ManageDeployTokens),
            ("manage_group_access_tokens", role.ManageGroupAccessTokens),
            ("admin_group_member", role.AdminGroupMember),
            ("admin_integrations", role.AdminIntegrations),
            ("manage_merge_request_settings", role.ManageMergeRequestSettings),
            ("manage_project_access_tokens", role.ManageProjectAccessTokens),
            ("admin_protected_branch", role.AdminProtectedBranch),
            ("admin_protected_environments", role.AdminProtectedEnvironments),
            ("admin_push_rules", role.AdminPushRules),
            ("admin_runners", role.AdminRunners),
            ("admin_security_attributes", role.AdminSecurityAttributes),
            ("admin_terraform_state", role.AdminTerraformState),
            ("admin_vulnerability", role.AdminVulnerability),
            ("admin_web_hook", role.AdminWebHook),
            ("read_agent_artifacts", role.ReadAgentArtifacts),
            ("read_compliance_dashboard", role.ReadComplianceDashboard),
            ("read_security_scan_profiles", role.ReadSecurityScanProfiles),
            ("read_virtual_registry", role.ReadVirtualRegistry),
            ("update_sec_ai_workflow_settings", role.UpdateSecAiWorkflowSettings),
            ("update_security_scan_profiles", role.UpdateSecurityScanProfiles),
            ("read_admin_cicd", role.ReadAdminCicd),
            ("read_crm_contact", role.ReadCrmContact),
            ("read_dependency", role.ReadDependency),
            ("read_admin_groups", role.ReadAdminGroups),
            ("read_admin_projects", role.ReadAdminProjects),
            ("read_code", role.ReadCode),
            ("read_runners", role.ReadRunners),
            ("read_security_attribute", role.ReadSecurityAttribute),
            ("read_admin_subscription", role.ReadAdminSubscription),
            ("read_admin_monitoring", role.ReadAdminMonitoring),
            ("read_admin_users", role.ReadAdminUsers),
            ("read_vulnerability", role.ReadVulnerability)
        ];

        Assert.All(abilities, ability => Assert.True(ability.Value, ability.Wire));
    }

    [Fact]
    public async Task ListForGroupAsync_EncodesTheNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{MemberRoleJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        List<GitLabMemberRole> roles = [];
        await foreach (GitLabMemberRole item in repository.ListForGroupAsync("parent-group/subgroup",
                           TestContext.Current.CancellationToken))
        {
            roles.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/member_roles",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Compliance auditor", Assert.Single(roles).Name);
    }

    [Fact]
    public async Task ListAdminRolesAsync_BuildsTheAdminRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        await foreach (GitLabMemberRole _ in repository.ListAdminRolesAsync(TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        // The admin roles are their own collection, not /member_roles with a filter.
        Assert.Equal("https://gitlab.example/api/v4/admin_member_roles",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateForGroupAsync_SendsBaseAccessLevelAsANumber_AndOmitsTheUnsetAbilities()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MemberRoleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        CreateMemberRoleRequest request = new()
        {
            BaseAccessLevel = 20,
            Name = "Compliance auditor",
            Description = "Reads code and compliance data",
            AdminCicdVariables = true,
            ReadCode = true,
            ReadVulnerability = false
        };

        GitLabMemberRole role =
            await repository.CreateForGroupAsync(345, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/345/member_roles",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        // base_access_level must go out as 20, not "20": GitLab types it as an integer enum. An ability the
        // caller did not mention is omitted rather than sent as false, so it keeps GitLab's own default.
        Assert.Equal(
            """
            {"base_access_level":20,"name":"Compliance auditor","description":"Reads code and compliance data","admin_cicd_variables":true,"read_code":true,"read_vulnerability":false}
            """,
            sentBody);

        Assert.Equal(2, role.Id);
    }

    [Fact]
    public async Task CreateAsync_PostsToTheInstanceRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(MemberRoleJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.CreateAsync(new CreateMemberRoleRequest { BaseAccessLevel = 10, ReadCode = true },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/member_roles", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAdminRoleAsync_PostsOnlyTheAdminAbilities()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MemberRoleJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        CreateAdminMemberRoleRequest request = new() { Name = "Auditor", ReadAdminCicd = true, ReadAdminUsers = true };

        await repository.CreateAdminRoleAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin_member_roles",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        // An admin role carries no base_access_level - the request type does not even offer one.
        Assert.Equal("""{"name":"Auditor","read_admin_cicd":true,"read_admin_users":true}""", sentBody);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheInstanceRoleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/member_roles/2", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteForGroupAsync_EncodesTheNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteForGroupAsync("parent-group/subgroup", 2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/groups/parent-group%2Fsubgroup/member_roles/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAdminRoleAsync_SendsDeleteToTheAdminRoleRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteAdminRoleAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin_member_roles/2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListAsync_OnANonUltimateInstance_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabMemberRole _ in repository.ListAsync(TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                Assert.Fail("The stubbed response is a 403.");
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task DeleteForGroupAsync_OnARoleStillInUse_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "base": ["Role is assigned to one or more members"] } }""";

        using StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        MemberRolesRepository repository = new(new GitLabApiConnection(httpClient));

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.DeleteForGroupAsync(345, 2, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("assigned to one or more members", exception.Message, StringComparison.Ordinal);
    }
}