using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     The chat-callback endpoints of the Integrations area. These exist to be called <i>by</i> Slack
///     and Mattermost rather than by an application, so they are wrapped for completeness and for
///     testing an instance's chat wiring - not because a typical caller reaches for them.
///     <para>
///         None of them declares a response schema in the spec, so all four return no value. In
///         particular <c>POST /integrations/slack/events</c> answers Slack's URL-verification handshake
///         with a challenge body that the spec does not describe and this client therefore does not
///         surface.
///     </para>
/// </summary>
public partial interface IIntegrationsClient
{
    /// <summary>
    ///     Delivers a Slack Events API callback to GitLab
    ///     (<c>POST /integrations/slack/events</c>).
    /// </summary>
    Task ReceiveSlackEventAsync(SlackEventRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Processes a Slack interactive-component event
    ///     (<c>POST /integrations/slack/interactions</c>). Slack signs and form-encodes the real payload;
    ///     the spec declares no request body, so none is sent.
    /// </summary>
    Task ProcessSlackInteractionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks GitLab for the options of a Slack interactive component
    ///     (<c>POST /integrations/slack/options</c>). As with the interactions endpoint, the spec declares
    ///     neither a request nor a response body.
    /// </summary>
    Task GetSlackOptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Runs a global GitLab slash command from Slack (<c>POST /slack/trigger</c>), routed through the
    ///     GitLab for Slack app rather than through one project's webhook.
    /// </summary>
    Task TriggerSlackCommandAsync(TriggerSlackCommandRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Runs a GitLab slash command from Mattermost against one project
    ///     (<c>POST /projects/:id/integrations/mattermost_slash_commands/trigger</c>). Note GitLab's own
    ///     inconsistency: this route underscores the slug that the integration itself hyphenates.
    /// </summary>
    Task TriggerMattermostSlashCommandAsync(ProjectId projectId, TriggerMattermostSlashCommandRequest request,
        CancellationToken cancellationToken = default);
}