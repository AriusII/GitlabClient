using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     What an external-forge importer does when a stage times out
///     (<c>POST /import/github</c>, <c>POST /import/bitbucket_server</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabProjectImportTimeoutStrategy>))]
public enum GitLabProjectImportTimeoutStrategy
{
    /// <summary>Continue to the next stage and finish the import with whatever was fetched.</summary>
    [JsonStringEnumMemberName("optimistic")]
    Optimistic,

    /// <summary>Fail the import. The default.</summary>
    [JsonStringEnumMemberName("pessimistic")]
    Pessimistic
}