namespace GitLab.Client.Models;

/// <summary>
///     One URL variable of a webhook - the named value interpolated into the hook's URL template
///     (<c>https://example.com/{token}</c>).
///     <para>
///         The same shape travels in both directions, but not the same content: a create/update request
///         carries <see cref="Key" /> and <see cref="Value" /> together, while GitLab only ever echoes the
///         <see cref="Key" /> back, leaving <see cref="Value" /> null. Variables exist precisely so a secret
///         can be kept out of the readable URL, so treat a null <see cref="Value" /> as "GitLab is holding
///         it", never as "it is unset".
///     </para>
/// </summary>
public sealed record GitLabHookUrlVariable
{
    /// <summary>The variable name, referenced from the hook URL as <c>{name}</c>.</summary>
    public required string Key { get; init; }

    /// <summary>The value substituted into the URL. Never returned by GitLab; required when writing.</summary>
    public string? Value { get; init; }
}