using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>How the Auto DevOps pipeline deploys to production (<c>auto_devops_deploy_strategy</c>).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabAutoDevopsDeployStrategy>))]
public enum GitLabAutoDevopsDeployStrategy
{
    /// <summary>Deploy every successful pipeline on the default branch.</summary>
    [JsonStringEnumMemberName("continuous")]
    Continuous,

    /// <summary>Deploy only when someone runs the manual deployment job.</summary>
    [JsonStringEnumMemberName("manual")] Manual,

    /// <summary>Deploy after a fixed delay - an incremental rollout.</summary>
    [JsonStringEnumMemberName("timed_incremental")]
    TimedIncremental
}