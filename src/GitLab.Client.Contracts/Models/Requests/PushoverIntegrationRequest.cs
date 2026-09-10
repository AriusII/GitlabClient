namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Pushover integration (<c>PUT /projects/:id/integrations/pushover</c>).</summary>
public sealed record PushoverIntegrationRequest
{
    /// <summary>Pushover application API key.</summary>
    public required string ApiKey { get; init; }

    /// <summary>Pushover user key.</summary>
    public required string UserKey { get; init; }

    /// <summary>Optional target device.</summary>
    public string? Device { get; init; }

    /// <summary>Pushover priority.</summary>
    public required string Priority { get; init; }

    /// <summary>Notification sound.</summary>
    public string? Sound { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}