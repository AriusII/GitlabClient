using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Typed settings for the <c>jira</c> integration - see <see cref="GitLabIntegrationSlug.Jira" />.
///     <para>
///         <see cref="IssuesEnabled" /> is a <see cref="string" />, not a <see cref="bool" />, even
///         though it reads as a flag - that is how the spec itself types this one field on this
///         endpoint, unlike every other <c>*_enabled</c> member here. Kept as-written rather than
///         "fixed" to a bool, since GitLab, not this client, decides the wire shape.
///     </para>
/// </summary>
public sealed record JiraIntegrationRequest
{
    /// <summary>
    ///     The URL to the Jira project which is being linked to this GitLab project (for example,
    ///     <c>https://jira.example.com</c>).
    /// </summary>
    public required Uri Url { get; init; }

    /// <summary>
    ///     The base URL to the Jira instance API. The <see cref="Url" /> value is used if not set (for
    ///     example, <c>https://jira-api.example.com</c>).
    /// </summary>
    public Uri? ApiUrl { get; init; }

    /// <summary>
    ///     The authentication method to use with Jira: <c>0</c> for basic authentication, <c>1</c> for a
    ///     Jira personal access token, and <c>2</c> for Jira Cloud service accounts. Defaults to
    ///     <c>0</c>.
    /// </summary>
    public int? JiraAuthType { get; init; }

    /// <summary>
    ///     The email or username to use with Jira. Use an email for Jira Cloud, and a username for Jira
    ///     Data Center and Jira Server. Required when using basic authentication (<see cref="JiraAuthType" />
    ///     is <c>0</c>).
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    ///     The Jira API token, password, or personal access token to use with Jira. When using basic
    ///     authentication (<see cref="JiraAuthType" /> is <c>0</c>), use an API token for Jira Cloud, and
    ///     a password for Jira Data Center or Jira Server. For a Jira personal access token
    ///     (<see cref="JiraAuthType" /> is <c>1</c>), use the personal access token.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>Regular expression to match Jira issue keys.</summary>
    public string? JiraIssueRegex { get; init; }

    /// <summary>Prefix to match Jira issue keys.</summary>
    public string? JiraIssuePrefix { get; init; }

    /// <summary>
    ///     The ID of one or more transitions for custom issue transitions. Ignored when automatic issue
    ///     transitions are enabled. Defaults to a blank string, which disables custom transitions.
    /// </summary>
    public string? JiraIssueTransitionId { get; init; }

    /// <summary>Enable viewing Jira issues in GitLab. See the type remark on this record.</summary>
    public string? IssuesEnabled { get; init; }

    /// <summary>
    ///     Keys of Jira projects to display. When <see cref="IssuesEnabled" /> is <c>"true"</c>, this
    ///     setting filters which Jira projects are shown in GitLab. It does not restrict the API token's
    ///     access.
    /// </summary>
    public IReadOnlyList<string>? ProjectKeys { get; init; }

    /// <summary>Turn on Jira issue creation for GitLab vulnerabilities.</summary>
    public bool? VulnerabilitiesEnabled { get; init; }

    /// <summary>Jira issue type to use when creating issues from vulnerabilities.</summary>
    [JsonPropertyName("vulnerabilities_issuetype")]
    public string? VulnerabilitiesIssueType { get; init; }

    /// <summary>
    ///     Key of the project to use when creating issues from vulnerabilities. Required if using the
    ///     integration to create Jira issues from vulnerabilities.
    /// </summary>
    public string? ProjectKey { get; init; }

    /// <summary>
    ///     When set to <see langword="true" />, opens a prefilled form on the Jira instance when creating
    ///     a Jira issue from a vulnerability.
    /// </summary>
    public bool? CustomizeJiraIssueEnabled { get; init; }

    /// <summary>Verify Jira issues referenced in commit messages exist before allowing the push.</summary>
    public bool? JiraCheckEnabled { get; init; }

    /// <summary>Verify the Jira issues referenced in commit messages exist in Jira.</summary>
    public bool? JiraExistsCheckEnabled { get; init; }

    /// <summary>Verify the committer is the assignee of the Jira issues referenced in commit messages.</summary>
    public bool? JiraAssigneeCheckEnabled { get; init; }

    /// <summary>Verify the status of Jira issues referenced in commit messages.</summary>
    public bool? JiraStatusCheckEnabled { get; init; }

    /// <summary>Comma-separated list of allowed Jira issue statuses.</summary>
    public string? JiraAllowedStatusesAsString { get; init; }

    /// <summary>
    ///     Enable comments inside Jira issues on each GitLab event (commit / merge request).
    /// </summary>
    public bool? CommentOnEventEnabled { get; init; }

    /// <summary>Trigger event when a commit is created or updated.</summary>
    public bool? CommitEvents { get; init; }

    /// <summary>Trigger event when a merge request is created, updated, or merged.</summary>
    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}