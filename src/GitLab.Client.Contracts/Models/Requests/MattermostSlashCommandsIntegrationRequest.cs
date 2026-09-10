namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Mattermost slash-commands integration
///     (<c>PUT /projects/:id/integrations/mattermost-slash-commands</c>).
/// </summary>
public sealed record MattermostSlashCommandsIntegrationRequest
{
    /// <summary>Token used to authenticate commands from Mattermost.</summary>
    public required string Token { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}