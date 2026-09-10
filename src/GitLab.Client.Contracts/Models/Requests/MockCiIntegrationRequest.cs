namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Mock CI integration (<c>PUT /projects/:id/integrations/mock-ci</c>).</summary>
public sealed record MockCiIntegrationRequest
{
    /// <summary>Enables SSL certificate verification.</summary>
    public bool? EnableSslVerification { get; init; }

    /// <summary>URL of the mock CI service.</summary>
    public required Uri MockServiceUrl { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}