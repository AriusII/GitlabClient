namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>POST /slack/trigger</c> - a GitLab slash command typed in Slack, routed through
///     the GitLab for Slack app rather than through one project's webhook.
/// </summary>
public sealed record TriggerSlackCommandRequest
{
    /// <summary>
    ///     The command text exactly as Slack captured it, without the leading slash - for example
    ///     <c>issue show 42</c>.
    /// </summary>
    public required string Text { get; init; }
}