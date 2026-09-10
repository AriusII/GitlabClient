using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     One custom attribute - an administrator-defined key/value pair hung off a user, a group or a
///     project (<c>/users/:id/custom_attributes</c>, <c>/groups/:id/custom_attributes</c>,
///     <c>/projects/:id/custom_attributes</c>).
/// </summary>
/// <remarks>
///     The whole area is administrator-only: a non-administrator token gets <c>403</c> even for the read
///     endpoints. Attributes are also invisible on the parent resource unless it was fetched with
///     <c>with_custom_attributes=true</c>.
/// </remarks>
[SuppressMessage("Naming", "CA1711",
    Justification =
        "'Custom attribute' is GitLab's own name for this entity - it names the API area, the route "
        + "segment (custom_attributes) and the with_custom_attributes query parameter. CA1711 guards "
        + "against a name that falsely suggests System.Attribute; this record is a plain DTO in the "
        + "Models namespace, never used as an attribute, and renaming it would break the one-to-one "
        + "mapping between the public surface and the GitLab vocabulary the whole library is built on.")]
public sealed record GitLabCustomAttribute
{
    /// <summary>
    ///     The attribute's key. Free text chosen by whoever set it, so it may contain <c>/</c> or spaces.
    ///     GitLab's OpenAPI schema does not declare response members as required, so this remains nullable
    ///     rather than rejecting a forward-compatible partial projection.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    ///     The attribute's value. GitLab stores no type information, so a number or a flag comes back as
    ///     its text form. The OpenAPI schema explicitly permits <c>null</c>, which is distinct from an
    ///     absent response member and must therefore reach callers intact.
    /// </summary>
    public string? Value { get; init; }
}