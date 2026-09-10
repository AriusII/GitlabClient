using GitLab.Client.Models;

namespace GitLab.Client.Endpoints;

/// <summary>
///     Projects <see cref="GitLabWebhookTestTrigger" /> onto the <c>{trigger}</c> path segment of the test
///     endpoints. The mapping is an explicit switch rather than a naming convention so that adding an enum
///     member without giving it a wire name is a compile error (CS8509), not a <c>404</c> at run time.
/// </summary>
internal static class GitLabWebhookTestTriggerRoutes
{
    /// <summary>
    ///     The GitLab wire name for a trigger. Safe to pass to
    ///     <see cref="Infrastructure.Routing.GitLabRouteBuilder.Literal" />: every value is a fixed
    ///     lower-snake-case word from a closed vocabulary, never caller-supplied text.
    /// </summary>
    internal static string ToRouteValue(this GitLabWebhookTestTrigger trigger)
    {
        return trigger switch
        {
            GitLabWebhookTestTrigger.PushEvents => "push_events",
            GitLabWebhookTestTrigger.TagPushEvents => "tag_push_events",
            GitLabWebhookTestTrigger.IssuesEvents => "issues_events",
            GitLabWebhookTestTrigger.ConfidentialIssuesEvents => "confidential_issues_events",
            GitLabWebhookTestTrigger.MergeRequestsEvents => "merge_requests_events",
            GitLabWebhookTestTrigger.NoteEvents => "note_events",
            GitLabWebhookTestTrigger.ConfidentialNoteEvents => "confidential_note_events",
            GitLabWebhookTestTrigger.JobEvents => "job_events",
            GitLabWebhookTestTrigger.PipelineEvents => "pipeline_events",
            GitLabWebhookTestTrigger.WikiPageEvents => "wiki_page_events",
            GitLabWebhookTestTrigger.DeploymentEvents => "deployment_events",
            GitLabWebhookTestTrigger.FeatureFlagEvents => "feature_flag_events",
            GitLabWebhookTestTrigger.MilestoneEvents => "milestone_events",
            GitLabWebhookTestTrigger.ReleasesEvents => "releases_events",
            GitLabWebhookTestTrigger.EmojiEvents => "emoji_events",
            GitLabWebhookTestTrigger.ResourceAccessTokenEvents => "resource_access_token_events",
            GitLabWebhookTestTrigger.ResourceDeployTokenEvents => "resource_deploy_token_events",
            GitLabWebhookTestTrigger.VulnerabilityEvents => "vulnerability_events",
            _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger,
                "Unknown GitLab webhook test trigger.")
        };
    }
}