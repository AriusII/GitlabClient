using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>The chat-callback half of the Integrations service contract.</summary>
internal partial interface IIntegrationsService
{
    Task ReceiveSlackEventAsync(SlackEventRequest request, CancellationToken cancellationToken = default);

    Task ProcessSlackInteractionAsync(CancellationToken cancellationToken = default);

    Task ProcessSlackOptionsAsync(CancellationToken cancellationToken = default);

    Task TriggerSlackCommandAsync(TriggerSlackCommandRequest request,
        CancellationToken cancellationToken = default);

    Task TriggerMattermostSlashCommandAsync(ProjectId projectId, TriggerMattermostSlashCommandRequest request,
        CancellationToken cancellationToken = default);
}