namespace GitLab.Client.Models;

/// <summary>
///     A pointer to one stored revision of a VS Code Settings Sync resource
///     (<c>GET /vscode/settings_sync/v1/resource/:resource_name</c>), which lists the revisions rather
///     than returning their bodies.
///     <para>
///         Both members are nullable, and <see cref="Created" /> is a <see langword="string" /> rather
///         than a <see cref="DateTimeOffset" />, because the spec types this tag's fields from Grape's
///         defaults: the value has no declared format to bind against.
///     </para>
/// </summary>
public sealed record GitLabVsCodeSettingReference
{
    /// <summary>The URL of the revision - the retrieve endpoint for it, with the revision id already in the path.</summary>
    public Uri? Url { get; init; }

    /// <summary>When the revision was stored, as GitLab renders it.</summary>
    public string? Created { get; init; }
}