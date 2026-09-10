namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Bamboo integration (<c>PUT /projects/:id/integrations/bamboo</c>).</summary>
public sealed record BambooIntegrationRequest
{
    /// <summary>Enables SSL certificate verification.</summary>
    public bool? EnableSslVerification { get; init; }

    /// <summary>Bamboo root URL.</summary>
    public required Uri BambooUrl { get; init; }

    /// <summary>Bamboo build plan key.</summary>
    public required string BuildKey { get; init; }

    /// <summary>User with API access to the Bamboo server.</summary>
    public required string Username { get; init; }

    /// <summary>Password of the user.</summary>
    public required string Password { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}