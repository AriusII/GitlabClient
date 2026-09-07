namespace GitLab.Client.Models;

/// <summary>
///     One custom HTTP header sent with a webhook request.
///     <para>
///         As with <see cref="GitLabHookUrlVariable" />, GitLab returns only the <see cref="Key" />: header
///         values routinely carry credentials, so <see cref="Value" /> is write-only and comes back null.
///     </para>
/// </summary>
public sealed record GitLabHookCustomHeader
{
    /// <summary>The header name, for example <c>X-Custom-Header</c>.</summary>
    public required string Key { get; init; }

    /// <summary>The header value. Never returned by GitLab; required when writing.</summary>
    public string? Value { get; init; }
}