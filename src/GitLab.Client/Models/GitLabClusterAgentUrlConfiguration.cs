namespace GitLab.Client.Models;

/// <summary>
///     A URL configuration for a receptive cluster agent
///     (<c>/projects/:id/cluster_agents/:agent_id/url_configurations</c>) - the address and TLS material
///     GitLab uses to connect <em>to</em> the agent, rather than waiting for the agent to dial out.
///     Introduced in GitLab 17.4. An agent can hold only one URL configuration at a time.
///     <para>
///         Carries <see cref="PublicKey" /> but never the client's private key: GitLab accepts
///         <c>client_key</c> on create (see <see cref="CreateClusterAgentUrlConfigurationRequest.ClientKey" />)
///         but never echoes it back on any read.
///     </para>
/// </summary>
public sealed record GitLabClusterAgentUrlConfiguration
{
    public required long Id { get; init; }

    /// <summary>The agent this URL configuration belongs to.</summary>
    public long? AgentId { get; init; }

    /// <summary>The URL where the receptive agent is listening.</summary>
    public Uri? Url { get; init; }

    /// <summary>The public half of the mTLS client certificate's key pair.</summary>
    public string? PublicKey { get; init; }

    /// <summary>The client certificate, in PEM format, used for mTLS.</summary>
    public string? ClientCert { get; init; }

    /// <summary>The CA certificate, in PEM format, used to validate the agent's TLS certificate.</summary>
    public string? CaCert { get; init; }

    /// <summary>The host name TLS validation is performed against.</summary>
    public string? TlsHost { get; init; }
}