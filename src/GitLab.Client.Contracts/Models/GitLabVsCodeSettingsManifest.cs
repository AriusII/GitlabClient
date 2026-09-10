namespace GitLab.Client.Models;

/// <summary>
///     The Settings Sync manifest (<c>GET /vscode/settings_sync/v1/manifest</c>) - what VS Code polls to
///     decide whether anything on the server is newer than what it holds locally.
///     <para>
///         Every member is nullable: the GitLab spec derives this tag's schema from Grape defaults rather
///         than declared documentation, so a member missing from a response must not be a deserialization
///         failure. Treat both values as opaque tokens to be echoed back, not as numbers to reason about.
///     </para>
/// </summary>
public sealed record GitLabVsCodeSettingsManifest
{
    /// <summary>The latest resource version known to the server, per synced resource kind.</summary>
    public string? Latest { get; init; }

    /// <summary>The sync session this manifest belongs to. A changed session means the client must re-sync from scratch.</summary>
    public string? Session { get; init; }
}