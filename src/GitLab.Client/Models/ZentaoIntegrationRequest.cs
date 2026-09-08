namespace GitLab.Client.Models;

/// <summary>
///     Settings for the ZenTao integration (<c>PUT /projects/:id/integrations/zentao</c>) - use ZenTao
///     as the project's external issue tracker.
/// </summary>
public sealed record ZentaoIntegrationRequest
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