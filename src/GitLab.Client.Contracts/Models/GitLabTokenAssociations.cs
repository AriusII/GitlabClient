namespace GitLab.Client.Models;

/// <summary>
///     The groups and projects reachable by the token that authenticated the request, as returned by
///     <c>GET /personal_access_tokens/self/associations</c>.
///     <para>
///         The pinned spec mislabels this response as a personal access token entity while its own
///         description says it lists "all groups and projects accessible by the personal access token".
///         The description is the accurate half - GitLab returns a
///         <c>{ "groups": [...], "projects": [...] }</c> envelope - so that is what is modelled here.
///         Both collections are nullable so a payload that omits one still deserializes.
///     </para>
/// </summary>
public sealed record GitLabTokenAssociations
{
    public IReadOnlyList<GitLabTokenAssociationGroup>? Groups { get; init; }

    public IReadOnlyList<GitLabTokenAssociationProject>? Projects { get; init; }
}