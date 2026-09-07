using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The visibility levels GitLab accepts on <c>POST /organizations</c>. Deliberately narrower than
///     <see cref="Domain.GitLabVisibility" />: the request schema for creating an organization
///     enumerates only these two values, with no "internal" option.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabOrganizationVisibility>))]
public enum GitLabOrganizationVisibility
{
    [JsonStringEnumMemberName("private")] Private,

    [JsonStringEnumMemberName("public")] Public
}