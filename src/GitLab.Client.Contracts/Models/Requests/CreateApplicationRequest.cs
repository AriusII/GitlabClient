using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /applications</c> and <c>POST /user/applications</c>. GitLab declares
///     the two bodies identically, so one type serves both.
/// </summary>
public sealed record CreateApplicationRequest
{
    /// <summary>The name shown for this application in GitLab's authorization prompts.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The application's redirect URI. Several URIs may be sent by separating them with a newline,
    ///     which is why this is a <see cref="string" /> rather than a single <see cref="Uri" />.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "Several redirect URIs may be sent newline-separated in one string; a single System.Uri cannot express that.")]
    public required string RedirectUri { get; init; }

    /// <summary>
    ///     Space-separated OAuth scopes to grant, for example <c>"api read_user"</c>. See
    ///     <see cref="GitLabOAuthApplicationScopes" /> for the well-known values.
    /// </summary>
    public required string Scopes { get; init; }

    /// <summary>
    ///     Whether the application can keep its client secret private (a server-side app) rather than
    ///     public (a native or single-page app). GitLab defaults this to <see langword="true" /> when
    ///     omitted.
    /// </summary>
    public bool? Confidential { get; init; }
}