using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     The signature on a signed tag
///     (<c>GET /projects/:id/repository/tags/:tag_name/signature</c>). GitLab answers <c>404</c> for an
///     unsigned tag, which surfaces as a <see cref="Abstractions.Exceptions.GitLabNotFoundException" />.
/// </summary>
public sealed record GitLabTagSignature
{
    /// <summary>GitLab's own signature kind, for example <c>PGP</c>.</summary>
    public string? SignatureType { get; init; }

    /// <summary>
    ///     The complete signature object. GitLab intentionally leaves this object open in its 19.4 OpenAPI
    ///     schema, so callers can inspect its provider-specific members without this SDK discarding them.
    /// </summary>
    public JsonElement? Signature { get; init; }
}