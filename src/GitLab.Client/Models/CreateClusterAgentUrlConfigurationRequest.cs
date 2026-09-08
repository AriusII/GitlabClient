namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/cluster_agents/:agent_id/url_configurations</c>. Introduced
///     in GitLab 17.4. An agent can hold only one URL configuration at a time.
/// </summary>
public sealed record CreateClusterAgentUrlConfigurationRequest
{
    /// <summary>The URL where the receptive agent is listening.</summary>
    public required Uri Url { get; init; }

    /// <summary>The client certificate, in PEM format, for mTLS.</summary>
    public string? ClientCert { get; init; }

    /// <summary>
    ///     The client key, in PEM format, for mTLS. Write-only: no read of a
    ///     <see cref="GitLabClusterAgentUrlConfiguration" /> ever echoes this back.
    /// </summary>
    public string? ClientKey { get; init; }

    /// <summary>The CA certificate, in PEM format, for TLS validation.</summary>
    public string? CaCert { get; init; }

    /// <summary>The host name for TLS validation.</summary>
    public string? TlsHost { get; init; }
}