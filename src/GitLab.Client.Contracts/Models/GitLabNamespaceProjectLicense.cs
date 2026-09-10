namespace GitLab.Client.Models;

/// <summary>The open-source license embedded in <see cref="GitLabNamespaceProject" />, when known.</summary>
public sealed record GitLabNamespaceProjectLicense
{
    /// <summary>The license's SPDX-style key, for example "gpl-3.0".</summary>
    public string? Key { get; init; }

    public string? Name { get; init; }

    public string? Nickname { get; init; }

    public Uri? HtmlUrl { get; init; }

    public Uri? SourceUrl { get; init; }
}