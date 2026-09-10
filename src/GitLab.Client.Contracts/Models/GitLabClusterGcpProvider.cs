namespace GitLab.Client.Models;

/// <summary>
///     Google Kubernetes Engine details embedded in a certificate-based cluster response
///     (<c>APIEntitiesProviderGcp</c>).
/// </summary>
public sealed record GitLabClusterGcpProvider
{
    public string? ClusterId { get; init; }

    public string? StatusName { get; init; }

    public string? GcpProjectId { get; init; }

    public string? Zone { get; init; }

    public string? MachineType { get; init; }

    public string? NumNodes { get; init; }

    public Uri? Endpoint { get; init; }
}