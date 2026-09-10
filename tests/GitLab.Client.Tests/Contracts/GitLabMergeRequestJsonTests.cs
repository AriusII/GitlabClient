using System.Text.Json;

using GitLab.Client.Models.Responses;
using GitLab.Client.Serialization;

namespace GitLab.Client.Tests.Contracts;

public sealed class GitLabMergeRequestJsonTests
{
    [Fact]
    public void Deserialize_MapsTheCompleteGitLab19MergeRequestShape()
    {
        const string Json = """
                            {
                              "id": 84,
                              "iid": 14,
                              "project_id": 4,
                              "title": "Complete merge request",
                              "description": "Description",
                              "state": "opened",
                              "created_at": "2026-02-01T10:11:12Z",
                              "updated_at": "2026-02-02T10:11:12Z",
                              "merged_by": { "id": 1, "username": "legacy-merger", "name": "Legacy Merger", "web_url": "https://gitlab.example/legacy-merger" },
                              "merge_user": { "id": 2, "username": "merger", "name": "Merger", "web_url": "https://gitlab.example/merger" },
                              "merged_at": "2026-02-03T10:11:12Z",
                              "closed_by": { "id": 3, "username": "closer", "name": "Closer", "web_url": "https://gitlab.example/closer" },
                              "closed_at": "2026-02-04T10:11:12Z",
                              "title_html": "<strong>Complete merge request</strong>",
                              "description_html": "<p>Description</p>",
                              "target_branch": "main",
                              "source_branch": "feature/complete",
                              "user_notes_count": 3,
                              "upvotes": 4,
                              "downvotes": 1,
                              "author": { "id": 4, "username": "author", "name": "Author", "web_url": "https://gitlab.example/author" },
                              "assignees": [{ "id": 5, "username": "assignee-one", "name": "Assignee One", "web_url": "https://gitlab.example/assignee-one" }],
                              "assignee": { "id": 6, "username": "assignee", "name": "Assignee", "web_url": "https://gitlab.example/assignee" },
                              "reviewers": [{ "id": 7, "username": "reviewer", "name": "Reviewer", "web_url": "https://gitlab.example/reviewer" }],
                              "source_project_id": 8,
                              "target_project_id": 9,
                              "labels": ["backend"],
                              "draft": false,
                              "imported": true,
                              "imported_from": "bitbucket",
                              "work_in_progress": false,
                              "milestone": { "id": 10, "iid": 11, "title": "19.4", "state": "active", "web_url": "https://gitlab.example/groups/example/-/milestones/11" },
                              "merge_when_pipeline_succeeds": true,
                              "merge_status": "can_be_merged",
                              "detailed_merge_status": "mergeable",
                              "merge_after": "2026-02-05T10:11:12Z",
                              "sha": "source-sha",
                              "merge_commit_sha": "merge-sha",
                              "squash_commit_sha": "squash-sha",
                              "discussion_locked": false,
                              "should_remove_source_branch": true,
                              "force_remove_source_branch": false,
                              "prepared_at": "2026-02-06T10:11:12Z",
                              "allow_collaboration": true,
                              "allow_maintainer_to_push": true,
                              "reference": "!14",
                              "references": { "short": "!14", "relative": "example!14", "full": "group/example!14" },
                              "web_url": "https://gitlab.example/group/example/-/merge_requests/14",
                              "time_stats": { "time_estimate": 3600, "total_time_spent": 1200, "human_time_estimate": "1h", "human_total_time_spent": "20m" },
                              "squash": true,
                              "squash_on_merge": true,
                              "task_completion_status": { "count": 5, "completed_count": 3 },
                              "has_conflicts": false,
                              "blocking_discussions_resolved": true,
                              "approvals_before_merge": 2,
                              "subscribed": true,
                              "changes_count": "1",
                              "latest_build_started_at": "2026-02-07T10:11:12Z",
                              "latest_build_finished_at": "2026-02-07T10:31:12Z",
                              "first_deployed_to_production_at": "2026-02-08T10:11:12Z",
                              "pipeline": {
                                "id": 12,
                                "iid": 13,
                                "project_id": 4,
                                "sha": "pipeline-sha",
                                "ref": "refs/merge-requests/14/head",
                                "status": "success",
                                "source": "merge_request_event",
                                "created_at": "2026-02-07T10:00:00Z",
                                "updated_at": "2026-02-07T10:10:00Z",
                                "web_url": "https://gitlab.example/group/example/-/pipelines/12"
                              },
                              "head_pipeline": {
                                "id": 14,
                                "iid": 15,
                                "project_id": 4,
                                "sha": "head-pipeline-sha",
                                "ref": "feature/complete",
                                "status": "success",
                                "source": "merge_request_event",
                                "created_at": "2026-02-07T10:00:00Z",
                                "updated_at": "2026-02-07T10:10:00Z",
                                "web_url": "https://gitlab.example/group/example/-/pipelines/14",
                                "before_sha": "before-sha",
                                "tag": false,
                                "yaml_errors": null,
                                "user": { "id": 8, "username": "pipeline-user", "name": "Pipeline User", "web_url": "https://gitlab.example/pipeline-user" },
                                "started_at": "2026-02-07T10:01:00Z",
                                "finished_at": "2026-02-07T10:05:00Z",
                                "committed_at": "2026-02-07T09:59:00Z",
                                "duration": 127,
                                "queued_duration": 63,
                                "coverage": 98.25,
                                "detailed_status": {
                                  "icon": "status_success",
                                  "text": "passed",
                                  "label": "passed",
                                  "group": "success",
                                  "tooltip": "passed",
                                  "has_details": true,
                                  "details_path": "/group/example/-/pipelines/14",
                                  "illustration": { "image": "illustrations/success.svg" },
                                  "favicon": "/assets/ci_favicons/favicon_status_success.png",
                                  "action": { "icon": "cancel", "title": "Cancel", "path": "/group/example/-/jobs/2/cancel", "method": "post", "button_title": "Cancel", "confirmation_message": "Are you sure?" }
                                },
                                "archived": false
                              },
                              "diff_refs": { "base_sha": "base-sha", "head_sha": "head-sha", "start_sha": "start-sha" },
                              "merge_error": "none",
                              "rebase_in_progress": false,
                              "diverged_commits_count": 0,
                              "first_contribution": true,
                              "user": { "can_merge": true }
                            }
                            """;

        GitLabMergeRequest mergeRequest = JsonSerializer.Deserialize(
                                              Json,
                                              GitLabJsonContext.Default.GitLabMergeRequest)
                                          ?? throw new InvalidOperationException(
                                              "The GitLab merge request payload should deserialize.");

        Assert.Equal("legacy-merger", mergeRequest.MergedBy?.Username);
        Assert.Equal("<strong>Complete merge request</strong>", mergeRequest.TitleHtml);
        Assert.Equal("<p>Description</p>", mergeRequest.DescriptionHtml);
        Assert.Equal(6, mergeRequest.Assignee?.Id);
        Assert.True(mergeRequest.Imported);
        Assert.Equal("bitbucket", mergeRequest.ImportedFrom);
        Assert.False(mergeRequest.WorkInProgress);
        Assert.Equal(10, mergeRequest.Milestone?.Id);
        Assert.Equal(new DateTimeOffset(2026, 2, 5, 10, 11, 12, TimeSpan.Zero), mergeRequest.MergeAfter);
        Assert.True(mergeRequest.AllowMaintainerToPush);
        Assert.Equal("!14", mergeRequest.References?.ShortReference);
        Assert.Equal("example!14", mergeRequest.References?.Relative);
        Assert.Equal("group/example!14", mergeRequest.References?.Full);
        Assert.True(mergeRequest.SquashOnMerge);
        Assert.Equal(5, mergeRequest.TaskCompletionStatus?.Count);
        Assert.Equal(3, mergeRequest.TaskCompletionStatus?.CompletedCount);
        Assert.Equal(2, mergeRequest.ApprovalsBeforeMerge);
        Assert.Equal(new DateTimeOffset(2026, 2, 7, 10, 11, 12, TimeSpan.Zero), mergeRequest.LatestBuildStartedAt);
        Assert.Equal(new DateTimeOffset(2026, 2, 7, 10, 31, 12, TimeSpan.Zero), mergeRequest.LatestBuildFinishedAt);
        Assert.Equal(new DateTimeOffset(2026, 2, 8, 10, 11, 12, TimeSpan.Zero),
            mergeRequest.FirstDeployedToProductionAt);

        GitLabMergeRequestPipeline pipeline = Assert.IsType<GitLabMergeRequestPipeline>(mergeRequest.Pipeline);
        Assert.Equal(12, pipeline.Id);
        Assert.Equal(13, pipeline.Iid);
        Assert.Equal(4, pipeline.ProjectId);
        Assert.Equal("pipeline-sha", pipeline.Sha);
        Assert.Equal("success", pipeline.Status);
        Assert.Equal("refs/merge-requests/14/head", pipeline.Ref);
        Assert.Equal("merge_request_event", pipeline.Source);
        Assert.Equal(new DateTimeOffset(2026, 2, 7, 10, 0, 0, TimeSpan.Zero), pipeline.CreatedAt);
        Assert.Equal(new DateTimeOffset(2026, 2, 7, 10, 10, 0, TimeSpan.Zero), pipeline.UpdatedAt);
        Assert.Equal(new Uri("https://gitlab.example/group/example/-/pipelines/12"), pipeline.WebUrl);

        GitLabMergeRequestPipeline headPipeline = Assert.IsType<GitLabMergeRequestPipeline>(mergeRequest.HeadPipeline);
        Assert.Equal(14, headPipeline.Id);
        Assert.Equal(15, headPipeline.Iid);
        Assert.Equal(4, headPipeline.ProjectId);
        Assert.Equal("head-pipeline-sha", headPipeline.Sha);
        Assert.Equal("feature/complete", headPipeline.Ref);
        Assert.Equal("success", headPipeline.Status);
        Assert.Equal("merge_request_event", headPipeline.Source);
        Assert.Equal(new DateTimeOffset(2026, 2, 7, 10, 0, 0, TimeSpan.Zero), headPipeline.CreatedAt);
        Assert.Equal(new DateTimeOffset(2026, 2, 7, 10, 10, 0, TimeSpan.Zero), headPipeline.UpdatedAt);
        Assert.Equal(new Uri("https://gitlab.example/group/example/-/pipelines/14"), headPipeline.WebUrl);
        Assert.Equal("before-sha", headPipeline.BeforeSha);
        Assert.False(headPipeline.Tag);
        Assert.Null(headPipeline.YamlErrors);
        Assert.Equal("pipeline-user", headPipeline.User?.Username);
        Assert.Equal(new DateTimeOffset(2026, 2, 7, 10, 1, 0, TimeSpan.Zero), headPipeline.StartedAt);
        Assert.Equal(new DateTimeOffset(2026, 2, 7, 10, 5, 0, TimeSpan.Zero), headPipeline.FinishedAt);
        Assert.Equal(new DateTimeOffset(2026, 2, 7, 9, 59, 0, TimeSpan.Zero), headPipeline.CommittedAt);
        Assert.Equal(127, headPipeline.Duration);
        Assert.Equal(63, headPipeline.QueuedDuration);
        Assert.Equal(98.25f, headPipeline.Coverage);
        Assert.False(headPipeline.Archived);

        GitLabMergeRequestPipelineDetailedStatus detailedStatus =
            Assert.IsType<GitLabMergeRequestPipelineDetailedStatus>(headPipeline.DetailedStatus);
        Assert.Equal("status_success", detailedStatus.Icon);
        Assert.Equal("passed", detailedStatus.Text);
        Assert.Equal("passed", detailedStatus.Label);
        Assert.Equal("success", detailedStatus.Group);
        Assert.Equal("passed", detailedStatus.Tooltip);
        Assert.True(detailedStatus.HasDetails);
        Assert.Equal("/group/example/-/pipelines/14", detailedStatus.DetailsPath);
        Assert.Equal(JsonValueKind.Object, detailedStatus.Illustration?.ValueKind);
        Assert.Equal("/assets/ci_favicons/favicon_status_success.png", detailedStatus.Favicon);

        GitLabMergeRequestPipelineDetailedStatusAction action =
            Assert.IsType<GitLabMergeRequestPipelineDetailedStatusAction>(detailedStatus.Action);
        Assert.Equal("cancel", action.Icon);
        Assert.Equal("Cancel", action.Title);
        Assert.Equal("/group/example/-/jobs/2/cancel", action.Path);
        Assert.Equal("post", action.Method);
        Assert.Equal("Cancel", action.ButtonTitle);
        Assert.Equal("Are you sure?", action.ConfirmationMessage);

        Assert.Equal("base-sha", mergeRequest.DiffRefs?.BaseSha);
        Assert.Equal("head-sha", mergeRequest.DiffRefs?.HeadSha);
        Assert.Equal("start-sha", mergeRequest.DiffRefs?.StartSha);
        Assert.Equal("none", mergeRequest.MergeError);
        Assert.False(mergeRequest.RebaseInProgress);
        Assert.Equal(0, mergeRequest.DivergedCommitsCount);
        Assert.True(mergeRequest.FirstContribution);
        Assert.True(mergeRequest.User?.CanMerge);
    }
}