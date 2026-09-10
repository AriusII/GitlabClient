using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Serialization;

namespace GitLab.Client.Tests.Models;

public sealed class GitLabProjectTests
{
    private const string RichProjectJson = """
                                           {
                                             "id": 42,
                                             "description": "A complete project projection",
                                             "name": "client",
                                             "name_with_namespace": "platform / client",
                                             "path": "client",
                                             "path_with_namespace": "platform/client",
                                             "created_at": "2026-01-02T03:04:05Z",
                                             "default_branch": "main",
                                             "tag_list": ["sdk"],
                                             "topics": ["dotnet", "aot"],
                                             "ssh_url_to_repo": "git@gitlab.example:platform/client.git",
                                             "http_url_to_repo": "https://gitlab.example/platform/client.git",
                                             "web_url": "https://gitlab.example/platform/client",
                                             "readme_url": "https://gitlab.example/platform/client/-/blob/main/README.md",
                                             "forks_count": 4,
                                             "license_url": "https://choosealicense.com/licenses/mit/",
                                             "license": {
                                               "key": "mit",
                                               "name": "MIT License",
                                               "nickname": "MIT",
                                               "html_url": "https://choosealicense.com/licenses/mit/",
                                               "source_url": "https://spdx.org/licenses/MIT.html"
                                             },
                                             "avatar_url": "https://gitlab.example/uploads/project/avatar.png",
                                             "star_count": 7,
                                             "last_activity_at": "2026-02-03T04:05:06Z",
                                             "visibility": "private",
                                             "namespace": {
                                               "id": 5,
                                               "name": "Platform",
                                               "path": "platform",
                                               "kind": "group",
                                               "full_path": "platform",
                                               "parent_id": 1,
                                               "avatar_url": "https://gitlab.example/uploads/group/avatar.png",
                                               "web_url": "https://gitlab.example/platform"
                                             },
                                             "custom_attributes": { "key": "tier", "value": "gold" },
                                             "repository_storage": "default",
                                             "forked_from_project": { "id": 2, "path_with_namespace": "upstream/client" },
                                             "container_registry_image_prefix": "registry.example/platform/client",
                                             "_links": {
                                               "self": "https://gitlab.example/api/v4/projects/42",
                                               "issues": "https://gitlab.example/api/v4/projects/42/issues",
                                               "merge_requests": "https://gitlab.example/api/v4/projects/42/merge_requests",
                                               "repo_branches": "https://gitlab.example/api/v4/projects/42/repository/branches",
                                               "labels": "https://gitlab.example/api/v4/projects/42/labels",
                                               "events": "https://gitlab.example/api/v4/projects/42/events",
                                               "members": "https://gitlab.example/api/v4/projects/42/members",
                                               "cluster_agents": "https://gitlab.example/api/v4/projects/42/cluster_agents"
                                             },
                                             "marked_for_deletion_at": "2026-03-04T05:06:07Z",
                                             "marked_for_deletion_on": "2026-03-05T00:00:00Z",
                                             "packages_enabled": true,
                                             "empty_repo": false,
                                             "archived": false,
                                             "owner": {
                                               "id": 9,
                                               "username": "maintainer",
                                               "public_email": "maintainer@example.com",
                                               "name": "The Maintainer",
                                               "state": "active",
                                               "locked": false,
                                               "avatar_url": "https://gitlab.example/uploads/user/avatar.png",
                                               "avatar_path": "/uploads/user/avatar.png",
                                               "custom_attributes": [{ "key": "region", "value": "eu" }],
                                               "web_url": "https://gitlab.example/maintainer"
                                             },
                                             "resolve_outdated_diff_discussions": true,
                                             "container_expiration_policy": {
                                               "cadence": "1d",
                                               "enabled": "true",
                                               "keep_n": "10",
                                               "older_than": "30d",
                                               "name_regex": ".*",
                                               "name_regex_keep": "release-.*",
                                               "next_run_at": "2026-04-01T00:00:00Z"
                                             },
                                             "repository_object_format": "sha256",
                                             "issues_enabled": true,
                                             "merge_requests_enabled": true,
                                             "wiki_enabled": true,
                                             "jobs_enabled": true,
                                             "snippets_enabled": false,
                                             "container_registry_enabled": true,
                                             "service_desk_enabled": true,
                                             "service_desk_address": "support@example.com",
                                             "can_create_merge_request_in": true,
                                             "issues_access_level": "enabled",
                                             "repository_access_level": "enabled",
                                             "merge_requests_access_level": "enabled",
                                             "forking_access_level": "enabled",
                                             "wiki_access_level": "enabled",
                                             "builds_access_level": "enabled",
                                             "snippets_access_level": "enabled",
                                             "pages_access_level": "public",
                                             "analytics_access_level": "enabled",
                                             "container_registry_access_level": "enabled",
                                             "security_and_compliance_access_level": "enabled",
                                             "releases_access_level": "enabled",
                                             "environments_access_level": "enabled",
                                             "feature_flags_access_level": "enabled",
                                             "infrastructure_access_level": "enabled",
                                             "monitor_access_level": "enabled",
                                             "model_experiments_access_level": "enabled",
                                             "model_registry_access_level": "enabled",
                                             "package_registry_access_level": "enabled",
                                             "emails_disabled": false,
                                             "emails_enabled": true,
                                             "show_diff_preview_in_email": true,
                                             "shared_runners_enabled": true,
                                             "lfs_enabled": true,
                                             "creator_id": 9,
                                             "mr_default_target_self": false,
                                             "import_url": "https://example.com/client.git",
                                             "import_type": "git",
                                             "import_status": "finished",
                                             "import_error": null,
                                             "open_issues_count": 3,
                                             "description_html": "<p>A complete project projection</p>",
                                             "updated_at": "2026-02-04T05:06:07Z",
                                             "ci_default_git_depth": 20,
                                             "ci_delete_pipelines_in_seconds": 86400,
                                             "ci_forward_deployment_enabled": true,
                                             "ci_forward_deployment_rollback_allowed": true,
                                             "ci_job_token_scope_enabled": true,
                                             "ci_separated_caches": true,
                                             "ci_allow_fork_pipelines_to_run_in_parent_project": false,
                                             "ci_id_token_sub_claim_components": ["project_path", "ref_type"],
                                             "build_git_strategy": "fetch",
                                             "keep_latest_artifact": true,
                                             "restrict_user_defined_variables": true,
                                             "ci_pipeline_variables_minimum_override_role": "maintainer",
                                             "runner_token_expiration_interval": 3600,
                                             "group_runners_enabled": true,
                                             "resource_group_default_process_mode": "unordered",
                                             "auto_cancel_pending_pipelines": "enabled",
                                             "build_timeout": 3600,
                                             "auto_devops_enabled": true,
                                             "auto_devops_deploy_strategy": "continuous",
                                             "ci_push_repository_for_job_token_allowed": true,
                                             "protect_merge_request_pipelines": true,
                                             "ci_display_pipeline_variables": true,
                                             "runners_token": "secret",
                                             "ci_config_path": ".gitlab-ci.yml",
                                             "public_jobs": true,
                                             "shared_with_groups": [{ "group_id": 17, "group_access": 30 }],
                                             "only_allow_merge_if_pipeline_succeeds": true,
                                             "allow_merge_on_skipped_pipeline": false,
                                             "request_access_enabled": true,
                                             "only_allow_merge_if_all_discussions_are_resolved": true,
                                             "remove_source_branch_after_merge": true,
                                             "printing_merge_request_link_enabled": true,
                                             "merge_method": "merge",
                                             "squash_option": "default_off",
                                             "automatic_rebase_enabled": true,
                                             "enforce_auth_checks_on_uploads": true,
                                             "suggestion_commit_message": "Suggestion",
                                             "merge_commit_template": "%(title)",
                                             "squash_commit_template": "%(source_branch)",
                                             "mr_default_title_template": "%(source_branch)",
                                             "issue_branch_template": "%(title)",
                                             "statistics": {
                                               "commit_count": 37,
                                               "storage_size": 3000000000,
                                               "repository_size": 2000000000,
                                               "wiki_size": 0,
                                               "lfs_objects_size": 1,
                                               "job_artifacts_size": 2,
                                               "pipeline_artifacts_size": 3,
                                               "packages_size": 4,
                                               "snippets_size": 5,
                                               "uploads_size": 6,
                                               "container_registry_size": 7
                                             },
                                             "warn_about_potentially_unwanted_characters": true,
                                             "autoclose_referenced_issues": true,
                                             "max_artifacts_size": 100,
                                             "approvals_before_merge": "2",
                                             "mirror": "true",
                                             "mirror_user_id": "9",
                                             "mirror_trigger_builds": "false",
                                             "only_mirror_protected_branches": "true",
                                             "mirror_overwrites_diverged_branches": "false",
                                             "external_authorization_classification_label": "internal",
                                             "requirements_enabled": "true",
                                             "requirements_access_level": "enabled",
                                             "security_and_compliance_enabled": "true",
                                             "secret_push_protection_enabled": true,
                                             "pre_receive_secret_detection_enabled": true,
                                             "compliance_frameworks": "[]",
                                             "issues_template": "Issue template",
                                             "merge_requests_template": "Merge request template",
                                             "ci_restrict_pipeline_cancellation_role": "maintainer",
                                             "merge_pipelines_enabled": "true",
                                             "merge_trains_enabled": "true",
                                             "merge_trains_skip_train_allowed": "true",
                                             "merge_train_enforcement": "all",
                                             "max_pipelines_per_merge_train": "20",
                                             "only_allow_merge_if_all_status_checks_passed": "true",
                                             "allow_pipeline_trigger_approve_deployment": true,
                                             "prevent_merge_without_jira_issue": "false",
                                             "auto_duo_code_review_enabled": "true",
                                             "reviewer_assignment_strategy": "code_owners",
                                             "duo_remote_flows_enabled": "true",
                                             "duo_foundational_flows_enabled": "true",
                                             "duo_sast_fp_detection_enabled": "true",
                                             "duo_secret_detection_fp_enabled": "true",
                                             "duo_dependency_bump_breaking_changes_enabled": "true",
                                             "duo_sast_vr_workflow_enabled": "true",
                                             "web_based_commit_signing_enabled": "true",
                                             "spp_repository_pipeline_access": true,
                                             "security_policy_pipeline_must_succeed": true,
                                             "merge_request_title_regex": "/Title/",
                                             "merge_request_title_regex_description": "Requires a Jira label",
                                             "permissions": {
                                               "project_access": { "access_level": "40", "notification_level": "watch" },
                                               "group_access": { "access_level": "30", "notification_level": "participating" }
                                             },
                                             "cicd_catalog_enabled": true
                                           }
                                           """;

    [Fact]
    public void Deserialize_ProjectsWithAccessAndCatalogSetting_MapsPrimitivesReferencesAndRawObjects()
    {
        GitLabProject project = Assert.IsType<GitLabProject>(
            JsonSerializer.Deserialize(RichProjectJson, GitLabJsonContext.Default.GitLabProject));

        Assert.Equal(42, project.Id);
        Assert.Equal("platform / client", project.NameWithNamespace);
        Assert.Equal(GitLabVisibility.Private, project.Visibility);
        Assert.Equal("dotnet", project.Topics![0]);
        Assert.Equal("MIT License", project.License?.Name);
        Assert.Equal("platform", project.Namespace?.Path);
        Assert.Equal("gold", project.CustomAttributes?.Value);
        Assert.Equal(2, project.ForkedFromProject?.Id);
        Assert.Equal("/api/v4/projects/42/repository/branches", project.Links?.RepoBranches?.AbsolutePath);
        Assert.Equal("maintainer", project.Owner?.Username);
        Assert.Equal("eu", project.Owner?.CustomAttributes?[0].Value);
        Assert.Equal("10", project.ContainerExpirationPolicy?.KeepN);
        Assert.Equal("enabled", project.ModelRegistryAccessLevel);
        Assert.Equal(86400, project.CiDeletePipelinesInSeconds);
        Assert.Equal("project_path", project.CiIdTokenSubClaimComponents?[0]);
        Assert.Equal(JsonValueKind.Object, Assert.Single(project.SharedWithGroups!).ValueKind);
        Assert.Equal(3_000_000_000, project.Statistics?.StorageSize);
        Assert.Equal("true", project.MergeTrainsEnabled);
        Assert.True(project.SecretPushProtectionEnabled);
        Assert.Equal("40", project.Permissions?.ProjectAccess?.AccessLevel);
        Assert.Equal("participating", project.Permissions?.GroupAccess?.NotificationLevel);
        Assert.True(project.CiCdCatalogEnabled);
    }

    [Fact]
    public void Deserialize_BasicProjectDetails_DoesNotRequireFieldsAbsentFromTheSchema()
    {
        GitLabProject project = Assert.IsType<GitLabProject>(
            JsonSerializer.Deserialize("{}", GitLabJsonContext.Default.GitLabProject));

        Assert.Null(project.Id);
        Assert.Null(project.Name);
        Assert.Null(project.PathWithNamespace);
        Assert.Null(project.Visibility);
        Assert.Null(project.WebUrl);
    }
}