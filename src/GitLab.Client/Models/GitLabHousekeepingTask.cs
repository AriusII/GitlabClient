using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Which housekeeping task <c>POST /projects/:id/housekeeping</c> should run.</summary>
/// <remarks>
///     The vendored spec renders these two values as the Ruby symbols <c>:eager</c> and <c>:prune</c>,
///     which is an artefact of how the Grape parameter is declared - GitLab coerces the request value
///     with <c>to_sym</c>, so what actually has to go on the wire is the bare <c>eager</c>/<c>prune</c>
///     that the endpoint's own description uses.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabHousekeepingTask>))]
public enum GitLabHousekeepingTask
{
    /// <summary>Run eager housekeeping - a full repacking pass.</summary>
    [JsonStringEnumMemberName("eager")] Eager,

    /// <summary>Prune unreachable objects.</summary>
    [JsonStringEnumMemberName("prune")] Prune
}