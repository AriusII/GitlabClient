namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /integrations/jira_connect/subscriptions</c>: the namespace to link to a Jira
///     Connect installation, together with the Jira-issued JWT that identifies which installation is
///     asking.
/// </summary>
public sealed record SubscribeJiraConnectNamespaceRequest
{
    /// <summary>
    ///     The JWT minted by the Jira Connect app, which identifies the calling
    ///     <c>JiraConnectInstallation</c>. It is not a GitLab credential and cannot be produced by an
    ///     ordinary API consumer; it arrives in the Atlassian iframe context that the app runs in.
    /// </summary>
    public required string Jwt { get; init; }

    /// <summary>
    ///     Full path of the group or user namespace to subscribe, for example <c>gitlab-org</c> or
    ///     <c>parent/child</c>. This is a body field, not a route segment, so it is sent unencoded.
    /// </summary>
    public required string NamespacePath { get; init; }
}