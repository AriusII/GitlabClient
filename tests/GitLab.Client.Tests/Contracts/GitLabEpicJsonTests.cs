using System.Text.Json;

using GitLab.Client.Models;
using GitLab.Client.Serialization;

namespace GitLab.Client.Tests.Contracts;

public sealed class GitLabEpicJsonTests
{
    [Fact]
    public void Deserialize_MapsDocumentedEpicIssuableReferences()
    {
        const string Json = """
                            {
                              "id": 29,
                              "iid": 4,
                              "reference": "&4",
                              "references": {
                                "short": "&4",
                                "relative": "sample&4",
                                "full": "test/sample&4"
                              }
                            }
                            """;

        GitLabEpic epic = JsonSerializer.Deserialize(Json, GitLabJsonContext.Default.GitLabEpic)
                          ?? throw new InvalidOperationException("The GitLab epic payload should deserialize.");

        GitLabIssuableReferences references = Assert.IsType<GitLabIssuableReferences>(epic.References);
        Assert.Equal("&4", references.ShortReference);
        Assert.Equal("sample&4", references.Relative);
        Assert.Equal("test/sample&4", references.Full);
    }

    [Fact]
    public void Deserialize_MapsDocumentedRelatedEpicIssuableReferences()
    {
        const string Json = """
                            {
                              "id": 50,
                              "iid": 35,
                              "reference": "&35",
                              "references": {
                                "short": "&35",
                                "relative": "nested&35",
                                "full": "group/nested&35"
                              }
                            }
                            """;

        GitLabRelatedEpic epic = JsonSerializer.Deserialize(Json, GitLabJsonContext.Default.GitLabRelatedEpic)
                                 ?? throw new InvalidOperationException(
                                     "The GitLab related-epic payload should deserialize.");

        GitLabIssuableReferences references = Assert.IsType<GitLabIssuableReferences>(epic.References);
        Assert.Equal("&35", references.ShortReference);
        Assert.Equal("nested&35", references.Relative);
        Assert.Equal("group/nested&35", references.Full);
    }

    [Fact]
    public void Deserialize_MapsReleaseMilestoneIssueStats()
    {
        const string Json = """
                            {
                              "tag_name": "v19.4",
                              "milestones": [
                                {
                                  "id": 53,
                                  "iid": 3,
                                  "project_id": 24,
                                  "title": "v19.4",
                                  "state": "active",
                                  "web_url": "https://gitlab.example/root/awesome-app/-/milestones/3",
                                  "issue_stats": { "total": 98, "closed": 76 }
                                }
                              ]
                            }
                            """;

        GitLabRelease release = JsonSerializer.Deserialize(Json, GitLabJsonContext.Default.GitLabRelease)
                                ?? throw new InvalidOperationException(
                                    "The GitLab release payload should deserialize.");

        GitLabMilestone milestone = Assert.Single(release.Milestones!);
        GitLabMilestoneIssueStats issueStats = Assert.IsType<GitLabMilestoneIssueStats>(milestone.IssueStats);
        Assert.Equal(98, issueStats.Total);
        Assert.Equal(76, issueStats.Closed);
    }
}