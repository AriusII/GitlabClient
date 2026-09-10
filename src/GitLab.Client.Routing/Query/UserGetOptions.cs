using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Optional projections for <c>GET /users/:id</c>.</summary>
/// <remarks>
///     GitLab also documents <c>custom_attributes</c> as an unconstrained object. It is deliberately
///     not exposed here: the OpenAPI document does not define its key/value encoding, and choosing one
///     in a typed route builder would create a client contract GitLab does not guarantee. Use
///     <see cref="WithCustomAttributes" /> to request the attributes themselves.
/// </remarks>
[GitLabQuery]
public readonly record struct UserGetOptions
{
    /// <summary>
    ///     Includes administrator-defined custom attributes in the user projection. GitLab only honours
    ///     this for instance administrators.
    /// </summary>
    public bool? WithCustomAttributes { get; init; }
}