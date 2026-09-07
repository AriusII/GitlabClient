namespace GitLab.Client.Models;

/// <summary>
///     One namespace subscribed to a GitLab for Jira (Forge) installation - the link that makes the
///     namespace's branches, commits and merge requests show up as development information against Jira
///     issues.
/// </summary>
public sealed record GitLabJiraConnectSubscription
{
    /// <summary>When the namespace was subscribed.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    ///     The instance-relative path that removes this subscription from the GitLab web UI, for example
    ///     <c>/-/jira_connect/subscriptions/42</c>. It is a path rather than an absolute URL, which is why it
    ///     is typed as a string; it is meant for the Jira app's own "unlink" control, and API callers should
    ///     use <c>DeleteForgeSubscriptionAsync</c> instead.
    /// </summary>
    public string? UnlinkPath { get; init; }

    /// <summary>The subscribed namespace.</summary>
    public GitLabJiraConnectGroup? Group { get; init; }
}