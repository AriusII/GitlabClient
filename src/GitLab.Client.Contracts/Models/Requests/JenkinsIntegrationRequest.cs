namespace GitLab.Client.Models.Requests;

/// <summary>
///     Typed settings for the <c>jenkins</c> integration - see
///     <see cref="GitLabIntegrationSlug.Jenkins" />.
/// </summary>
public sealed record JenkinsIntegrationRequest
{
    /// <summary>Enable SSL verification. Defaults to <see langword="true" /> (enabled).</summary>
    public bool? EnableSslVerification { get; init; }

    /// <summary>URL of the Jenkins server.</summary>
    public required Uri JenkinsUrl { get; init; }

    /// <summary>Name of the Jenkins project.</summary>
    public required string ProjectName { get; init; }

    /// <summary>Username of the Jenkins server.</summary>
    public string? Username { get; init; }

    /// <summary>Password of the Jenkins server.</summary>
    public string? Password { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Trigger event when a merge request is created, updated, or merged.</summary>
    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Trigger event for new tags pushed to the repository.</summary>
    public bool? TagPushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}