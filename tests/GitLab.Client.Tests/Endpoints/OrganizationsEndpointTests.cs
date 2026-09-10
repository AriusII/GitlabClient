using System.Net;
using System.Net.Http.Headers;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class OrganizationsEndpointTests
{
    private const string OrganizationJson = """
                                            {
                                              "id": 1,
                                              "uuid": "0192f8c2-1a2b-7cde-89ab-0123456789ab",
                                              "name": "GitLab",
                                              "path": "gitlab",
                                              "description": "My description",
                                              "visibility": "public",
                                              "created_at": "2022-02-24T20:22:30.097Z",
                                              "updated_at": "2022-02-24T20:22:30.097Z",
                                              "web_url": "https://example.com/o/gitlab/-/overview",
                                              "avatar_url": "https://example.com/uploads/-/system/organizations/organization_detail/avatar/1/avatar.png"
                                            }
                                            """;

    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    [Fact]
    public async Task CreateAsync_WithoutAnAvatar_PostsMultipartFormFields()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(OrganizationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        OrganizationsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabOrganization organization = await repository.CreateAsync(
            new CreateOrganizationRequest
            {
                Name = "GitLab", Path = "gitlab", Visibility = GitLabOrganizationVisibility.Public
            },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/organizations", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        string body = sentBody.Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=name", body, StringComparison.Ordinal);
        Assert.Contains("GitLab", body, StringComparison.Ordinal);
        Assert.Contains("name=path", body, StringComparison.Ordinal);
        Assert.Contains("name=visibility", body, StringComparison.Ordinal);
        Assert.Contains("public", body, StringComparison.Ordinal);
        Assert.Equal(1, organization.Id);
        Assert.Equal("public", organization.Visibility);
    }

    [Fact]
    public async Task CreateAsync_WithAnAvatar_PostsMultipartFormData_WithTheFieldsAlongsideTheFile()
    {
        string? sentBody = null;
        MediaTypeHeaderValue? sentContentType = null;

        using StubHttpMessageHandler handler = new(request =>
        {
            sentContentType = request.Content?.Headers.ContentType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(OrganizationJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        OrganizationsClient repository = new(new GitLabApiConnection(httpClient));

        using MemoryStream avatarBytes = new([0x89, 0x50, 0x4E, 0x47]);
        GitLabFileUpload avatar = new() { Content = avatarBytes, FileName = "logo.png", ContentType = "image/png" };

        GitLabOrganization organization = await repository.CreateAsync(
            new CreateOrganizationRequest
            {
                Name = "GitLab",
                Path = "gitlab",
                Description = "My description",
                Visibility = GitLabOrganizationVisibility.Private
            },
            avatar,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/organizations", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentContentType?.MediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=name", sentBody, StringComparison.Ordinal);
        Assert.Contains("GitLab", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=path", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=description", sentBody, StringComparison.Ordinal);
        Assert.Contains("My description", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=visibility", sentBody, StringComparison.Ordinal);
        Assert.Contains("private", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=avatar", sentBody, StringComparison.Ordinal);
        Assert.Contains("logo.png", sentBody, StringComparison.Ordinal);

        // The stream is borrowed, never owned: it must still be usable after the call returns.
        Assert.True(avatarBytes.CanRead);
        Assert.Equal(1, organization.Id);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteToTheOrganizationRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        OrganizationsClient repository = new(new GitLabApiConnection(httpClient));

        await repository.DeleteAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/organizations/1", handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task CreateAsync_OnBadRequest_ThrowsGitLabValidationException()
    {
        const string Json = """{ "message": { "path": ["has already been taken"] } }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = BaseAddress };
        OrganizationsClient repository = new(new GitLabApiConnection(httpClient));

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.CreateAsync(new CreateOrganizationRequest { Name = "GitLab", Path = "gitlab" },
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}