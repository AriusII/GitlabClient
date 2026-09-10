namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Buildkite integration (<c>PUT /projects/:id/integrations/buildkite</c>).</summary>
public sealed record BuildkiteIntegrationRequest
{
    /// <summary>Buildkite pipeline URL.</summary>
    public required Uri ProjectUrl { get; init; }

    /// <summary>Token issued for the Buildkite pipeline.</summary>
    public required string Token { get; init; }

    /// <summary>Deprecated GitLab setting; SSL verification is always enabled.</summary>
    public bool? EnableSslVerification { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Triggers events for merge requests.</summary>
    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Triggers events for pushed tags.</summary>
    public bool? TagPushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}