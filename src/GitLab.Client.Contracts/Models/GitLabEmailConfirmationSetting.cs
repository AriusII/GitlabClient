using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>When and whether GitLab requires a new user to confirm their email address before signing in.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabEmailConfirmationSetting>))]
public enum GitLabEmailConfirmationSetting
{
    /// <summary>No confirmation is required.</summary>
    [JsonStringEnumMemberName("off")] Off,

    /// <summary>The account is usable immediately; GitLab asks for confirmation later.</summary>
    [JsonStringEnumMemberName("soft")] Soft,

    /// <summary>The account cannot be used until the email address is confirmed.</summary>
    [JsonStringEnumMemberName("hard")] Hard
}