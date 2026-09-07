using System.Text.Json;

using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for GitLab Duo, sitting between the public <c>IDuoClient</c>
///     controller and <c>IDuoRepository</c>'s raw GitLab access. Mirrors the repository's method shapes
///     1:1 today (its implementation is generated); this is the seam where request validation, caching, or
///     cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface IDuoService
{
    Task<JsonElement> GenerateCodeCompletionAsync(GenerateCodeCompletionRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetCodeSuggestionsConnectionDetailsAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> GetCodeSuggestionsDirectAccessAsync(CodeSuggestionsDirectAccessRequest? request = null,
        CancellationToken cancellationToken = default);

    Task ValidateCodeSuggestionsEnabledAsync(CodeSuggestionsEnabledRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GenerateGitCommandAsync(GenerateGitCommandRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetThirdPartyAgentsDirectAccessAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> ChatAsync(DuoChatRequest request, CancellationToken cancellationToken = default);

    Task<JsonElement> EvaluateCodeReviewAsync(EvaluateCodeReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabGlqlResult> ExecuteGlqlQueryAsync(ExecuteGlqlQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonElement> GetGlqlSchemaAsync(CancellationToken cancellationToken = default);

    Task<JsonElement> SendMcpRequestAsync(McpJsonRpcRequest request, CancellationToken cancellationToken = default);

    Task<JsonElement> ListenMcpAsync(CancellationToken cancellationToken = default);
}