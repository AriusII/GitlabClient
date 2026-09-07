using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Which kind of source <c>GET /users/:id/memberships</c> is restricted to. GitLab spells these
///     capitalised on the wire, matching its internal source-type discriminator rather than its usual
///     snake_case query vocabulary.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabUserMembershipType>))]
public enum GitLabUserMembershipType
{
    /// <summary>Project memberships only.</summary>
    [JsonStringEnumMemberName("Project")] Project,

    /// <summary>Group memberships only - GitLab calls a group a namespace here.</summary>
    [JsonStringEnumMemberName("Namespace")]
    Namespace
}