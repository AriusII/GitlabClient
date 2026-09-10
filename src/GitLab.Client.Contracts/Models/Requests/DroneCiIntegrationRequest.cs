namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Drone CI integration (<c>PUT /projects/:id/integrations/drone-ci</c>) - runs
///     Drone CI builds as the project's CI.
/// </summary>
public sealed record DroneCiIntegrationRequest
{
    /// <summary>Enable SSL verification. Defaults to <c>true</c> (enabled).</summary>
    public bool? EnableSslVerification { get; init; }

    /// <summary>Drone CI URL (for example, <c>http://drone.example.com</c>).</summary>
    public required Uri DroneUrl { get; init; }

    /// <summary>Drone CI token.</summary>
    public required string Token { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Trigger event when a merge request is created, updated, or merged.</summary>
    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Trigger event for new tags pushed to the repository.</summary>
    public bool? TagPushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}