using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class SuggestionsRepositoryTests
{
    private const string SuggestionJson = """
                                          {
                                            "id": 36,
                                            "from_line": 10,
                                            "to_line": 12,
                                            "appliable": false,
                                            "applied": true,
                                            "from_content": "    var x = 1;\n",
                                            "to_content": "    const int X = 1;\n"
                                          }
                                          """;

    [Fact]
    public async Task ApplyAsync_PutsToInstanceLevelApplyRoute_AndDeserializesSuggestion()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SuggestionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SuggestionsRepository repository = new(connection);

        GitLabSuggestion suggestion = await repository.ApplyAsync(36,
            new ApplySuggestionRequest { CommitMessage = "Apply reviewer suggestion" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/suggestions/36/apply",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("""{"commit_message":"Apply reviewer suggestion"}""", sentBody);

        Assert.Equal(36, suggestion.Id);
        Assert.Equal(10, suggestion.FromLine);
        Assert.Equal(12, suggestion.ToLine);
        Assert.False(suggestion.Appliable);
        Assert.True(suggestion.Applied);
        Assert.Equal("    var x = 1;\n", suggestion.FromContent);
        Assert.Equal("    const int X = 1;\n", suggestion.ToContent);
    }

    [Fact]
    public async Task ApplyAsync_SendsAnEmptyObject_WhenNoCommitMessageIsSupplied()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SuggestionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SuggestionsRepository repository = new(connection);

        await repository.ApplyAsync(36, new ApplySuggestionRequest(), TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/suggestions/36/apply",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("{}", sentBody);
    }

    /// <summary>
    ///     GitLab's OpenAPI document declares the batch response as a single suggestion object, not an array.
    ///     This pins that reading: if a live instance ever answers with an array, this is the test that fails
    ///     first and the signature that has to change.
    /// </summary>
    [Fact]
    public async Task ApplyBatchAsync_PutsIdsToBatchApplyRoute_AndDeserializesSingleSuggestion()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SuggestionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SuggestionsRepository repository = new(connection);

        ApplySuggestionBatchRequest request = new()
        {
            Ids = [36, 37, 38], CommitMessage = "Apply all reviewer suggestions"
        };

        GitLabSuggestion suggestion =
            await repository.ApplyBatchAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/suggestions/batch_apply",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"ids":[36,37,38],"commit_message":"Apply all reviewer suggestions"}""", sentBody);

        Assert.Equal(36, suggestion.Id);
        Assert.True(suggestion.Applied);
        Assert.Equal("    const int X = 1;\n", suggestion.ToContent);
    }

    [Fact]
    public async Task ApplyBatchAsync_OmitsCommitMessage_WhenNotSupplied()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SuggestionJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SuggestionsRepository repository = new(connection);

        await repository.ApplyBatchAsync(new ApplySuggestionBatchRequest { Ids = [36] },
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"ids":[36]}""", sentBody);
    }

    [Fact]
    public async Task ApplyAsync_OnForbiddenResponse_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SuggestionsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            repository.ApplyAsync(36, new ApplySuggestionRequest(), TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task ApplyBatchAsync_OnNotFoundResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Suggestion Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        SuggestionsRepository repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.ApplyBatchAsync(new ApplySuggestionBatchRequest { Ids = [999] },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Suggestion Not Found", exception.Message);
    }
}