namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/services/squash-tm</c> - GitLab's older path spelling for
///     the Squash TM integration (requirements synchronisation with Squash Test Management).
/// </summary>
public sealed record SquashTmSettingsRequest
{
    /// <summary>URL of the Squash TM webhook.</summary>
    public required Uri Url { get; init; }

    /// <summary>Secret token.</summary>
    public string? Token { get; init; }

    /// <summary>Trigger event when a work item is created, updated, or closed.</summary>
    public bool? IssuesEvents { get; init; }

    /// <summary>Trigger event when a confidential work item is created, updated, or closed.</summary>
    public bool? ConfidentialIssuesEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}