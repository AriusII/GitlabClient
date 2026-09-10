using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Prompt-injection protection behavior for GitLab Duo Agent Platform
///     (<c>prompt_injection_protection_level</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPromptInjectionProtectionLevel>))]
public enum GitLabPromptInjectionProtectionLevel
{
    [JsonStringEnumMemberName("no_checks")]
    NoChecks,

    [JsonStringEnumMemberName("log_only")] LogOnly,

    [JsonStringEnumMemberName("interrupt")]
    Interrupt
}