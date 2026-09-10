using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Markup format accepted for a wiki page's <c>format</c> field.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabWikiFormat>))]
public enum GitLabWikiFormat
{
    /// <summary>GitLab Flavored Markdown, the default format.</summary>
    [JsonStringEnumMemberName("markdown")] Markdown,

    /// <summary>RDoc markup.</summary>
    [JsonStringEnumMemberName("rdoc")] Rdoc,

    /// <summary>AsciiDoc markup.</summary>
    [JsonStringEnumMemberName("asciidoc")] Asciidoc,

    /// <summary>Org-mode markup.</summary>
    [JsonStringEnumMemberName("org")] Org
}