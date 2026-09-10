using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The HTTP verb GitLab uses to push a finished export archive to <see cref="GitLabProjectExportUpload.Url" />.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabProjectExportUploadMethod>))]
public enum GitLabProjectExportUploadMethod
{
    /// <summary><c>PUT</c>, which is what an S3-compatible pre-signed URL expects. GitLab's default.</summary>
    [JsonStringEnumMemberName("PUT")] Put,

    /// <summary><c>POST</c>.</summary>
    [JsonStringEnumMemberName("POST")] Post
}