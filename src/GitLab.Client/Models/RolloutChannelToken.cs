namespace GitLab.Client.Models;

/// <summary>
///     One AutoFlow channel opened alongside a rollout event, carried in
///     <see cref="IngestRolloutEventRequest.ChannelTokens" /> so a later event can post a value back into
///     it.
/// </summary>
public sealed record RolloutChannelToken
{
    public required string ChannelName { get; init; }

    public required string Token { get; init; }
}