namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Packagist integration (<c>PUT /projects/:id/integrations/packagist</c>).</summary>
public sealed record PackagistIntegrationRequest
{
    /// <summary>Packagist username.</summary>
    public required string Username { get; init; }

    /// <summary>Packagist API token.</summary>
    public required string Token { get; init; }

    /// <summary>Packagist server.</summary>
    public string? Server { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Triggers events for merge requests.</summary>
    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Triggers events for pushed tags.</summary>
    public bool? TagPushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}