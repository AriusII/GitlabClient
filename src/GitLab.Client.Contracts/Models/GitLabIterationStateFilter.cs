using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>state</c> filter of the iteration listings (<c>GET /groups/:id/iterations</c>,
///     <c>GET /projects/:id/iterations</c>).
///     <para>
///         The spec also accepts <c>started</c>, which GitLab deprecated in favour of <c>current</c>; it is
///         deliberately not exposed here, so this enum cannot be used to write a call against a value that
///         is scheduled to disappear.
///     </para>
///     <para>
///         This is the query vocabulary, not the entity's state: <see cref="GitLabIteration.State" /> comes
///         back as an <b>integer</b> (1 upcoming, 2 current, 3 closed) and the two are not interchangeable.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabIterationStateFilter>))]
public enum GitLabIterationStateFilter
{
    [JsonStringEnumMemberName("opened")] Opened,

    [JsonStringEnumMemberName("upcoming")] Upcoming,

    [JsonStringEnumMemberName("current")] Current,

    [JsonStringEnumMemberName("closed")] Closed,

    [JsonStringEnumMemberName("all")] All
}