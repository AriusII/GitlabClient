using System.Text.Json;

using GitLab.Client.Models;
using GitLab.Client.Models.Responses;
using GitLab.Client.Serialization;

namespace GitLab.Client.Tests.Contracts;

public sealed class GitLabIssuableResponseEnrichmentJsonTests
{
    [Fact]
    public void Deserialize_IssueMapsAllTypedUnderscoreLinks()
    {
        const string Json = """
                            {
                              "id": 41,
                              "iid": 7,
                              "title": "Link-aware issue",
                              "state": "opened",
                              "web_url": "https://gitlab.example/group/project/-/issues/7",
                              "_links": {
                                "self": "/api/v4/projects/41/issues/7",
                                "notes": "/api/v4/projects/41/issues/7/notes",
                                "award_emoji": "/api/v4/projects/41/issues/7/award_emoji",
                                "project": "/api/v4/projects/41",
                                "closed_as_duplicate_of": "/api/v4/projects/41/issues/3"
                              }
                            }
                            """;

        GitLabIssue issue = JsonSerializer.Deserialize(Json, GitLabJsonContext.Default.GitLabIssue)
                            ?? throw new InvalidOperationException("The issue payload should deserialize.");

        GitLabIssueLinks links = Assert.IsType<GitLabIssueLinks>(issue.Links);
        Assert.Equal("/api/v4/projects/41/issues/7", links.Self);
        Assert.Equal("/api/v4/projects/41/issues/7/notes", links.Notes);
        Assert.Equal("/api/v4/projects/41/issues/7/award_emoji", links.AwardEmoji);
        Assert.Equal("/api/v4/projects/41", links.Project);
        Assert.Equal("/api/v4/projects/41/issues/3", links.ClosedAsDuplicateOf);
    }

    [Fact]
    public void Deserialize_MergeRequestMapsChangesOverflow()
    {
        const string Json = """
                            {
                              "id": 84,
                              "iid": 14,
                              "title": "Truncated changes",
                              "state": "opened",
                              "source_branch": "feature/truncated-diff",
                              "target_branch": "main",
                              "web_url": "https://gitlab.example/group/project/-/merge_requests/14",
                              "overflow": true
                            }
                            """;

        GitLabMergeRequest mergeRequest = JsonSerializer.Deserialize(
                                              Json,
                                              GitLabJsonContext.Default.GitLabMergeRequest)
                                          ?? throw new InvalidOperationException(
                                              "The merge request payload should deserialize.");

        Assert.Equal(true, mergeRequest.Overflow);
    }

    [Fact]
    public void Deserialize_NotePreservesOpaqueCommandsChangesJson()
    {
        const string Json = """
                            {
                              "id": 93,
                              "body": "/close",
                              "commands_changes": {
                                "state": "closed",
                                "metadata": { "quick_action": "close" }
                              }
                            }
                            """;

        GitLabNote note = JsonSerializer.Deserialize(Json, GitLabJsonContext.Default.GitLabNote)
                          ?? throw new InvalidOperationException("The note payload should deserialize.");

        Assert.True(note.CommandsChanges.HasValue);

        JsonElement commandsChanges = note.CommandsChanges.Value;
        Assert.Equal(JsonValueKind.Object, commandsChanges.ValueKind);
        Assert.Equal("closed", commandsChanges.GetProperty("state").GetString());
        Assert.Equal("close", commandsChanges.GetProperty("metadata").GetProperty("quick_action").GetString());
    }
}