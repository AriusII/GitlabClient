namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /integrations/jira_forge/subscriptions</c>: the namespace to subscribe to the
///     GitLab for Jira (Forge) installation. Unlike the Jira Connect form it carries no JWT - the Forge
///     endpoints authenticate as the GitLab user and resolve the installation from the Forge invocation
///     context.
/// </summary>
public sealed record CreateJiraForgeSubscriptionRequest
{
    /// <summary>
    ///     Full path of the group or user namespace to subscribe, for example <c>gitlab-org</c> or
    ///     <c>parent/child</c>. GitLab rejects anything longer than 255 characters.
    /// </summary>
    public required string NamespacePath { get; init; }
}