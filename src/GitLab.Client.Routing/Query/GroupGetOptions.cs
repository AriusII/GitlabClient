using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Optional projections for <c>GET /groups/:id</c>.</summary>
/// <remarks>
///     GitLab also documents <c>custom_attributes</c> as an unstructured object. It is deliberately not
///     exposed here because a typed route builder cannot safely choose a query-string representation for
///     an object whose key/value shape the specification leaves open.
/// </remarks>
[GitLabQuery]
public readonly record struct GroupGetOptions
{
    /// <summary>Includes custom attributes in the group projection. Administrators only.</summary>
    public bool? WithCustomAttributes { get; init; }

    /// <summary>
    ///     Includes the group's project details. GitLab includes them by default; set this to
    ///     <see langword="false" /> to reduce a large group payload.
    /// </summary>
    public bool? WithProjects { get; init; }
}