using System.Text.Json;
using System.Text.Json.Serialization;

using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Tests.Contracts.Groups;

/// <summary>
///     Pins the nested GitLab 19.4 group-update shapes so the snake_case policy and null omission cannot
///     silently turn a valid setting update into a no-op.
/// </summary>
public sealed class UpdateGroupRequestSerializationTests
{
    [Fact]
    public void Serialize_MapsEveryAiSettingsAttributeToItsGitLab19WireName()
    {
        UpdateGroupRequest request = new()
        {
            AiSettingsAttributes = new GitLabAiSettingsAttributes
            {
                DuoAgentPlatformEnabled = true,
                DuoWorkflowMcpEnabled = true,
                AiUsageDataCollectionEnabled = false,
                AiCatalogRestrictedToGroupHierarchy = true,
                FoundationalAgentsDefaultEnabled = false,
                PromptInjectionProtectionLevel = GitLabPromptInjectionProtectionLevel.LogOnly,
                IncludeRecommendedAllowed = true,
                AllowAllUnixSockets = false,
                AllowProjectExtension = true,
                MinimumAccessLevelExecute = 10,
                MinimumAccessLevelExecuteAsync = 30,
                MinimumAccessLevelManage = 40,
                MinimumAccessLevelEnableOnProjects = 50
            }
        };

        using JsonDocument document = JsonDocument.Parse(
            JsonSerializer.Serialize(request, UpdateGroupRequestTestJsonContext.Default.UpdateGroupRequest));
        JsonElement settings = document.RootElement.GetProperty("ai_settings_attributes");

        Assert.Equal(
            [
                "ai_catalog_restricted_to_group_hierarchy", "ai_usage_data_collection_enabled",
                "allow_all_unix_sockets", "allow_project_extension", "duo_agent_platform_enabled",
                "duo_workflow_mcp_enabled", "foundational_agents_default_enabled", "include_recommended_allowed",
                "minimum_access_level_enable_on_projects", "minimum_access_level_execute",
                "minimum_access_level_execute_async", "minimum_access_level_manage",
                "prompt_injection_protection_level"
            ],
            settings.EnumerateObject().Select(static property => property.Name).OrderBy(static name => name));
        Assert.Equal("log_only", settings.GetProperty("prompt_injection_protection_level").GetString());
        Assert.Equal(10, settings.GetProperty("minimum_access_level_execute").GetInt32());
        Assert.Equal(30, settings.GetProperty("minimum_access_level_execute_async").GetInt32());
        Assert.Equal(40, settings.GetProperty("minimum_access_level_manage").GetInt32());
        Assert.Equal(50, settings.GetProperty("minimum_access_level_enable_on_projects").GetInt32());
    }

    [Fact]
    public void Serialize_MapsFoundationalAgentStatusesAndDuoNamespaceAccessRules()
    {
        UpdateGroupRequest request = new()
        {
            FoundationalAgentsStatuses =
            [
                new GitLabFoundationalAgentStatus { Reference = "duo_agent", Enabled = true }
            ],
            DuoNamespaceAccessRules =
            [
                new GitLabDuoNamespaceAccessRule
                {
                    ThroughNamespace = new GitLabDuoNamespaceAccessRuleThroughNamespace
                    {
                        Id = 47, Name = "Platform", FullPath = "engineering/platform"
                    },
                    Features = ["code_suggestions", "duo_agent_platform"]
                }
            ]
        };

        using JsonDocument document = JsonDocument.Parse(
            JsonSerializer.Serialize(request, UpdateGroupRequestTestJsonContext.Default.UpdateGroupRequest));
        JsonElement root = document.RootElement;
        JsonElement foundationalAgent = root.GetProperty("foundational_agents_statuses")[0];
        JsonElement duoAccessRule = root.GetProperty("duo_namespace_access_rules")[0];
        JsonElement throughNamespace = duoAccessRule.GetProperty("through_namespace");

        Assert.Equal("duo_agent", foundationalAgent.GetProperty("reference").GetString());
        Assert.True(foundationalAgent.GetProperty("enabled").GetBoolean());
        Assert.Equal(47, throughNamespace.GetProperty("id").GetInt64());
        Assert.Equal("Platform", throughNamespace.GetProperty("name").GetString());
        Assert.Equal("engineering/platform", throughNamespace.GetProperty("full_path").GetString());
        Assert.Equal(
            ["code_suggestions", "duo_agent_platform"],
            duoAccessRule.GetProperty("features").EnumerateArray().Select(static value => value.GetString()));
    }

    [Fact]
    public void Serialize_OmitsNullSettingsAtEveryNestedLevel()
    {
        UpdateGroupRequest request = new()
        {
            AiSettingsAttributes = new GitLabAiSettingsAttributes(),
            FoundationalAgentsStatuses =
            [
                new GitLabFoundationalAgentStatus { Reference = "duo_agent", Enabled = true }
            ],
            DuoNamespaceAccessRules =
            [
                new GitLabDuoNamespaceAccessRule { Features = ["code_suggestions"] }
            ]
        };

        using JsonDocument document = JsonDocument.Parse(
            JsonSerializer.Serialize(request, UpdateGroupRequestTestJsonContext.Default.UpdateGroupRequest));
        JsonElement root = document.RootElement;
        JsonElement settings = root.GetProperty("ai_settings_attributes");
        JsonElement accessRule = root.GetProperty("duo_namespace_access_rules")[0];

        Assert.Empty(settings.EnumerateObject());
        Assert.False(accessRule.TryGetProperty("through_namespace", out _));
        Assert.Equal("{}",
            JsonSerializer.Serialize(new UpdateGroupRequest(),
                UpdateGroupRequestTestJsonContext.Default.UpdateGroupRequest));
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(UpdateGroupRequest))]
internal sealed partial class UpdateGroupRequestTestJsonContext : JsonSerializerContext;