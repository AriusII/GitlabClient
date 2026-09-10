using GitLab.Client.Models;
using GitLab.Client.Models.Responses;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for the three merge request listings - <c>GET /merge_requests</c>,
///     <c>GET /groups/:id/merge_requests</c> and <c>GET /projects/:id/merge_requests</c>. GitLab declares
///     almost exactly the same parameter set on all three, so one options record covers them all.
/// </summary>
/// <remarks>
///     <para>
///         Two parameters are project-scoped or instance-scoped rather than universal: <see cref="Iids" />
///         is only honoured by the project listing, and <see cref="NonArchived" /> only by the group and
///         instance listings. GitLab runs Grape, which answers 200 and silently ignores a parameter an
///         endpoint does not declare, so sending either to the wrong listing is harmless.
///     </para>
///     <para>
///         GitLab's <c>view=simple</c> is deliberately not exposed. It replaces the response with a lean
///         projection that omits <c>source_branch</c> and <c>target_branch</c>, both of which
///         <see cref="GitLabMergeRequest" /> marks required - the request would succeed and then fail to
///         deserialize. The same goes for <c>wip=yes|no</c>, which <see cref="Draft" /> supersedes.
///     </para>
/// </remarks>
[GitLabQuery]
public sealed record MergeRequestListOptions
{
    /// <summary>Which merge requests to return. GitLab defaults to all of them.</summary>
    public MergeRequestStateFilter? State { get; init; }

    /// <summary>Returns merge requests matching all of these labels. "None" and "Any" are accepted too.</summary>
    public IReadOnlyList<string>? Labels { get; init; }

    /// <summary>Excludes merge requests carrying any of these labels.</summary>
    [QueryParameter("not[labels]")]
    public IReadOnlyList<string>? NotLabels { get; init; }

    public string? SourceBranch { get; init; }

    public string? TargetBranch { get; init; }

    /// <summary>The project the source branch lives in, for cross-project merge requests.</summary>
    public long? SourceProjectId { get; init; }

    /// <summary>Returns merge requests created by this user id. Mutually exclusive with <see cref="AuthorUsername" />.</summary>
    public long? AuthorId { get; init; }

    /// <summary>Returns merge requests created by this username. Mutually exclusive with <see cref="AuthorId" />.</summary>
    public string? AuthorUsername { get; init; }

    /// <summary>Excludes merge requests created by this user id.</summary>
    [QueryParameter("not[author_id]")]
    public long? NotAuthorId { get; init; }

    /// <summary>
    ///     Returns merge requests assigned to this user. A string rather than a number because GitLab also
    ///     accepts the wildcards "None" (unassigned) and "Any" (assigned to somebody).
    /// </summary>
    public string? AssigneeId { get; init; }

    /// <summary>Returns merge requests assigned to any of these usernames.</summary>
    public IReadOnlyList<string>? AssigneeUsername { get; init; }

    /// <summary>
    ///     Returns merge requests reviewed by this user. As with <see cref="AssigneeId" />, "None" and "Any"
    ///     are accepted alongside a numeric id.
    /// </summary>
    public string? ReviewerId { get; init; }

    /// <summary>Returns merge requests reviewed by this username.</summary>
    public string? ReviewerUsername { get; init; }

    /// <summary>Returns merge requests whose milestone has this title. "None" and "Any" are accepted.</summary>
    public string? Milestone { get; init; }

    /// <summary>Excludes merge requests whose milestone has this title.</summary>
    [QueryParameter("not[milestone]")]
    public string? NotMilestone { get; init; }

    /// <summary>Returns merge requests the authenticated user reacted to with this emoji.</summary>
    public string? MyReactionEmoji { get; init; }

    /// <summary>Whose merge requests to return, relative to the authenticated user.</summary>
    public MergeRequestScope? Scope { get; init; }

    /// <summary>Returns only draft merge requests when true, and only non-drafts when false.</summary>
    public bool? Draft { get; init; }

    /// <summary>Returns label objects rather than bare label names in the response.</summary>
    public bool? WithLabelsDetails { get; init; }

    /// <summary>Asks GitLab to recompute each merge request's merge status before answering.</summary>
    public bool? WithMergeStatusRecheck { get; init; }

    /// <summary>Free-text search over the title and description.</summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Which fields <see cref="Search" /> runs against - "title", "description", or both joined by a
    ///     comma - sent as the <c>in</c> parameter.
    /// </summary>
    [QueryParameter("in")]
    public string? SearchIn { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    public DateTimeOffset? MergedAfter { get; init; }

    public DateTimeOffset? MergedBefore { get; init; }

    /// <summary>Returns merge requests deployed after this moment. Pair it with <see cref="Environment" />.</summary>
    public DateTimeOffset? DeployedAfter { get; init; }

    public DateTimeOffset? DeployedBefore { get; init; }

    /// <summary>Returns merge requests deployed to this environment.</summary>
    public string? Environment { get; init; }

    /// <summary>Returns merge requests merged by this user id.</summary>
    public long? MergeUserId { get; init; }

    /// <summary>Returns merge requests merged by this username.</summary>
    public string? MergeUserUsername { get; init; }

    /// <summary>Returns merge requests approved by all of these user ids.</summary>
    public IReadOnlyList<long>? ApprovedByIds { get; init; }

    /// <summary>Returns merge requests approved by all of these usernames.</summary>
    public IReadOnlyList<string>? ApprovedByUsernames { get; init; }

    /// <summary>Returns merge requests these users are eligible to approve.</summary>
    public IReadOnlyList<long>? ApproverIds { get; init; }

    /// <summary>Restricts the result to these iids. Honoured by the project listing only.</summary>
    public IReadOnlyList<long>? Iids { get; init; }

    /// <summary>Skips merge requests in archived projects. Honoured by the group and instance listings only.</summary>
    public bool? NonArchived { get; init; }

    public MergeRequestOrderBy? OrderBy { get; init; }

    public MergeRequestSortDirection? Sort { get; init; }

    public int? PerPage { get; init; }
}