using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     A single file's diff inside a <see cref="GitLabCompare" /> result.
/// </summary>
public sealed record GitLabDiff
{
    /// <summary>The unified diff text for this file.</summary>
    public string? Diff { get; init; }

    public bool? Collapsed { get; init; }

    public bool? TooLarge { get; init; }

    public string? NewPath { get; init; }

    public string? OldPath { get; init; }

    /// <summary>
    ///     The Git file mode on the "from" side. The wire name is spelled out rather than left to the
    ///     context's snake_case policy: a one-letter PascalCase prefix is exactly the case where the
    ///     derived name is not obvious, and a wrong guess deserializes silently to <c>null</c>.
    /// </summary>
    [JsonPropertyName("a_mode")]
    public string? AMode { get; init; }

    /// <summary>The Git file mode on the "to" side. See <see cref="AMode" /> for why the name is explicit.</summary>
    [JsonPropertyName("b_mode")]
    public string? BMode { get; init; }

    public bool? NewFile { get; init; }

    public bool? RenamedFile { get; init; }

    public bool? DeletedFile { get; init; }

    public bool? GeneratedFile { get; init; }
}