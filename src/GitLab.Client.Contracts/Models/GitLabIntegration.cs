using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One integration configured on a project or a group
///     (<c>/projects/:id/integrations/:slug</c>, <c>/groups/:id/integrations/:slug</c>) - Slack, Jira,
///     Jenkins, and the ~50 others GitLab ships.
///     <para>
///         This single shape covers both entities the spec declares. The listing and the "create or
///         update" response are <c>IntegrationBasic</c>, which carries every member here except
///         <see cref="Properties" />; only "retrieve integration settings" returns the full
///         <c>Integration</c>, which adds it. Everything except <see cref="Id" /> and
///         <see cref="Slug" /> is therefore nullable - which members GitLab populates depends on which
///         events the individual integration supports, not on a fixed contract.
///     </para>
/// </summary>
public sealed record GitLabIntegration
{
    /// <summary>The integration's numeric id. Not usable in a route: every endpoint keys on <see cref="Slug" />.</summary>
    public required long Id { get; init; }

    /// <summary>Human-readable name, for example <c>Slack notifications</c>.</summary>
    public string? Title { get; init; }

    /// <summary>
    ///     The hyphenated key this integration is addressed by - <c>slack</c>, <c>apple-app-store</c>,
    ///     <c>custom-issue-tracker</c>. See <see cref="GitLabIntegrationSlug" /> for the well-known values.
    /// </summary>
    public required string Slug { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Whether the integration is enabled. A listing endpoint returns only active integrations.</summary>
    public bool? Active { get; init; }

    /// <summary>Whether the settings are inherited from a group or from the instance rather than set here.</summary>
    public bool? Inherited { get; init; }

    /// <summary>Whether GitLab comments back on the originating object when the integration fires.</summary>
    public bool? CommentOnEventEnabled { get; init; }

    public bool? CommitEvents { get; init; }

    public bool? PushEvents { get; init; }

    public bool? IssuesEvents { get; init; }

    public bool? ConfidentialIssuesEvents { get; init; }

    public bool? IncidentEvents { get; init; }

    public bool? AlertEvents { get; init; }

    public bool? MergeRequestsEvents { get; init; }

    public bool? TagPushEvents { get; init; }

    public bool? DeploymentEvents { get; init; }

    public bool? NoteEvents { get; init; }

    public bool? ConfidentialNoteEvents { get; init; }

    public bool? PipelineEvents { get; init; }

    public bool? WikiPageEvents { get; init; }

    public bool? JobEvents { get; init; }

    public bool? VulnerabilityEvents { get; init; }

    /// <summary>
    ///     The integration's own settings - the webhook URL for Slack, the project key for Jira, the
    ///     bundle id for the Apple App Store. Returned only by "retrieve integration settings", and its
    ///     members differ for every one of the ~50 slugs, which is why the spec types it as a bare object
    ///     and it is surfaced as a raw <see cref="JsonElement" /> here rather than forced into a shape
    ///     GitLab does not promise. Deserialize it into a typed settings record when you know the slug.
    ///     <para>
    ///         Secrets never round-trip: GitLab masks tokens and passwords out of this object, so writing
    ///         a value read from here straight back through a setter would blank the credential.
    ///     </para>
    /// </summary>
    public JsonElement? Properties { get; init; }
}