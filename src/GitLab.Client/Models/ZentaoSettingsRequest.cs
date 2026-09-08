namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/services/zentao</c> - GitLab's older path spelling for the
///     ZenTao integration (use ZenTao as the project's external issue tracker).
/// </summary>
public sealed record ZentaoSettingsRequest
{
    /// <summary>Base URL of the ZenTao instance.</summary>
    public required Uri Url { get; init; }

    /// <summary>If different from Web URL.</summary>
    public Uri? ApiUrl { get; init; }

    public required string ApiToken { get; init; }

    public required string ZentaoProductXid { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}