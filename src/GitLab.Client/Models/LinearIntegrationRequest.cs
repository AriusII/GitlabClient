namespace GitLab.Client.Models;

/// <summary>
///     Typed settings for the <c>linear</c> integration - see
///     <see cref="GitLabIntegrationSlug.Linear" />.
/// </summary>
public sealed record LinearIntegrationRequest
{
    /// <summary>Linear workspace URL (for example, <c>https://linear.app/example</c>).</summary>
    public required Uri WorkspaceUrl { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}