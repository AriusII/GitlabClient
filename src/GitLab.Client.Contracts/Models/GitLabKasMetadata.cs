using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Status of the GitLab Agent Server for Kubernetes (KAS), nested in <see cref="GitLabMetadata.Kas" />.
///     <para>
///         GitLab's spec declares this object's members in camelCase rather than the snake_case every
///         other DTO in this library uses, so <see cref="ExternalUrl" /> and
///         <see cref="ExternalK8sProxyUrl" /> carry an explicit <see cref="JsonPropertyNameAttribute" />.
///     </para>
/// </summary>
public sealed record GitLabKasMetadata
{
    /// <summary>Whether KAS is enabled on this instance.</summary>
    public bool? Enabled { get; init; }

    /// <summary>The gRPC endpoint agents connect to, for example <c>grpc://gitlab.example.com:8150</c>.</summary>
    [JsonPropertyName("externalUrl")]
    [SuppressMessage("Design", "CA1056",
        Justification =
            "Kept as string like the rest of this nullable-permissive DTO; the scheme (grpc://) is non-standard for System.Uri.")]
    public string? ExternalUrl { get; init; }

    /// <summary>The Kubernetes API proxy endpoint, for example <c>https://gitlab.example.com:8150/k8s-proxy</c>.</summary>
    [JsonPropertyName("externalK8sProxyUrl")]
    [SuppressMessage("Design", "CA1056",
        Justification = "Kept as string like the rest of this nullable-permissive DTO.")]
    public string? ExternalK8sProxyUrl { get; init; }

    /// <summary>The running KAS version.</summary>
    public string? Version { get; init; }
}