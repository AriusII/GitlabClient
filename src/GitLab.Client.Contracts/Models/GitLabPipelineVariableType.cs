using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The two variable forms accepted while creating a pipeline.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPipelineVariableType>))]
public enum GitLabPipelineVariableType
{
    /// <summary>Supplies the value in the pipeline environment.</summary>
    [JsonStringEnumMemberName("env_var")] EnvironmentVariable,

    /// <summary>Writes the value to a temporary file and supplies that file's path.</summary>
    [JsonStringEnumMemberName("file")] File
}