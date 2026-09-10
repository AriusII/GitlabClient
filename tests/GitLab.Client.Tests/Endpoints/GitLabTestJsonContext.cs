using System.Text.Json.Serialization;

namespace GitLab.Client.Tests.Endpoints;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TestSlackSettings))]
[JsonSerializable(typeof(string))]
internal sealed partial class GitLabTestJsonContext : JsonSerializerContext;