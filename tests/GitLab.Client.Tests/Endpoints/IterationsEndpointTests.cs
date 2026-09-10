using System.Globalization;
using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class IterationsEndpointTests
{
    private const string IterationsJson = """
                                          [
                                            {
                                              "id": 53,
                                              "iid": 13,
                                              "sequence": 2,
                                              "group_id": 5,
                                              "title": "Iteration 13",
                                              "description": "Second sprint of the quarter",
                                              "state": 2,
                                              "created_at": "2024-03-01T09:15:00.000Z",
                                              "updated_at": "2024-03-04T11:00:00.000Z",
                                              "start_date": "2024-03-04",
                                              "due_date": "2024-03-17",
                                              "web_url": "https://gitlab.example/groups/gitlab-org/-/iterations/53"
                                            }
                                          ]
                                          """;

    [Fact]
    public async Task ListForGroupAsync_BuildsGroupIterationsRoute_WithQueryOptions_AndDeserializesIterations()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(IterationsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        IterationsClient repository = new(connection);

        IterationListOptions options = new()
        {
            State = GitLabIterationStateFilter.Current,
            Search = "Iteration 13",
            SearchIn = ["title", "cadence_title"],
            IncludeAncestors = true,
            IncludeDescendants = false,
            UpdatedAfter = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedBefore = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero),
            PerPage = 25
        };

        List<GitLabIteration> iterations = [];
        await foreach (GitLabIteration item in
                       repository.ListForGroupAsync(5, options, TestContext.Current.CancellationToken))
        {
            iterations.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/groups/5/iterations", requestUri, StringComparison.Ordinal);
        Assert.Contains("state=current", requestUri, StringComparison.Ordinal);
        Assert.Contains("search=Iteration%2013", requestUri, StringComparison.Ordinal);

        // The spec names this parameter "in", which no snake_case rule produces from a C# property
        // name; it is pinned by [QueryParameter("in")] on IterationListOptions.SearchIn.
        Assert.Contains("in=title,cadence_title", requestUri, StringComparison.Ordinal);
        Assert.Contains("include_ancestors=true", requestUri, StringComparison.Ordinal);
        Assert.Contains("include_descendants=false", requestUri, StringComparison.Ordinal);
        Assert.Contains("updated_before=2024-12-31T23:59:59Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("updated_after=2024-01-01T00:00:00Z", requestUri, StringComparison.Ordinal);
        Assert.Contains("per_page=25", requestUri, StringComparison.Ordinal);

        GitLabIteration iteration = Assert.Single(iterations);
        Assert.Equal(53, iteration.Id);
        Assert.Equal(13, iteration.Iid);
        Assert.Equal(2, iteration.Sequence);
        Assert.Equal(5, iteration.GroupId);
        Assert.Equal("Iteration 13", iteration.Title);
        Assert.Equal("Second sprint of the quarter", iteration.Description);

        // The state on the entity is an integer, not the string the query parameter takes.
        Assert.Equal(2, iteration.State);

        Assert.Equal(new DateTimeOffset(2024, 3, 1, 9, 15, 0, TimeSpan.Zero), iteration.CreatedAt);
        Assert.Equal(new DateTimeOffset(2024, 3, 4, 11, 0, 0, TimeSpan.Zero), iteration.UpdatedAt);
        Assert.Equal(new DateOnly(2024, 3, 4), iteration.StartDate);
        Assert.Equal(new DateOnly(2024, 3, 17), iteration.DueDate);
        Assert.Equal(new Uri("https://gitlab.example/groups/gitlab-org/-/iterations/53"), iteration.WebUrl);
    }

    [Fact]
    public async Task ListForGroupAsync_EncodesNamespacedGroupPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        IterationsClient repository = new(connection);

        await foreach (GitLabIteration _ in repository.ListForGroupAsync(
                           "gitlab-org/subgroup",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/groups/gitlab-org%2Fsubgroup/iterations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForProjectAsync_BuildsProjectIterationsRoute_AndDeserializesIterations()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(IterationsJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        IterationsClient repository = new(connection);

        List<GitLabIteration> iterations = [];
        await foreach (GitLabIteration item in repository.ListForProjectAsync(
                           7,
                           new IterationListOptions { State = GitLabIterationStateFilter.Opened },
                           TestContext.Current.CancellationToken))
        {
            iterations.Add(item);
        }

        string? requestUri = handler.LastRequest?.RequestUri?.AbsoluteUri;
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Contains("/projects/7/iterations", requestUri, StringComparison.Ordinal);
        Assert.Contains("state=opened", requestUri, StringComparison.Ordinal);

        GitLabIteration iteration = Assert.Single(iterations);
        Assert.Equal(53, iteration.Id);
        Assert.Equal("Iteration 13", iteration.Title);
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesNamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        IterationsClient repository = new(connection);

        await foreach (GitLabIteration _ in repository.ListForProjectAsync(
                           "gitlab-org/gitlab",
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stub returns an empty page.");
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/iterations",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListForProjectAsync_DeserializesNullTitleAndDescription_ForCadenceScheduledIterations()
    {
        const string Json = """
                            [
                              {
                                "id": 91,
                                "iid": 4,
                                "group_id": 5,
                                "title": null,
                                "description": null,
                                "state": 1,
                                "start_date": "2025-01-06",
                                "due_date": "2025-01-19",
                                "web_url": "https://gitlab.example/groups/gitlab-org/-/iterations/91"
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        IterationsClient repository = new(connection);

        List<GitLabIteration> iterations = [];
        await foreach (GitLabIteration item in repository.ListForProjectAsync(
                           7,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            iterations.Add(item);
        }

        GitLabIteration iteration = Assert.Single(iterations);
        Assert.Equal(91, iteration.Id);
        Assert.Null(iteration.Title);
        Assert.Null(iteration.Description);
        Assert.Null(iteration.Sequence);
        Assert.Null(iteration.CreatedAt);
        Assert.Equal(1, iteration.State);
    }

    [Fact]
    public async Task ListForGroupAsync_OnForbiddenResponse_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = CreateHttpClient(handler);
        GitLabApiConnection connection = new(httpClient);
        IterationsClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabIteration _ in repository
                               .ListForGroupAsync(5, cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                Assert.Fail("The stub never returns a successful page.");
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("403 Forbidden", exception.Message);
    }

    [Fact]
    public void UpdatedFilters_AreNormalisedToUtc_RegardlessOfTheCallerOffset()
    {
        // Guards the DateTimeOffset query overload: the wire value must be ISO-8601 UTC, so a caller
        // in a non-UTC zone cannot silently shift the filter window.
        DateTimeOffset value = new(2024, 6, 15, 8, 30, 0, TimeSpan.FromHours(2));

        Assert.Equal("2024-06-15T06:30:00Z",
            value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture));
    }

    private static HttpClient CreateHttpClient(StubHttpMessageHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
    }
}