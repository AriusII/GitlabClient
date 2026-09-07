using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the GitLab Duo resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IDuoService), typeof(IDuoClient))]
internal interface IDuoRepository
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