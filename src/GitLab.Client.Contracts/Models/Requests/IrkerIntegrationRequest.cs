namespace GitLab.Client.Models.Requests;

/// <summary>
///     Typed settings for the <c>irker</c> integration (IRC gateway) - see
///     <see cref="GitLabIntegrationSlug.Irker" />.
/// </summary>
public sealed record IrkerIntegrationRequest
{
    /// <summary>irker daemon hostname. The default value is <c>localhost</c>.</summary>
    public string? ServerHost { get; init; }

    /// <summary>irker daemon port. The default value is <c>6659</c>.</summary>
    public int? ServerPort { get; init; }

    /// <summary>URI to add before each recipient. The default value is <c>irc://irc.network.net:6697/</c>.</summary>
    public Uri? DefaultIrcUri { get; init; }

    /// <summary>Comma-separated list of channels or email addresses.</summary>
    public required string Recipients { get; init; }

    /// <summary>Colorize messages.</summary>
    public bool? ColorizeMessages { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}