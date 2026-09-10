namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Emails on Push integration
///     (<c>PUT /projects/:id/integrations/emails-on-push</c>) - mails a commit diff to a list of
///     recipients.
/// </summary>
public sealed record EmailsOnPushIntegrationRequest
{
    /// <summary>Send from committer.</summary>
    public bool? SendFromCommitterEmail { get; init; }

    /// <summary>Disable code diffs.</summary>
    public bool? DisableDiffs { get; init; }

    /// <summary>
    ///     Branches to send notifications for. Valid options are <c>all</c>, <c>default</c>,
    ///     <c>protected</c>, and <c>default_and_protected</c>. Defaults to <c>default</c>.
    /// </summary>
    public string? BranchesToBeNotified { get; init; }

    /// <summary>Emails separated by whitespace.</summary>
    public required string Recipients { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Trigger event for new tags pushed to the repository.</summary>
    public bool? TagPushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}