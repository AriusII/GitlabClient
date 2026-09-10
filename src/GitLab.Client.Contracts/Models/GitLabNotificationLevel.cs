using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The notification level vocabulary shared by the instance, group and project notification-settings
///     endpoints (<c>/notification_settings</c>, <c>/groups/:id/notification_settings</c>,
///     <c>/projects/:id/notification_settings</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabNotificationLevel>))]
public enum GitLabNotificationLevel
{
    /// <summary>No notifications at all.</summary>
    [JsonStringEnumMemberName("disabled")] Disabled,

    /// <summary>Notifies only for things the user is participating in - authored, assigned, or mentioned.</summary>
    [JsonStringEnumMemberName("participating")]
    Participating,

    /// <summary>Notifies for everything happening in the watched group or project.</summary>
    [JsonStringEnumMemberName("watch")] Watch,

    /// <summary>Defers to the higher-scoped setting - a group falling back to the global level, for one.</summary>
    [JsonStringEnumMemberName("global")] Global,

    /// <summary>Notifies only when the user is directly mentioned.</summary>
    [JsonStringEnumMemberName("mention")] Mention,

    /// <summary>
    ///     Notifies according to the individual event toggles on <see cref="GitLabNotificationSettings" />
    ///     rather than a fixed rule.
    /// </summary>
    [JsonStringEnumMemberName("custom")] Custom
}