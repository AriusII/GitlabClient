using System.Net;
using System.Text;

using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class MergeTrainsEndpointTests
{
    /// <summary>
    ///     A recorded car. The embedded "merge_request" is GitLab's MergeRequestSimple shape - note that it
    ///     carries no source_branch, which is exactly why this maps to GitLabMergeTrainMergeRequest rather
    ///     than to GitLabMergeRequest.
    /// </summary>
    private const string MergeTrainCarJson = """
                                             {
                                               "id": 267,
                                               "merge_request": {
                                                 "id": 273,
                                                 "iid": 3,
                                                 "project_id": 597,
                                                 "title": "My title 3",
                                                 "description": null,
                                                 "state": "merged",
                                                 "created_at": "2022-10-31T19:06:06.658Z",
                                                 "updated_at": "2022-10-31T19:06:07.475Z",
                                                 "web_url": "https://gitlab.example/root/merge-train/-/merge_requests/3"
                                               },
                                               "user": {
                                                 "id": 1,
                                                 "username": "root",
                                                 "name": "Administrator",
                                                 "state": "active",
                                                 "avatar_url": "https://gitlab.example/uploads/avatar.png",
                                                 "web_url": "https://gitlab.example/root"
                                               },
                                               "pipeline": {
                                                 "id": 597,
                                                 "iid": 2,
                                                 "project_id": 597,
                                                 "sha": "0ad8b8f1e7b3d0f8b3d3f4e2c1a0b9d8e7f6a5b4",
                                                 "ref": "refs/merge-requests/3/train",
                                                 "status": "success",
                                                 "source": "merge_request_event",
                                                 "created_at": "2022-10-31T19:06:06.000Z",
                                                 "updated_at": "2022-10-31T19:06:07.000Z",
                                                 "web_url": "https://gitlab.example/root/merge-train/-/pipelines/597"
                                               },
                                               "created_at": "2022-10-31T19:06:06.678Z",
                                               "updated_at": "2022-10-31T19:06:07.485Z",
                                               "target_branch": "main",
                                               "status": "merged",
                                               "merged_at": "2022-10-31T19:06:07.484Z",
                                               "duration": 6
                                             }
                                             """;

    [Fact]
    public async Task ListAsync_BuildsMergeTrainsRoute_AndDeserializesTheCar()
    {
        string json = $"[{MergeTrainCarJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeTrainsClient repository = new(connection);

        List<GitLabMergeTrainCar> cars = new();
        await foreach (GitLabMergeTrainCar item in repository.ListAsync(597,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            cars.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/597/merge_trains",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabMergeTrainCar car = Assert.Single(cars);
        Assert.Equal(267, car.Id);
        Assert.Equal("main", car.TargetBranch);
        Assert.Equal("merged", car.Status);
        Assert.Equal(6, car.Duration);
        Assert.Equal(new DateTimeOffset(2022, 10, 31, 19, 6, 7, 484, TimeSpan.Zero), car.MergedAt);

        Assert.Equal(3, car.MergeRequest?.Iid);
        Assert.Equal("My title 3", car.MergeRequest?.Title);
        Assert.Null(car.MergeRequest?.Description);
        Assert.Equal("root", car.User?.Username);
        Assert.Equal("success", car.Pipeline?.Status);
        Assert.Equal("refs/merge-requests/3/train", car.Pipeline?.Ref);
    }

    [Fact]
    public async Task ListAsync_WithOptions_WritesScopeSortAndPerPage()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeTrainsClient repository = new(connection);

        MergeTrainListOptions options = new() { Scope = "active", Sort = "desc", PerPage = 50 };

        await foreach (GitLabMergeTrainCar _ in repository.ListAsync("gitlab-org/gitlab", options,
                           TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/merge_trains?scope=active&sort=desc&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>One train exists per target branch, and branch names legally contain '/'.</summary>
    [Fact]
    public async Task ListForTargetBranchAsync_EscapesTheBranchName_AndKeepsTheQuery()
    {
        string json = $"[{MergeTrainCarJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeTrainsClient repository = new(connection);

        MergeTrainListOptions options = new() { Scope = "complete" };

        List<GitLabMergeTrainCar> cars = new();
        await foreach (GitLabMergeTrainCar item in repository.ListForTargetBranchAsync(597, "release/2.0", options,
                           TestContext.Current.CancellationToken))
        {
            cars.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/projects/597/merge_trains/release%2F2.0?scope=complete",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(267, Assert.Single(cars).Id);
    }

    [Fact]
    public async Task GetStatusAsync_AddressesTheMergeRequestByIid()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(MergeTrainCarJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeTrainsClient repository = new(connection);

        GitLabMergeTrainCar car = await repository.GetStatusAsync(597, 3, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/597/merge_trains/merge_requests/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("merged", car.Status);
    }

    [Fact]
    public async Task AddMergeRequestAsync_PostsTheShaGuardAndAutoMergeFlag()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MergeTrainCarJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeTrainsClient repository = new(connection);

        AddToMergeTrainRequest request = new()
        {
            Sha = "0ad8b8f1e7b3d0f8b3d3f4e2c1a0b9d8e7f6a5b4", Squash = true, AutoMerge = true
        };

        GitLabMergeTrainCar car =
            await repository.AddMergeRequestAsync(597, 3, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/597/merge_trains/merge_requests/3",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("application/json", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        Assert.Equal(
            """{"sha":"0ad8b8f1e7b3d0f8b3d3f4e2c1a0b9d8e7f6a5b4","squash":true,"auto_merge":true}""",
            sentBody);
        Assert.Equal(267, car.Id);
    }

    [Fact]
    public async Task AddMergeRequestAsync_WithAnEmptyRequest_SendsAnEmptyJsonObject()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(MergeTrainCarJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeTrainsClient repository = new(connection);

        await repository.AddMergeRequestAsync(597, 3, new AddToMergeTrainRequest(),
            TestContext.Current.CancellationToken);

        Assert.Equal("{}", sentBody);
    }

    /// <summary>Merge trains are a Premium feature; an instance without them answers 403, not an empty list.</summary>
    [Fact]
    public async Task ListAsync_WhenMergeTrainsAreUnavailable_ThrowsGitLabForbiddenException()
    {
        const string Json = """{ "message": "403 Forbidden" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        MergeTrainsClient repository = new(connection);

        GitLabForbiddenException exception = await Assert.ThrowsAsync<GitLabForbiddenException>(async () =>
        {
            await foreach (GitLabMergeTrainCar _ in repository
                               .ListAsync(597, cancellationToken: TestContext.Current.CancellationToken)
                               .ConfigureAwait(false))
            {
                Assert.Fail("The stubbed response is an error.");
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}