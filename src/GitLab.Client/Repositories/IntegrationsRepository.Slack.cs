using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     The chat-callback half of the Integrations repository. Split into its own partial file so the
///     per-slug settings files that follow have a worked example of the convention - and so the primary
///     constructor's <c>connection</c> parameter is proven to be in scope from a sibling part.
/// </summary>
internal sealed partial class IntegrationsRepository
{
    public Task ReceiveSlackEventAsync(SlackEventRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(Integrations).Literal("slack").Literal("events").Build(),
            request,
            GitLabJsonContext.Default.SlackEventRequest,
            cancellationToken);
    }

    public Task ProcessSlackInteractionAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(Integrations).Literal("slack").Literal("interactions").Build(),
            cancellationToken);
    }

    public Task GetSlackOptionsAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(Integrations).Literal("slack").Literal("options").Build(),
            cancellationToken);
    }

    public Task TriggerSlackCommandAsync(TriggerSlackCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("slack").Literal("trigger").Build(),
            request,
            GitLabJsonContext.Default.TriggerSlackCommandRequest,
            cancellationToken);
    }

    public Task TriggerMattermostSlashCommandAsync(ProjectId projectId,
        TriggerMattermostSlashCommandRequest request, CancellationToken cancellationToken = default)
    {
        // Underscores, not hyphens: the trigger route spells the slug "mattermost_slash_commands" while
        // the integration itself is addressed as "mattermost-slash-commands". Both are path template
        // words here, so both are Literal.
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Integrations)
                .Literal("mattermost_slash_commands").Literal("trigger").Build(),
            request,
            GitLabJsonContext.Default.TriggerMattermostSlashCommandRequest,
            cancellationToken);
    }
}