using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Serialization;

namespace GitLab.Client.Tests.Contracts;

public sealed class UpdateApplicationSettingsRequestContractTests
{
    [Fact]
    public void JsonMetadata_ContainsTheEntireGitLab19_4ApplicationSettingsResponseSurface()
    {
        JsonTypeInfo<GitLabApplicationSettings> typeInfo =
            GitLabJsonContext.Default.GitLabApplicationSettings;

        Assert.Equal(654, typeInfo.Properties.Count);
        Assert.All(typeInfo.Properties, static property => Assert.True(property.IsGetNullable));

        Assert.Equal(typeof(string), GetProperty(typeInfo, "auto_accept_awarded_achievements").PropertyType);
        Assert.Equal(typeof(string), GetProperty(typeInfo, "audit_events_api_limit").PropertyType);
        Assert.Equal(typeof(string), GetProperty(typeInfo, "concurrent_pull_request_import_jobs_limit").PropertyType);
        Assert.Equal(typeof(string), GetProperty(typeInfo, "tags_create_limit").PropertyType);
        Assert.Equal(typeof(string), GetProperty(typeInfo, "email_otp_enabled").PropertyType);
        Assert.Equal(typeof(int?), GetProperty(typeInfo, "logging_field_schema_version").PropertyType);
        Assert.Equal(typeof(int?), GetProperty(typeInfo, "logging_field_dual_emit_target").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "ai_audit_events_streaming_enabled").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "duo_custom_agents_enabled").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "duo_custom_flows_enabled").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "duo_external_agents_enabled").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "use_nats_for_audit_streaming").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "lock_duo_custom_agents_enabled").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "lock_duo_custom_flows_enabled").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "lock_duo_external_agents_enabled").PropertyType);
        Assert.Equal(typeof(IReadOnlyList<GitLabElasticsearchIndexSetting>),
            GetProperty(typeInfo, "elasticsearch_index_settings").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "built_in_project_templates_enabled").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "lock_built_in_project_templates_enabled").PropertyType);
        Assert.Equal(typeof(string), GetProperty(typeInfo, "zoekt_max_restarts_15m").PropertyType);
    }

    [Fact]
    public void Deserialize_AcceptsTheGitLab19_4NonStringApplicationSettingsMembers()
    {
        const string json = """
                            {
                              "logging_field_schema_version": 1,
                              "logging_field_dual_emit_target": 2,
                              "ai_audit_events_streaming_enabled": true,
                              "duo_custom_agents_enabled": true,
                              "duo_custom_flows_enabled": false,
                              "duo_external_agents_enabled": true,
                              "use_nats_for_audit_streaming": false,
                              "lock_duo_custom_agents_enabled": true,
                              "lock_duo_custom_flows_enabled": false,
                              "lock_duo_external_agents_enabled": true,
                              "elasticsearch_index_settings": [
                                { "alias_name": "gitlab-production", "number_of_shards": 5, "number_of_replicas": 1 }
                              ],
                              "built_in_project_templates_enabled": true,
                              "lock_built_in_project_templates_enabled": false,
                              "zoekt_max_restarts_15m": "3"
                            }
                            """;

        GitLabApplicationSettings? settings = JsonSerializer.Deserialize(json,
            GitLabJsonContext.Default.GitLabApplicationSettings);

        Assert.NotNull(settings);
        Assert.Equal(1, settings.LoggingFieldSchemaVersion);
        Assert.Equal(2, settings.LoggingFieldDualEmitTarget);
        Assert.True(settings.AiAuditEventsStreamingEnabled);
        Assert.True(settings.DuoCustomAgentsEnabled);
        Assert.False(settings.DuoCustomFlowsEnabled);
        Assert.True(settings.DuoExternalAgentsEnabled);
        Assert.False(settings.UseNatsForAuditStreaming);
        Assert.True(settings.LockDuoCustomAgentsEnabled);
        Assert.False(settings.LockDuoCustomFlowsEnabled);
        Assert.True(settings.LockDuoExternalAgentsEnabled);
        Assert.True(settings.BuiltInProjectTemplatesEnabled);
        Assert.False(settings.LockBuiltInProjectTemplatesEnabled);
        Assert.Equal("3", settings.ZoektMaxRestarts15m);

        GitLabElasticsearchIndexSetting index = Assert.Single(settings.ElasticsearchIndexSettings!);
        Assert.Equal("gitlab-production", index.AliasName);
        Assert.Equal(5, index.NumberOfShards);
        Assert.Equal(1, index.NumberOfReplicas);
    }

    [Fact]
    public void Serialize_UsesTheExactZoektMaxRestarts15mWireName()
    {
        UpdateApplicationSettingsRequest request = new() { ZoektMaxRestarts15m = "3" };

        using JsonDocument document = JsonDocument.Parse(
            JsonSerializer.Serialize(request, GitLabJsonContext.Default.UpdateApplicationSettingsRequest));

        Assert.Equal("3", document.RootElement.GetProperty("zoekt_max_restarts_15m").GetString());
        Assert.False(document.RootElement.TryGetProperty("zoekt_max_restarts15m", out _));
    }

    [Fact]
    public void JsonMetadata_ContainsTheCompleteAppearanceRequestAndResponseShapes()
    {
        JsonTypeInfo<UpdateApplicationAppearanceRequest> requestTypeInfo =
            GitLabJsonContext.Default.UpdateApplicationAppearanceRequest;
        JsonTypeInfo<GitLabAppearance> responseTypeInfo = GitLabJsonContext.Default.GitLabAppearance;

        Assert.Equal(
        [
            "title", "description", "pwa_name", "pwa_short_name", "pwa_description", "new_project_guidelines",
            "member_guidelines", "profile_image_guidelines", "header_message", "footer_message",
            "message_background_color", "message_font_color", "email_header_and_footer_enabled", "site_name"
        ], requestTypeInfo.Properties.Select(static property => property.Name));
        Assert.Equal(
        [
            "title", "description", "pwa_name", "pwa_short_name", "pwa_description", "logo", "pwa_icon",
            "header_logo", "favicon", "new_project_guidelines", "member_guidelines", "profile_image_guidelines",
            "header_message", "footer_message", "message_background_color", "message_font_color",
            "email_header_and_footer_enabled", "site_name"
        ], responseTypeInfo.Properties.Select(static property => property.Name));
        Assert.Equal(typeof(bool?), GetProperty(requestTypeInfo, "email_header_and_footer_enabled").PropertyType);
        Assert.Equal(typeof(bool?), GetProperty(responseTypeInfo, "email_header_and_footer_enabled").PropertyType);
        Assert.Equal(typeof(Uri), GetProperty(responseTypeInfo, "logo").PropertyType);
        Assert.Equal(typeof(Uri), GetProperty(responseTypeInfo, "pwa_icon").PropertyType);
        Assert.Equal(typeof(Uri), GetProperty(responseTypeInfo, "header_logo").PropertyType);
        Assert.Equal(typeof(Uri), GetProperty(responseTypeInfo, "favicon").PropertyType);
    }

    [Fact]
    public void JsonMetadata_ContainsTheEntireGitLab19_4ApplicationSettingsRequestSurface()
    {
        JsonTypeInfo<UpdateApplicationSettingsRequest> typeInfo =
            GitLabJsonContext.Default.UpdateApplicationSettingsRequest;

        Assert.Equal(650, typeInfo.Properties.Count);
        Assert.All(typeInfo.Properties, static property => Assert.True(property.IsGetNullable));

        Assert.Equal(typeof(bool?), GetProperty(typeInfo, "elasticsearch_aws").PropertyType);
        Assert.Equal(typeof(int?), GetProperty(typeInfo, "project_jobs_api_rate_limit").PropertyType);
        Assert.Equal(typeof(double?), GetProperty(typeInfo, "ci_job_telemetry_sampling_rate").PropertyType);
        Assert.Equal(typeof(string), GetProperty(typeInfo, "duo_availability").PropertyType);
        Assert.Equal(typeof(IReadOnlyList<string>),
            GetProperty(typeInfo, "git_rate_limit_users_allowlist").PropertyType);
        Assert.Equal(typeof(IReadOnlyList<int>), GetProperty(typeInfo, "elasticsearch_namespace_ids").PropertyType);
        Assert.Equal(typeof(IReadOnlyDictionary<string, int>),
            GetProperty(typeInfo, "repository_storages_weighted").PropertyType);
        Assert.Equal(typeof(IReadOnlyList<GitLabDuoNamespaceAccessRule>),
            GetProperty(typeInfo, "duo_namespace_access_rules").PropertyType);
        Assert.Equal(typeof(JsonElement?), GetProperty(typeInfo, "resource_usage_limits").PropertyType);
        Assert.Equal(typeof(GitLabVscodeExtensionMarketplaceSettings),
            GetProperty(typeInfo, "vscode_extension_marketplace").PropertyType);
    }

    [Fact]
    public void Serialize_UsesThePinnedSchemaNames_AndOmitsUnsetSettings()
    {
        using JsonDocument usageLimits = JsonDocument.Parse("""{ "ci_minutes": 100 }""");

        UpdateApplicationSettingsRequest request = new()
        {
            GitRateLimitUsersAllowlist = ["maintainer"],
            GitRateLimitUsersAlertlist = [42],
            RepositoryStoragesWeighted = new Dictionary<string, int> { ["default"] = 100 },
            ResourceUsageLimits = usageLimits.RootElement.Clone(),
            VscodeExtensionMarketplace = new GitLabVscodeExtensionMarketplaceSettings
            {
                Enabled = true, Preset = "open-vsx", CustomValues = usageLimits.RootElement.Clone()
            },
            DuoNamespaceAccessRules =
            [
                new GitLabDuoNamespaceAccessRule
                {
                    Features = ["code_suggestions"],
                    ThroughNamespace = new GitLabDuoNamespaceAccessRuleThroughNamespace { Id = 7 }
                }
            ]
        };

        string json = JsonSerializer.Serialize(request, GitLabJsonContext.Default.UpdateApplicationSettingsRequest);

        Assert.Equal(
            """
            {"repository_storages_weighted":{"default":100},"resource_usage_limits":{"ci_minutes":100},"vscode_extension_marketplace":{"enabled":true,"preset":"open-vsx","custom_values":{"ci_minutes":100}},"git_rate_limit_users_allowlist":["maintainer"],"git_rate_limit_users_alertlist":[42],"duo_namespace_access_rules":[{"through_namespace":{"id":7},"features":["code_suggestions"]}]}
            """,
            json);
        Assert.DoesNotContain("admin_mode", json, StringComparison.Ordinal);
    }

    private static JsonPropertyInfo GetProperty(JsonTypeInfo typeInfo, string name)
    {
        return Assert.Single(typeInfo.Properties, property => property.Name == name);
    }
}