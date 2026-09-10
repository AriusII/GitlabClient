namespace GitLab.Client.Models.Requests;

/// <summary>
///     Names the Git ref to publish as a Composer package (<c>POST /projects/:id/packages/composer</c>).
///     Exactly one of <see cref="Branch" /> or <see cref="Tag" /> is normally given; the body itself is
///     optional on the wire, which is why the repository defaults it to an empty instance rather than
///     requiring a caller to construct one for every call.
/// </summary>
public sealed record ComposerPackageCreateRequest
{
    /// <summary>The name of the branch to publish.</summary>
    public string? Branch { get; init; }

    /// <summary>The name of the tag to publish.</summary>
    public string? Tag { get; init; }
}