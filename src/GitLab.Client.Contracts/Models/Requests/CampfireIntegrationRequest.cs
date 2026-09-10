namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Campfire integration (<c>PUT /projects/:id/integrations/campfire</c>).</summary>
public sealed record CampfireIntegrationRequest
{
    /// <summary>Authentication token.</summary>
    public required string Token { get; init; }

    /// <summary>Campfire subdomain.</summary>
    public string? Subdomain { get; init; }

    /// <summary>Campfire room name.</summary>
    public string? Room { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}