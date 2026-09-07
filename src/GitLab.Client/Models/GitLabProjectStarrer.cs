namespace GitLab.Client.Models;

/// <summary>One entry of <c>GET /projects/:id/starrers</c>: a user, and when they starred the project.</summary>
/// <remarks>
///     The vendored spec declares this response as the plain user entity, which is a gap in the spec
///     rather than the behaviour: the endpoint answers with the star itself, wrapping the user under a
///     <c>user</c> key next to <c>starred_since</c>. Both members are optional so that either shape
///     deserializes rather than throwing.
/// </remarks>
public sealed record GitLabProjectStarrer
{
    /// <summary>When this user starred the project.</summary>
    public DateTimeOffset? StarredSince { get; init; }

    /// <summary>The user who starred the project.</summary>
    public GitLabUser? User { get; init; }
}