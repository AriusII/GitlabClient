using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The authorization mode GitLab applies when adding a certificate-based Kubernetes cluster.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabClusterAuthorizationType>))]
public enum GitLabClusterAuthorizationType
{
    [JsonStringEnumMemberName("unknown_authorization")]
    UnknownAuthorization,

    [JsonStringEnumMemberName("rbac")] RoleBasedAccessControl,

    [JsonStringEnumMemberName("abac")] AttributeBasedAccessControl
}