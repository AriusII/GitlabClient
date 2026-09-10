namespace GitLab.Client.Models;

/// <summary>
///     An open source license template, as returned by <c>GET /templates/licenses</c> and
///     <c>GET /templates/licenses/:key</c>. Unlike the plain <see cref="GitLabTemplate" /> shape this
///     carries the full <see href="https://choosealicense.com/">choosealicense.com</see> metadata, and
///     the list endpoint returns whole license objects rather than name/key pairs.
/// </summary>
public sealed record GitLabLicenseTemplate
{
    /// <summary>The SPDX-style key GitLab identifies the license by - <c>mit</c>, <c>gpl-3.0</c>, <c>apache-2.0</c>.</summary>
    public required string Key { get; init; }

    /// <summary>
    ///     The license's human-readable display name - <c>GNU General Public License v3.0</c>. Use
    ///     <see cref="Key" />, not this value, with the retrieve endpoint.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>The short form the license is usually cited as - <c>GNU GPLv3</c>. Null for licenses that have none.</summary>
    public string? Nickname { get; init; }

    /// <summary>The choosealicense.com page describing the license in prose.</summary>
    public Uri? HtmlUrl { get; init; }

    /// <summary>The canonical upstream text of the license, where its metadata records one.</summary>
    public Uri? SourceUrl { get; init; }

    /// <summary>Whether GitLab lists the license among the popular ones - the same set <c>popular=true</c> filters to.</summary>
    public bool? Popular { get; init; }

    /// <summary>A one-line human summary of what the license does.</summary>
    public string? Description { get; init; }

    /// <summary>What a user of the work must do - <c>include-copyright</c>, <c>document-changes</c>.</summary>
    public IReadOnlyList<string>? Conditions { get; init; }

    /// <summary>What the license grants - <c>commercial-use</c>, <c>modifications</c>, <c>distribution</c>.</summary>
    public IReadOnlyList<string>? Permissions { get; init; }

    /// <summary>What the license withholds - <c>liability</c>, <c>warranty</c>, <c>trademark-use</c>.</summary>
    public IReadOnlyList<string>? Limitations { get; init; }

    /// <summary>
    ///     The license text itself. On the retrieve endpoint the copyright placeholders are already
    ///     substituted when <c>project</c>/<c>fullname</c> were supplied.
    /// </summary>
    public string? Content { get; init; }
}