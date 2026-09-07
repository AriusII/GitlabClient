namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/protected_tags</c>.</summary>
public sealed record ProtectTagRequest
{
    /// <summary>The tag name or wildcard pattern to protect, for example <c>v*</c> or <c>release/*</c>.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Access level allowed to create the tag - 30 (developer), 40 (maintainer), 60 (admin) or 0 (no one).
    ///     GitLab defaults to 40 when omitted. Kept as an <see cref="int" /> rather than an enum, matching
    ///     <see cref="GitLabAccessLevel.AccessLevel" />.
    /// </summary>
    public int? CreateAccessLevel { get; init; }
}