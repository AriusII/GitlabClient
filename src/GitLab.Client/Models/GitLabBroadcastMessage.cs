namespace GitLab.Client.Models;

/// <summary>
///     An instance-wide broadcast banner or notification, as returned by the GitLab Broadcast Messages
///     API (<c>/broadcast_messages</c>). Administrator-only; every operation on this resource requires
///     the Administrator role.
///     <para>
///         <see cref="BroadcastType" /> and <see cref="Theme" /> stay <see cref="string" /> here even
///         though the create/update request bodies enumerate their vocabulary
///         (<see cref="GitLabBroadcastMessageType" />, <see cref="GitLabBroadcastMessageTheme" />):
///         GitLab's own response schema types both as a bare string with no enum, so a value the server
///         adds later must not turn a healthy read into a <see cref="System.Text.Json.JsonException" />.
///     </para>
/// </summary>
public sealed record GitLabBroadcastMessage
{
    public required long Id { get; init; }

    public required string Message { get; init; }

    public DateTimeOffset? StartsAt { get; init; }

    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>Deprecated by GitLab in favour of <see cref="Theme" />.</summary>
    public string? Color { get; init; }

    /// <summary>Deprecated by GitLab in favour of <see cref="Theme" />.</summary>
    public string? Font { get; init; }

    /// <summary>
    ///     The access levels this message targets - <c>10</c> Guest, <c>15</c> Planner, <c>20</c> Reporter,
    ///     <c>30</c> Developer, <c>40</c> Maintainer, <c>50</c> Owner. Empty or null targets every role.
    /// </summary>
    public IReadOnlyList<int>? TargetAccessLevels { get; init; }

    /// <summary>A glob restricting the message to matching pages, for example <c>*/welcome</c>.</summary>
    public string? TargetPath { get; init; }

    /// <summary><c>banner</c> or <c>notification</c>. See the type-level remarks for why this is a bare string.</summary>
    public string? BroadcastType { get; init; }

    /// <summary>See the type-level remarks for why this is a bare string.</summary>
    public string? Theme { get; init; }

    public bool? Dismissable { get; init; }

    /// <summary>Whether the message currently falls within its <see cref="StartsAt" />/<see cref="EndsAt" /> window.</summary>
    public bool? Active { get; init; }
}