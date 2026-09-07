using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /projects/:id/terraform/state/:name/lock</c> - Terraform's own lock-info document,
///     which the HTTP backend sends verbatim when it acquires a lock.
/// </summary>
/// <remarks>
///     Every member carries an explicit <see cref="JsonPropertyNameAttribute" /> because this is the one
///     GitLab payload that is not snake_case: it is Terraform's <c>LockInfo</c> struct, whose field names
///     are Go-style Pascal case (and <c>ID</c>, not <c>Id</c>). The library's snake_case naming policy
///     would silently rename all seven and GitLab would reject the request.
/// </remarks>
public sealed record LockTerraformStateRequest
{
    /// <summary>The lock id Terraform generated. The same value unlocks the state again.</summary>
    [JsonPropertyName("ID")]
    public required string Id { get; init; }

    /// <summary>The Terraform operation holding the lock - <c>OperationTypePlan</c>, <c>OperationTypeApply</c>.</summary>
    [JsonPropertyName("Operation")]
    public required string Operation { get; init; }

    /// <summary>Free-text information Terraform attaches to the lock.</summary>
    [JsonPropertyName("Info")]
    public required string Info { get; init; }

    /// <summary>Who took the lock, as <c>user@host</c>.</summary>
    [JsonPropertyName("Who")]
    public required string Who { get; init; }

    /// <summary>The Terraform version that took the lock.</summary>
    [JsonPropertyName("Version")]
    public required string Version { get; init; }

    /// <summary>
    ///     When the lock was taken. Left as a string on purpose - it is Terraform's own timestamp
    ///     formatting, echoed back untouched, not a GitLab timestamp.
    /// </summary>
    [JsonPropertyName("Created")]
    public required string Created { get; init; }

    /// <summary>The state path Terraform is locking.</summary>
    [JsonPropertyName("Path")]
    public required string Path { get; init; }
}