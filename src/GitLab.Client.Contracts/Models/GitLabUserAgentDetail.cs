namespace GitLab.Client.Models;

/// <summary>
///     The user-agent details GitLab records against user-submitted content for spam checking
///     (<c>APIEntitiesUserAgentDetail</c>). Readable only by an instance administrator; a non-admin token
///     gets a <c>404</c>.
/// </summary>
public sealed record GitLabUserAgentDetail
{
    /// <summary>The raw <c>User-Agent</c> header captured when the content was submitted.</summary>
    public string? UserAgent { get; init; }

    /// <summary>The IP address the submission came from.</summary>
    public string? IpAddress { get; init; }

    /// <summary>Whether the submission has already been reported to Akismet.</summary>
    public bool? AkismetSubmitted { get; init; }
}