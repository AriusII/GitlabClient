using System.Text.Json;

using GitLab.Client.Models;
using GitLab.Client.Serialization;

namespace GitLab.Client.Tests.Contracts.Groups;

public sealed class GitLabGroupJsonTests
{
    [Fact]
    public void Deserialize_GroupWithProjectsAndSharedProjects_PreservesProjectLists()
    {
        const string Json = """
                            {
                              "id": 20,
                              "projects": [
                                {
                                  "id": 101,
                                  "name": "Owned project",
                                  "path_with_namespace": "example/owned-project"
                                }
                              ],
                              "shared_projects": [
                                {
                                  "id": 202,
                                  "name": "Shared project",
                                  "path_with_namespace": "partners/shared-project"
                                }
                              ]
                            }
                            """;

        GitLabGroup group = JsonSerializer.Deserialize(Json, GitLabJsonContext.Default.GitLabGroup)
                            ?? throw new InvalidOperationException("The GitLab group payload should deserialize.");

        GitLabProject project = Assert.Single(group.Projects ?? []);
        Assert.Equal(101L, project.Id);
        Assert.Equal("Owned project", project.Name);
        Assert.Equal("example/owned-project", project.PathWithNamespace);

        GitLabProject sharedProject = Assert.Single(group.SharedProjects ?? []);
        Assert.Equal(202L, sharedProject.Id);
        Assert.Equal("Shared project", sharedProject.Name);
        Assert.Equal("partners/shared-project", sharedProject.PathWithNamespace);
    }

    [Fact]
    public void Deserialize_GroupWithNumericStatisticsAndDefaultBranchProtectionDefaults_PreservesValues()
    {
        const string Json = """
                            {
                              "default_branch_protection_defaults": {
                                "allowed_to_push": [{ "access_level": 30 }],
                                "allowed_to_merge": [{ "access_level": 40 }],
                                "allow_force_push": false,
                                "developer_can_initial_push": true,
                                "code_owner_approval_required": true
                              },
                              "statistics": {
                                "storage_size": 1,
                                "repository_size": 2,
                                "wiki_size": 3,
                                "lfs_objects_size": 4,
                                "job_artifacts_size": 5,
                                "pipeline_artifacts_size": 6,
                                "packages_size": 7,
                                "snippets_size": 8,
                                "uploads_size": 9
                              }
                            }
                            """;

        GitLabGroup group = JsonSerializer.Deserialize(Json, GitLabJsonContext.Default.GitLabGroup)
                            ?? throw new InvalidOperationException("The GitLab group payload should deserialize.");

        GitLabDefaultBranchProtectionDefaults defaultBranchProtectionDefaults =
            Assert.IsType<GitLabDefaultBranchProtectionDefaults>(
                group.DefaultBranchProtectionDefaults);
        Assert.Equal(30, Assert.Single(defaultBranchProtectionDefaults.AllowedToPush ?? []).AccessLevel);
        Assert.Equal(40, Assert.Single(defaultBranchProtectionDefaults.AllowedToMerge ?? []).AccessLevel);
        Assert.False(defaultBranchProtectionDefaults.AllowForcePush);
        Assert.True(defaultBranchProtectionDefaults.DeveloperCanInitialPush);
        Assert.True(defaultBranchProtectionDefaults.CodeOwnerApprovalRequired);

        GitLabGroupStatistics statistics = Assert.IsType<GitLabGroupStatistics>(group.Statistics);
        Assert.Equal(1L, statistics.StorageSize);
        Assert.Equal(2L, statistics.RepositorySize);
        Assert.Equal(3L, statistics.WikiSize);
        Assert.Equal(4L, statistics.LfsObjectsSize);
        Assert.Equal(5L, statistics.JobArtifactsSize);
        Assert.Equal(6L, statistics.PipelineArtifactsSize);
        Assert.Equal(7L, statistics.PackagesSize);
        Assert.Equal(8L, statistics.SnippetsSize);
        Assert.Equal(9L, statistics.UploadsSize);
    }
}