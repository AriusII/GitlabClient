namespace GitLab.Client.Models;

/// <summary>
///     The event type a test webhook fires as
///     (<c>POST /projects/:id/hooks/:hook_id/test/:trigger</c>,
///     <c>POST /groups/:id/hooks/:hook_id/test/:trigger</c>).
///     <para>
///         This is a closed vocabulary the spec enumerates on the path parameter, so it is modelled as an
///         enum rather than a free-text segment: an unknown trigger is a <c>422</c> from GitLab, and a
///         compile error is a cheaper way to find that out. It never appears in a JSON payload - the
///         repositories project it onto the route.
///     </para>
/// </summary>
public enum GitLabWebhookTestTrigger
{
    /// <summary>A push to a branch.</summary>
    PushEvents,

    /// <summary>A tag being created or deleted.</summary>
    TagPushEvents,

    /// <summary>An issue being opened, closed, or changed.</summary>
    IssuesEvents,

    /// <summary>A confidential issue being opened, closed, or changed.</summary>
    ConfidentialIssuesEvents,

    /// <summary>A merge request being opened, merged, or changed.</summary>
    MergeRequestsEvents,

    /// <summary>A comment being added.</summary>
    NoteEvents,

    /// <summary>A comment being added to a confidential issue.</summary>
    ConfidentialNoteEvents,

    /// <summary>A CI job changing state.</summary>
    JobEvents,

    /// <summary>A pipeline changing state.</summary>
    PipelineEvents,

    /// <summary>A wiki page being created, updated, or deleted.</summary>
    WikiPageEvents,

    /// <summary>A deployment succeeding, failing, or being cancelled.</summary>
    DeploymentEvents,

    /// <summary>A feature flag being toggled.</summary>
    FeatureFlagEvents,

    /// <summary>A milestone being created, closed, or reopened.</summary>
    MilestoneEvents,

    /// <summary>A release being created, updated, or deleted.</summary>
    ReleasesEvents,

    /// <summary>An emoji reaction being added or removed.</summary>
    EmojiEvents,

    /// <summary>A group or project access token approaching its expiry date.</summary>
    ResourceAccessTokenEvents,

    /// <summary>A deploy token approaching its expiry date.</summary>
    ResourceDeployTokenEvents,

    /// <summary>A vulnerability being created, dismissed, or otherwise updated.</summary>
    VulnerabilityEvents
}