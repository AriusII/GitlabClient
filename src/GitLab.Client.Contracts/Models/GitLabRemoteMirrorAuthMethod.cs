using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Authentication method for a remote mirror.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabRemoteMirrorAuthMethod>))]
public enum GitLabRemoteMirrorAuthMethod
{
    /// <summary>Uses an SSH public key registered by GitLab.</summary>
    [JsonStringEnumMemberName("ssh_public_key")]
    SshPublicKey,

    /// <summary>Uses the credentials embedded in the remote URL.</summary>
    [JsonStringEnumMemberName("password")] Password
}