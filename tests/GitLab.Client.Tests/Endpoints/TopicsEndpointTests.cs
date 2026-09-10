using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class TopicsEndpointTests
{
    private const string TopicJson = """
                                     {
                                       "id": 1,
                                       "name": "gitlab",
                                       "title": "GitLab",
                                       "description": "Everything GitLab",
                                       "total_projects_count": 1000,
                                       "organization_id": 1,
                                       "avatar_url": "http://gitlab.example.com/uploads/topic/avatar/1/avatar.png"
                                     }
                                     """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task ListAsync_BuildsTopicsRoute_AndDeserializesTheTopic()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{TopicJson}]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        TopicsClient repository = new(new GitLabApiConnection(httpClient));

        List<GitLabTopic> topics = [];
        await foreach (GitLabTopic topic in repository.ListAsync(cancellationToken: TestContext.Current
                           .CancellationToken))
        {
            topics.Add(topic);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/topics", handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabTopic only = Assert.Single(topics);
        Assert.Equal(1, only.Id);
        Assert.Equal("gitlab", only.Name);
        Assert.Equal("GitLab", only.Title);
        Assert.Equal("Everything GitLab", only.Description);
        Assert.Equal(1000, only.TotalProjectsCount);
        Assert.Equal(1, only.OrganizationId);
        Assert.Equal(new Uri("http://gitlab.example.com/uploads/topic/avatar/1/avatar.png"), only.AvatarUrl);
    }

    [Fact]
    public async Task ListAsync_ProjectsEveryFilterOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        TopicsClient repository = new(new GitLabApiConnection(httpClient));

        TopicListOptions options = new()
        {
            Search = "git lab", WithoutProjects = true, OrganizationId = 3, PerPage = 50
        };

        await foreach (GitLabTopic _ in repository.ListAsync(options, TestContext.Current.CancellationToken))
        {
            Assert.Fail("The stubbed response is an empty page.");
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/topics?search=git%20lab&without_projects=true&organization_id=3&per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_BuildsTheTopicRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TopicJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        TopicsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabTopic topic = await repository.GetAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/topics/1", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("gitlab", topic.Name);
    }

    [Fact]
    public async Task CreateAsync_PostsMultipartFormData_OmittingUnsetFields()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(TopicJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        TopicsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabTopic topic = await repository.CreateAsync(
            new CreateTopicRequest { Name = "gitlab", Title = "GitLab", OrganizationId = 1 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/topics", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        AssertFormField(sentBody, "name", "gitlab");
        AssertFormField(sentBody, "title", "GitLab");
        AssertFormField(sentBody, "organization_id", "1");
        Assert.DoesNotContain("name=avatar", NormalizeMultipartBody(sentBody), StringComparison.Ordinal);
        Assert.Equal(1, topic.Id);
    }

    [Fact]
    public async Task UpdateAsync_PutsOnlyTheSetFieldsAsMultipartFormData()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(TopicJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        TopicsClient repository = new(new GitLabApiConnection(httpClient));

        await repository.UpdateAsync(1, new UpdateTopicRequest { Description = "Everything GitLab" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/topics/1", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", handler.LastRequest?.Content?.Headers.ContentType?.MediaType);
        AssertFormField(sentBody, "description", "Everything GitLab");
    }

    /// <summary>
    ///     The upload's field name is forced to "avatar" by the repository, whatever the caller set: GitLab
    ///     answers 200 and silently ignores an avatar part sent under any other name, so a caller that left
    ///     <see cref="GitLabFileUpload.FieldName" /> at its "file" default would see a success and no avatar.
    /// </summary>
    [Fact]
    public async Task SetAvatarAsync_PutsMultipart_UnderTheAvatarFieldNameGitLabExpects()
    {
        string? sentBody = null;
        string? contentType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            contentType = request.Content?.Headers.ContentType?.MediaType;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(TopicJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        TopicsClient repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());

        GitLabTopic topic = await repository.SetAvatarAsync(
            1,
            new GitLabFileUpload { Content = content, FileName = "logo.png", ContentType = "image/png" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/topics/1", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", contentType);

        string body = (sentBody ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=avatar", body, StringComparison.Ordinal);
        Assert.DoesNotContain("name=file;", body, StringComparison.Ordinal);
        Assert.Contains("filename=logo.png", body, StringComparison.Ordinal);
        Assert.Equal(1, topic.Id);
    }

    [Fact]
    public async Task SetAvatarAsync_LeavesTheCallersStreamOpen()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(TopicJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        TopicsClient repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());

        await repository.SetAvatarAsync(
            1,
            new GitLabFileUpload { Content = content, FileName = "logo.png" },
            TestContext.Current.CancellationToken);

        Assert.True(content.CanRead);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheTopicRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        TopicsClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/topics/1", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task MergeAsync_PostsBothTopicIds_ToTheMergeRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(TopicJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        TopicsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabTopic survivor = await repository.MergeAsync(
            new MergeTopicsRequest { SourceTopicId = 2, TargetTopicId = 1 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/topics/merge", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"source_topic_id":2,"target_topic_id":1}""", sentBody);
        Assert.Equal(1, survivor.Id);
    }

    private static void AssertFormField(string? body, string name, string value)
    {
        Assert.Contains($"name={name}\r\n\r\n{value}\r\n", NormalizeMultipartBody(body), StringComparison.Ordinal);
    }

    private static string NormalizeMultipartBody(string? body)
    {
        return (body ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal);
    }
}