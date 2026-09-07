using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class FreezePeriodsRepositoryTests
{
    private const string FreezePeriodJson = """
                                            {
                                              "id": 1,
                                              "freeze_start": "0 23 * * 5",
                                              "freeze_end": "0 7 * * 1",
                                              "cron_timezone": "Europe/Berlin",
                                              "created_at": "2020-05-15T17:03:35.702Z",
                                              "updated_at": "2020-05-15T17:06:41.566Z"
                                            }
                                            """;

    [Fact]
    public async Task ListAsync_BuildsFreezePeriodsRoute_AndDeserializesTheCronBoundaries()
    {
        string json = $"[{FreezePeriodJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FreezePeriodsRepository repository = new(connection);

        List<GitLabFreezePeriod> periods = new();
        await foreach (GitLabFreezePeriod item in repository.ListAsync(19, TestContext.Current.CancellationToken))
        {
            periods.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/19/freeze_periods",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabFreezePeriod period = Assert.Single(periods);
        Assert.Equal(1, period.Id);

        // The two boundaries are cron expressions, not timestamps - the '*' must survive verbatim.
        Assert.Equal("0 23 * * 5", period.FreezeStart);
        Assert.Equal("0 7 * * 1", period.FreezeEnd);
        Assert.Equal("Europe/Berlin", period.CronTimezone);
        Assert.Equal(new DateTimeOffset(2020, 5, 15, 17, 3, 35, 702, TimeSpan.Zero), period.CreatedAt);
    }

    [Fact]
    public async Task ListAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FreezePeriodsRepository repository = new(connection);

        await foreach (GitLabFreezePeriod _ in
                       repository.ListAsync("gitlab-org/gitlab", TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/freeze_periods",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_AppendsTheNumericFreezePeriodId()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FreezePeriodJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FreezePeriodsRepository repository = new(connection);

        GitLabFreezePeriod period = await repository.GetAsync(19, 1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/19/freeze_periods/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("0 23 * * 5", period.FreezeStart);
    }

    [Fact]
    public async Task CreateAsync_PostsTheCronBodyAndDefaultsTheTimezoneAway()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(FreezePeriodJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FreezePeriodsRepository repository = new(connection);

        CreateFreezePeriodRequest request = new() { FreezeStart = "0 23 * * 5", FreezeEnd = "0 7 * * 1" };

        GitLabFreezePeriod period = await repository.CreateAsync(19, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/19/freeze_periods",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);

        // A null cron_timezone is omitted rather than sent as null, so GitLab applies its own UTC default.
        Assert.Equal("""{"freeze_start":"0 23 * * 5","freeze_end":"0 7 * * 1"}""", sentBody);
        Assert.Equal(1, period.Id);
    }

    [Fact]
    public async Task UpdateAsync_PutsOnlyTheChangedBoundary()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(FreezePeriodJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FreezePeriodsRepository repository = new(connection);

        UpdateFreezePeriodRequest request = new() { FreezeEnd = "0 8 * * 1" };

        await repository.UpdateAsync(19, 1, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/19/freeze_periods/1",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"freeze_end":"0 8 * * 1"}""", sentBody);
    }

    /// <summary>GitLab answers this delete with 200 and the deleted period, not the usual 204.</summary>
    [Fact]
    public async Task DeleteAsync_SendsDelete_AndDiscardsTheReturnedBody()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(FreezePeriodJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FreezePeriodsRepository repository = new(connection);

        await repository.DeleteAsync("gitlab-org/gitlab", 7, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/freeze_periods/7",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnMissingFreezePeriod_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 Freeze Period Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FreezePeriodsRepository repository = new(connection);

        GitLabNotFoundException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(19, 404, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 Freeze Period Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_OnInvalidCronExpression_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "freeze_start": ["is invalid syntax"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        FreezePeriodsRepository repository = new(connection);

        CreateFreezePeriodRequest request = new() { FreezeStart = "not cron", FreezeEnd = "0 7 * * 1" };

        GitLabValidationException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(19, request, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("is invalid syntax", exception.Message, StringComparison.Ordinal);
    }
}