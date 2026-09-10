namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Assembla integration (<c>PUT /projects/:id/integrations/assembla</c>).</summary>
public sealed record AssemblaIntegrationRequest
{
    /// <summary>Authentication token.</summary>
    public required string Token { get; init; }

    /// <summary>Assembla subdomain.</summary>
    public string? Subdomain { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}