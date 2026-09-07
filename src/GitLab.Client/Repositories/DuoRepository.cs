using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class DuoRepository(IGitLabApiConnection connection) : IDuoRepository
{
    public Task<JsonElement> GenerateCodeCompletionAsync(GenerateCodeCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("code_suggestions").Literal("completions").Build(),
            request,
            GitLabJsonContext.Default.GenerateCodeCompletionRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetCodeSuggestionsConnectionDetailsAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("code_suggestions").Literal("connection_details").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetCodeSuggestionsDirectAccessAsync(CodeSuggestionsDirectAccessRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        Uri route = GitLabRouteBuilder.Create("code_suggestions").Literal("direct_access").Build();

        // The request body is declared optional, so an omitted one is sent as no body at all rather than
        // as "{}" - Grape distinguishes the two when it validates declared parameters.
        return request is null
            ? connection.PostAsync(route, GitLabJsonContext.Default.JsonElement, cancellationToken)
            : connection.PostAsync(
                route,
                request,
                GitLabJsonContext.Default.CodeSuggestionsDirectAccessRequest,
                GitLabJsonContext.Default.JsonElement,
                cancellationToken);
    }

    public Task ValidateCodeSuggestionsEnabledAsync(CodeSuggestionsEnabledRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("code_suggestions").Literal("enabled").Build(),
            request,
            GitLabJsonContext.Default.CodeSuggestionsEnabledRequest,
            cancellationToken);
    }

    public Task<JsonElement> GenerateGitCommandAsync(GenerateGitCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("ai").Literal("llm").Literal("git_command").Build(),
            request,
            GitLabJsonContext.Default.GenerateGitCommandRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetThirdPartyAgentsDirectAccessAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("ai").Literal("third_party_agents").Literal("direct_access").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> ChatAsync(DuoChatRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("chat").Literal("completions").Build(),
            request,
            GitLabJsonContext.Default.DuoChatRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> EvaluateCodeReviewAsync(EvaluateCodeReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("duo_code_review").Literal("evaluations").Build(),
            request,
            GitLabJsonContext.Default.EvaluateCodeReviewRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<GitLabGlqlResult> ExecuteGlqlQueryAsync(ExecuteGlqlQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("glql").Build(),
            request,
            GitLabJsonContext.Default.ExecuteGlqlQueryRequest,
            GitLabJsonContext.Default.GitLabGlqlResult,
            cancellationToken);
    }

    public Task<JsonElement> GetGlqlSchemaAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("glql").Literal("schema").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> SendMcpRequestAsync(McpJsonRpcRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("mcp").Build(),
            request,
            GitLabJsonContext.Default.McpJsonRpcRequest,
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> ListenMcpAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("mcp").Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }
}