using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     The chat-callback half of the Integrations area: the endpoints Slack and Mattermost call back
///     into, which are the only operations in the tag that are not "read/write/disable one integration".
/// </summary>
internal partial interface IIntegrationsRepository
{
    Task ReceiveSlackEventAsync(SlackEventRequest request, CancellationToken cancellationToken = default);

    Task ProcessSlackInteractionAsync(CancellationToken cancellationToken = default);

    Task GetSlackOptionsAsync(CancellationToken cancellationToken = default);

    Task TriggerSlackCommandAsync(TriggerSlackCommandRequest request,
        CancellationToken cancellationToken = default);

    Task TriggerMattermostSlashCommandAsync(ProjectId projectId, TriggerMattermostSlashCommandRequest request,
        CancellationToken cancellationToken = default);
}