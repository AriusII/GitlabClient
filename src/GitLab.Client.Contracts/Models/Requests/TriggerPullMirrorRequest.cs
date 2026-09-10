namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/mirror/pull</c>. Every member is optional: the endpoint's
///     ordinary use is to trigger an update with no body at all (see
///     <see cref="Force" /> for the one common exception), but the spec also documents a
///     <see cref="PullRequest" />-shaped payload on this same route for relaying a GitHub pull-request
///     webhook to keep a GitHub-sourced pull mirror's merge requests in sync.
/// </summary>
public sealed record TriggerPullMirrorRequest
{
    /// <summary>Resets the mirror if it is in a hard-failed state, and retries the update.</summary>
    public bool? Force { get; init; }

    /// <summary>The GitHub pull-request action (<c>opened</c>, <c>synchronize</c>, ...) being relayed.</summary>
    public string? Action { get; init; }

    /// <summary>The GitHub pull request being relayed, when this call is forwarding a GitHub webhook.</summary>
    public PullMirrorPullRequest? PullRequest { get; init; }
}