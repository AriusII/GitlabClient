using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Tests.Endpoints;

/// <summary>
///     Pins the wire names <see cref="Serialization.GitLabJsonContext" /> resolves for the two large group request
///     bodies against the parameter names the vendored spec declares.
///     <para>
///         Worth its own test because these two records carry 110 members between them and lean entirely
///         on the context's snake_case naming policy rather than per-property
///         <c>[JsonPropertyName]</c> attributes. GitLab runs Grape, which answers an unrecognised
///         parameter with a 200 and simply ignores it - so a name the policy derives differently from
///         GitLab's spelling would silently fail to apply a setting, and nothing else in the suite would
///         notice.
///     </para>
/// </summary>
public sealed class GroupRequestSerializationTests
{
    private static readonly string[] CreateGroupParameterNames =
    [
        "name", "path", "parent_id", "organization_id", "description", "visibility", "share_with_group_lock",
        "require_two_factor_authentication", "two_factor_grace_period", "project_creation_level",
        "auto_devops_enabled", "subgroup_creation_level", "emails_enabled", "show_diff_preview_in_email",
        "mentions_disabled", "lfs_enabled", "request_access_enabled", "default_branch",
        "default_branch_protection", "default_branch_protection_defaults", "enabled_git_access_protocol",
        "crm_enabled", "resource_access_token_notify_inherited",
        "lock_resource_access_token_notify_inherited", "membership_lock", "ldap_cn", "ldap_access",
        "shared_runners_minutes_limit", "extra_shared_runners_minutes_limit", "wiki_access_level",
        "duo_availability", "duo_remote_flows_availability", "duo_foundational_flows_availability",
        "duo_custom_agents_availability", "duo_custom_flows_availability",
        "duo_external_agents_availability", "tool_approval_for_session_availability",
        "amazon_q_auto_review_enabled", "experiment_features_enabled", "model_prompt_cache_enabled",
        "foundational_agents_statuses", "ai_settings_attributes"
    ];

    private static readonly string[] UpdateGroupParameterNames =
    [
        "name", "path", "shared_runners_setting", "description", "visibility", "share_with_group_lock",
        "require_two_factor_authentication", "two_factor_grace_period", "project_creation_level",
        "auto_devops_enabled", "subgroup_creation_level", "emails_enabled", "show_diff_preview_in_email",
        "mentions_disabled", "lfs_enabled", "request_access_enabled", "default_branch",
        "default_branch_protection", "default_branch_protection_defaults", "enabled_git_access_protocol",
        "crm_enabled", "resource_access_token_notify_inherited",
        "lock_resource_access_token_notify_inherited", "membership_lock", "ldap_cn", "ldap_access",
        "shared_runners_minutes_limit", "extra_shared_runners_minutes_limit", "wiki_access_level",
        "duo_availability", "duo_remote_flows_availability", "duo_foundational_flows_availability",
        "duo_custom_agents_availability", "duo_custom_flows_availability",
        "duo_external_agents_availability", "tool_approval_for_session_availability",
        "amazon_q_auto_review_enabled", "experiment_features_enabled", "model_prompt_cache_enabled",
        "foundational_agents_statuses", "ai_settings_attributes",
        "prevent_sharing_groups_outside_hierarchy", "step_up_auth_required_oauth_provider",
        "lock_math_rendering_limits_enabled", "math_rendering_limits_enabled", "max_artifacts_size",
        "file_template_project_id", "prevent_forking_outside_group", "unique_project_download_limit",
        "unique_project_download_limit_interval_in_seconds", "unique_project_download_limit_allowlist",
        "unique_project_download_limit_alertlist", "auto_ban_user_on_excessive_projects_download",
        "ip_restriction_ranges", "allowed_email_domains_list", "service_access_tokens_expiration_enforced",
        "duo_core_features_enabled", "duo_features_enabled", "lock_duo_features_enabled",
        "auto_duo_code_review_enabled", "ai_audit_events_storage_enabled",
        "lock_ai_audit_events_storage_enabled", "web_based_commit_signing_enabled",
        "only_allow_merge_if_pipeline_succeeds", "allow_merge_on_skipped_pipeline",
        "only_allow_merge_if_all_discussions_are_resolved", "enabled_foundational_flows",
        "duo_template_project_id", "allow_personal_snippets", "duo_namespace_access_rules",
        "built_in_project_templates_enabled",
        "lock_built_in_project_templates_enabled", "create_code_review_flow_consent"
    ];

    /// <summary>
    ///     The binary <c>avatar</c> field (sent through <c>SetAvatarAsync</c> instead) and the deprecated
    ///     <c>emails_disabled</c> parameter are deliberately absent from both records.
    /// </summary>
    public static TheoryData<string[], string> RequestBodies =>
        new()
        {
            { CreateGroupParameterNames, "CreateGroupRequest" }, { UpdateGroupParameterNames, "UpdateGroupRequest" }
        };

    [Theory]
    [MemberData(nameof(RequestBodies))]
    public void EveryRequestMember_SerializesUnderTheNameTheSpecDeclares(string[] expected, string typeName)
    {
        JsonTypeInfo typeInfo = typeName == "CreateGroupRequest"
            ? GitLabJsonContext.Default.CreateGroupRequest
            : GitLabJsonContext.Default.UpdateGroupRequest;

        List<string> actual = typeInfo.Properties.Select(static property => property.Name).ToList();
        actual.Sort(StringComparer.Ordinal);

        List<string> sortedExpected = [.. expected];
        sortedExpected.Sort(StringComparer.Ordinal);

        Assert.Equal(sortedExpected, actual);
    }

    [Fact]
    public void CreateGroupRequest_SerializesTheGitLab19_4AiFields()
    {
        CreateGroupRequest request = new()
        {
            Name = "Platform",
            Path = "platform",
            FoundationalAgentsStatuses =
            [
                new GitLabFoundationalAgentStatus { Reference = "duo_agent", Enabled = true }
            ],
            AiSettingsAttributes = new GitLabAiSettingsAttributes
            {
                DuoAgentPlatformEnabled = true, MinimumAccessLevelManage = 40
            }
        };

        using JsonDocument document = JsonDocument.Parse(
            JsonSerializer.Serialize(request, GitLabJsonContext.Default.CreateGroupRequest));

        JsonElement agent = document.RootElement.GetProperty("foundational_agents_statuses")[0];
        JsonElement settings = document.RootElement.GetProperty("ai_settings_attributes");
        Assert.Equal("duo_agent", agent.GetProperty("reference").GetString());
        Assert.True(agent.GetProperty("enabled").GetBoolean());
        Assert.True(settings.GetProperty("duo_agent_platform_enabled").GetBoolean());
        Assert.Equal(40, settings.GetProperty("minimum_access_level_manage").GetInt32());
    }
}