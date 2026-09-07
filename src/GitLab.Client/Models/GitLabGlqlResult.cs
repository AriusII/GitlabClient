namespace GitLab.Client.Models;

/// <summary>
///     The result of <c>POST /glql</c>. Every member is nullable on purpose: GLQL reports a bad query as a
///     <c>200</c> carrying <see cref="Success" /> <c>false</c> and an <see cref="Error" />, not as an HTTP
///     error, so a failed query deserializes cleanly and must be checked rather than caught.
/// </summary>
public sealed record GitLabGlqlResult
{
    /// <summary>Whether the query ran. Check this before reading <see cref="Data" />.</summary>
    public bool? Success { get; init; }

    /// <summary>What was found, with its pagination cursors. Null when <see cref="Success" /> is <c>false</c>.</summary>
    public GitLabGlqlData? Data { get; init; }

    /// <summary>Why the query failed - a parse or validation message from GLQL itself.</summary>
    public string? Error { get; init; }

    /// <summary>The columns the query projected, in order. The key to reading <see cref="GitLabGlqlData.Nodes" />.</summary>
    public IReadOnlyList<GitLabGlqlField>? Fields { get; init; }
}