namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /broadcast_messages/:id</c>. Every member is optional; GitLab keeps the existing value
///     for anything omitted.
/// </summary>
public sealed record UpdateBroadcastMessageRequest
{
    public string? Message { get; init; }

    public DateTimeOffset? StartsAt { get; init; }

    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>Deprecated by GitLab in favour of <see cref="Theme" />.</summary>
    public string? Color { get; init; }

    /// <summary>Deprecated by GitLab in favour of <see cref="Theme" />.</summary>
    public string? Font { get; init; }

    /// <summary>
    ///     The access levels to target - <c>10</c> Guest, <c>15</c> Planner, <c>20</c> Reporter, <c>30</c>
    ///     Developer, <c>40</c> Maintainer, <c>50</c> Owner.
    /// </summary>
    public IReadOnlyList<int>? TargetAccessLevels { get; init; }

    /// <summary>A glob restricting the message to matching pages, for example <c>*/welcome</c>.</summary>
    public string? TargetPath { get; init; }

    public GitLabBroadcastMessageType? BroadcastType { get; init; }

    public bool? Dismissable { get; init; }

    public GitLabBroadcastMessageTheme? Theme { get; init; }
}