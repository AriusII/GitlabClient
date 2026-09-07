using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The indexing state a namespace transitions to via
///     <c>PUT /admin/active_context/code/enabled_namespaces</c>. GitLab's spec enumerates exactly these
///     two values for the request; the value the namespace reports back is a bare string - see the
///     remarks on <see cref="GitLabActiveContextCodeEnabledNamespace.State" />.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabActiveContextNamespaceState>))]
public enum GitLabActiveContextNamespaceState
{
    [JsonStringEnumMemberName("pending")] Pending,

    [JsonStringEnumMemberName("ready")] Ready
}