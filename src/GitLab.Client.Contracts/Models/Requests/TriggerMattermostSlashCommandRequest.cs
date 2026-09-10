namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of
///     <c>POST /projects/:id/integrations/mattermost_slash_commands/trigger</c> - the callback
///     Mattermost makes when someone runs a GitLab slash command in a channel.
/// </summary>
public sealed record TriggerMattermostSlashCommandRequest
{
    /// <summary>
    ///     The token Mattermost was configured with, which GitLab checks against the token stored on the
    ///     project's <c>mattermost-slash-commands</c> integration.
    /// </summary>
    public required string Token { get; init; }
}