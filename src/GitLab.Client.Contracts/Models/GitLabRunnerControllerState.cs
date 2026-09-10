using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Whether a runner controller evaluates jobs, and whether it acts on that evaluation.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabRunnerControllerState>))]
public enum GitLabRunnerControllerState
{
    /// <summary>The controller is registered but evaluates nothing. GitLab's default for a new controller.</summary>
    [JsonStringEnumMemberName("disabled")] Disabled,

    /// <summary>The controller evaluates jobs in its scope and acts on the result.</summary>
    [JsonStringEnumMemberName("enabled")] Enabled,

    /// <summary>The controller evaluates jobs in its scope but takes no action - the rehearsal mode.</summary>
    [JsonStringEnumMemberName("dry_run")] DryRun
}