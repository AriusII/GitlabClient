using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Infrastructure;

public sealed class GitLabApiExceptionMappingTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");
    private static readonly Uri ProjectRoute = new("projects/42", UriKind.Relative);
    private static readonly string[] ExpectedLabelErrors = ["is invalid", "is too long"];

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, typeof(GitLabAuthenticationException))]
    [InlineData(HttpStatusCode.Forbidden, typeof(GitLabForbiddenException))]
    [InlineData(HttpStatusCode.NotFound, typeof(GitLabNotFoundException))]
    [InlineData(HttpStatusCode.Conflict, typeof(GitLabConflictException))]
    [InlineData(HttpStatusCode.BadRequest, typeof(GitLabValidationException))]
    [InlineData(HttpStatusCode.UnprocessableEntity, typeof(GitLabValidationException))]
    [InlineData(HttpStatusCode.TooManyRequests, typeof(GitLabRateLimitExceededException))]
    [InlineData(HttpStatusCode.InternalServerError, typeof(GitLabServerException))]
    [InlineData(HttpStatusCode.BadGateway, typeof(GitLabServerException))]
    [InlineData(HttpStatusCode.ServiceUnavailable, typeof(GitLabServerException))]
    [InlineData(HttpStatusCode.GatewayTimeout, typeof(GitLabServerException))]
    public async Task StatusCode_MapsToDedicatedExceptionType(HttpStatusCode statusCode, Type expected)
    {
        GitLabApiException exception = await CaptureAsync(statusCode, """{ "message": "boom" }""");

        Assert.Equal(expected, exception.GetType());
        Assert.Equal(statusCode, exception.StatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.PaymentRequired)]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Gone)]
    [InlineData(HttpStatusCode.UnsupportedMediaType)]
    public async Task UnmappedStatusCode_FallsBackToBaseType(HttpStatusCode statusCode)
    {
        GitLabApiException exception = await CaptureAsync(statusCode, """{ "message": "boom" }""");

        Assert.Equal(typeof(GitLabApiException), exception.GetType());
        Assert.Equal(statusCode, exception.StatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout, true)]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.Unauthorized, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public async Task IsTransient_MarksOnlyTheRetryableStatusCodes(HttpStatusCode statusCode, bool expected)
    {
        GitLabApiException exception = await CaptureAsync(statusCode, """{ "message": "boom" }""");

        Assert.Equal(expected, exception.IsTransient);
    }

    [Fact]
    public async Task DerivedException_IsStillCaughtByBaseCatchBlock()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{ "message": "404 Project Not Found" }""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        bool caught = false;

        try
        {
            await connection.GetAsync(ProjectRoute, GitLabJsonContext.Default.GitLabProject,
                TestContext.Current.CancellationToken);
        }
        catch (GitLabApiException exception)
        {
            caught = true;
            Assert.IsType<GitLabNotFoundException>(exception);
            Assert.Equal("404 Project Not Found", exception.Message);
        }

        Assert.True(caught);
    }

    [Fact]
    public async Task StructuredValidationBody_ExposesFieldErrors_AndReadableMessage()
    {
        const string Json = """
                            {
                              "message": {
                                "title": ["can't be blank"],
                                "labels": ["is invalid", "is too long"]
                              }
                            }
                            """;

        GitLabApiException exception = await CaptureAsync(HttpStatusCode.UnprocessableEntity, Json);

        GitLabValidationException validation = Assert.IsType<GitLabValidationException>(exception);
        Assert.Equal(2, validation.Errors.Count);
        Assert.Equal("can't be blank", Assert.Single(validation.Errors["title"]));
        Assert.Equal(ExpectedLabelErrors, validation.Errors["labels"]);
        Assert.Contains("title: can't be blank", validation.Message, StringComparison.Ordinal);
        Assert.Contains("labels: is invalid, is too long", validation.Message, StringComparison.Ordinal);
        Assert.Equal(Json, validation.ResponseBody);
    }

    [Fact]
    public async Task PlainStringValidationBody_LeavesErrorsEmpty_AndMessageUnchanged()
    {
        GitLabApiException exception =
            await CaptureAsync(HttpStatusCode.BadRequest, """{ "message": "400 Bad request" }""");

        GitLabValidationException validation = Assert.IsType<GitLabValidationException>(exception);
        Assert.Empty(validation.Errors);
        Assert.Equal("400 Bad request", validation.Message);
    }

    [Fact]
    public async Task RateLimited_CarriesRetryAfterAndRateLimitSnapshot()
    {
        DateTimeOffset resetsAt = DateTimeOffset.FromUnixTimeSeconds(1_760_000_000);

        GitLabApiException exception = await CaptureAsync(
            HttpStatusCode.TooManyRequests,
            """{ "message": "429 Too Many Requests" }""",
            response =>
            {
                response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
                response.Headers.TryAddWithoutValidation("RateLimit-Limit", "600");
                response.Headers.TryAddWithoutValidation("RateLimit-Remaining", "0");
                response.Headers.TryAddWithoutValidation("RateLimit-Reset",
                    resetsAt.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));
            });

        GitLabRateLimitExceededException rateLimited = Assert.IsType<GitLabRateLimitExceededException>(exception);
        Assert.Equal(TimeSpan.FromSeconds(30), rateLimited.RetryAfter);
        Assert.Equal(600, rateLimited.RateLimit.Limit);
        Assert.Equal(0, rateLimited.RateLimit.Remaining);
        Assert.Equal(resetsAt, rateLimited.RateLimit.ResetsAt);
        Assert.True(rateLimited.IsTransient);
    }

    [Fact]
    public async Task RateLimited_ReadsRetryAfterFromItsHttpDateForm()
    {
        GitLabApiException exception = await CaptureAsync(
            HttpStatusCode.TooManyRequests,
            """{ "message": "429 Too Many Requests" }""",
            response => response.Headers.RetryAfter =
                new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddMinutes(2)));

        GitLabRateLimitExceededException rateLimited = Assert.IsType<GitLabRateLimitExceededException>(exception);

        // Both forms of Retry-After are understood: the exception exposes the delay directly, and
        // GitLabRateLimitSnapshot resolves the same HTTP-date form rather than reporting no delay at all.
        Assert.NotNull(rateLimited.RetryAfter);
        Assert.InRange(rateLimited.RetryAfter.Value, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2));
        Assert.NotNull(rateLimited.RateLimit.RetryAfter);
        Assert.InRange(rateLimited.RateLimit.RetryAfter.Value, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task Exception_CarriesRequestMethodAndAbsoluteRequestUri()
    {
        GitLabApiException exception =
            await CaptureAsync(HttpStatusCode.NotFound, """{ "message": "404 Project Not Found" }""");

        Assert.Equal(HttpMethod.Get, exception.RequestMethod);
        Assert.Equal("https://gitlab.example/api/v4/projects/42", exception.RequestUri?.AbsoluteUri);
        Assert.Contains("GET https://gitlab.example/api/v4/projects/42 -> 404", exception.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task EmptyErrorBody_SynthesizesAMessageInsteadOfLeavingItBlank()
    {
        GitLabApiException exception = await CaptureAsync(HttpStatusCode.Unauthorized, string.Empty);

        Assert.IsType<GitLabAuthenticationException>(exception);
        Assert.Equal("GitLab returned 401 Unauthorized with no response body.", exception.Message);
        Assert.False(exception.IsTransient);
    }

    [Fact]
    public async Task NonJsonErrorBody_KeepsRawBodyAsMessageAndResponseBody()
    {
        const string Html = "<html><body>502 Bad Gateway</body></html>";

        GitLabApiException exception = await CaptureAsync(HttpStatusCode.BadGateway, Html);

        Assert.IsType<GitLabServerException>(exception);
        Assert.Equal(Html, exception.Message);
        Assert.Equal(Html, exception.ResponseBody);
    }

    [Fact]
    public async Task PostAsync_MapsTheStatusCodeAndReportsThePostVerb()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent("""{ "message": "Branch already exists" }""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabConflictException exception = await Assert.ThrowsAsync<GitLabConflictException>(() =>
            connection.PostAsync(ProjectRoute, GitLabJsonContext.Default.GitLabProject,
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpMethod.Post, exception.RequestMethod);
        Assert.Equal("Branch already exists", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_MapsTheStatusCodeAndReportsTheDeleteVerb()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{ "message": "403 Forbidden" }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(() =>
            connection.DeleteAsync(ProjectRoute, TestContext.Current.CancellationToken));

        Assert.Equal(HttpMethod.Delete, exception.RequestMethod);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public async Task PagedRequest_MapsTheStatusCodeOfTheFailingPage()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("""{ "message": "401 Unauthorized" }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabAuthenticationException exception = await Assert.ThrowsAsync<GitLabAuthenticationException>(async () =>
        {
            await foreach (GitLabProject _ in connection.GetPagedAsync(
                                   ProjectRoute,
                                   GitLabJsonContext.Default.GitLabProjectArray,
                                   TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                // The first page already fails; the body exists only to drive the enumerator.
            }
        });

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal(HttpMethod.Get, exception.RequestMethod);
    }

    [Fact]
    public async Task SuccessfulResponseWithNoBody_StillThrowsTheBaseTypeWithRequestContext()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabApiException>(() =>
            connection.GetAsync(ProjectRoute, GitLabJsonContext.Default.GitLabProject,
                TestContext.Current.CancellationToken));

        Assert.Equal("GitLab returned an empty response body.", exception.Message);
        Assert.Equal(HttpMethod.Get, exception.RequestMethod);
        Assert.Equal("https://gitlab.example/api/v4/projects/42", exception.RequestUri?.AbsoluteUri);
    }

    private static async Task<GitLabApiException> CaptureAsync(
        HttpStatusCode statusCode,
        string body,
        Action<HttpResponseMessage>? configureResponse = null)
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            configureResponse?.Invoke(response);
            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        GitLabApiConnection connection = new(httpClient);

        return await Assert.ThrowsAnyAsync<GitLabApiException>(() =>
                connection.GetAsync(ProjectRoute, GitLabJsonContext.Default.GitLabProject,
                    TestContext.Current.CancellationToken))
            .ConfigureAwait(false);
    }
}