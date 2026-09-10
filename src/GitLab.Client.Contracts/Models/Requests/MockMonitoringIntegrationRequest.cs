namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Mock monitoring integration (<c>PUT /projects/:id/integrations/mock-monitoring</c>).
/// </summary>
public sealed record MockMonitoringIntegrationRequest
{
    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}