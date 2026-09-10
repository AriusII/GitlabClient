using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class RepositoryFilesEndpointTests
{
    [Fact]
    public async Task GetAsync_EscapesNestedFilePath_BuildsRefQuery_AndDeserializesFile()
    {
        const string Json = """
                            {
                              "file_name": "README.md",
                              "file_path": "docs/README.md",
                              "size": 1476,
                              "encoding": "base64",
                              "content": "IyBSZWFkbWU=",
                              "content_sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                              "ref": "main",
                              "blob_id": "79f10e5f6c1e4e9b1a2f5b1c3d4e5f6a7b8c9d0e",
                              "commit_id": "d5a3b1c2e4f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0",
                              "last_commit_id": "3b6c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c",
                              "execute_filemode": true
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        GitLabRepositoryFile file =
            await repository.GetAsync(42, "docs/README.md", "main", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/files/docs%2FREADME.md?ref=main",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("README.md", file.FileName);
        Assert.Equal("docs/README.md", file.FilePath);
        Assert.Equal(1476, file.Size);
        Assert.Equal("base64", file.Encoding);
        Assert.Equal("IyBSZWFkbWU=", file.Content);
        Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", file.ContentSha256);
        Assert.Equal("main", file.Ref);
        Assert.Equal("79f10e5f6c1e4e9b1a2f5b1c3d4e5f6a7b8c9d0e", file.BlobId);
        Assert.Equal("d5a3b1c2e4f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0", file.CommitId);
        Assert.Equal("3b6c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c", file.LastCommitId);
        Assert.True(file.ExecuteFilemode);
    }

    [Fact]
    public async Task GetAsync_EncodesNamespacedProjectPath()
    {
        const string Json = """
                            {
                              "file_name": "README.md",
                              "file_path": "README.md"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        await repository.GetAsync("gitlab-org/gitlab", "README.md", "main", TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/files/README.md?ref=main",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "404 File Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetAsync(42, "missing.md", "main", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("404 File Not Found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_PostsMultipartTextFields_AndDeserializesCreatedFile()
    {
        const string Json = """
                            {
                              "file_name": "new-file.txt",
                              "file_path": "new-file.txt",
                              "branch": "main"
                            }
                            """;

        string? sentBody = null;
        string? sentMediaType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentMediaType = request.Content?.Headers.ContentType?.MediaType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        CreateRepositoryFileRequest request = new()
        {
            Branch = "main", Content = "aGVsbG8=", CommitMessage = "Add new file", Encoding = "base64"
        };

        GitLabRepositoryFile file =
            await repository.CreateAsync(42, "new-file.txt", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/files/new-file.txt",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentMediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=branch", sentBody, StringComparison.Ordinal);
        Assert.Contains("main", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=content", sentBody, StringComparison.Ordinal);
        Assert.Contains("aGVsbG8=", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=commit_message", sentBody, StringComparison.Ordinal);
        Assert.Contains("Add new file", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=encoding", sentBody, StringComparison.Ordinal);
        Assert.Contains("base64", sentBody, StringComparison.Ordinal);
        Assert.Equal("new-file.txt", file.FileName);
        Assert.Equal("new-file.txt", file.FilePath);
    }

    [Fact]
    public async Task CreateAsync_WithFile_PostsTheRequiredFilePart()
    {
        const string Json = """
                            {
                              "file_name": "binary-file.bin",
                              "file_path": "binary-file.bin"
                            }
                            """;

        string? sentBody = null;
        string? sentMediaType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentMediaType = request.Content?.Headers.ContentType?.MediaType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);
        using MemoryStream content = new(Encoding.UTF8.GetBytes("binary repository file"));
        GitLabFileUpload file = new()
        {
            Content = content,
            FileName = "binary-file.bin",
            ContentType = "application/octet-stream",
            FieldName = "caller-supplied-name"
        };

        GitLabRepositoryFile created = await repository.CreateAsync(
            42, "binary-file.bin", file, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("multipart/form-data", sentMediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("name=caller-supplied-name", sentBody, StringComparison.Ordinal);
        Assert.Contains("binary-file.bin", sentBody, StringComparison.Ordinal);
        Assert.Contains("binary repository file", sentBody, StringComparison.Ordinal);
        Assert.True(content.CanRead);
        Assert.Equal("binary-file.bin", created.FileName);
    }

    [Fact]
    public async Task UpdateAsync_PutsMultipartTextFields_AndDeserializesUpdatedFile()
    {
        const string Json = """
                            {
                              "file_name": "existing.txt",
                              "file_path": "docs/existing.txt",
                              "branch": "main"
                            }
                            """;

        string? sentBody = null;
        string? sentMediaType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentMediaType = request.Content?.Headers.ContentType?.MediaType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        UpdateRepositoryFileRequest request = new()
        {
            Branch = "main", Content = "dXBkYXRlZA==", CommitMessage = "Update file"
        };

        GitLabRepositoryFile file =
            await repository.UpdateAsync(42, "docs/existing.txt", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/files/docs%2Fexisting.txt",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", sentMediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=branch", sentBody, StringComparison.Ordinal);
        Assert.Contains("main", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=content", sentBody, StringComparison.Ordinal);
        Assert.Contains("dXBkYXRlZA==", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=commit_message", sentBody, StringComparison.Ordinal);
        Assert.Contains("Update file", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("name=encoding", sentBody, StringComparison.Ordinal);
        Assert.Equal("existing.txt", file.FileName);
        Assert.Equal("docs/existing.txt", file.FilePath);
    }

    [Fact]
    public async Task UpdateAsync_WithFile_PutsTheRequiredFilePart()
    {
        const string Json = """
                            {
                              "file_name": "existing.bin",
                              "file_path": "docs/existing.bin"
                            }
                            """;

        string? sentBody = null;
        string? sentMediaType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentMediaType = request.Content?.Headers.ContentType?.MediaType;
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);
        using MemoryStream content = new(Encoding.UTF8.GetBytes("replacement binary file"));
        GitLabFileUpload file = new()
        {
            Content = content, FileName = "existing.bin", FieldName = "caller-supplied-name"
        };

        GitLabRepositoryFile updated = await repository.UpdateAsync(
            42, "docs/existing.bin", file, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("multipart/form-data", sentMediaType);
        Assert.NotNull(sentBody);
        Assert.Contains("name=file", sentBody, StringComparison.Ordinal);
        Assert.DoesNotContain("name=caller-supplied-name", sentBody, StringComparison.Ordinal);
        Assert.Contains("existing.bin", sentBody, StringComparison.Ordinal);
        Assert.Contains("replacement binary file", sentBody, StringComparison.Ordinal);
        Assert.True(content.CanRead);
        Assert.Equal("docs/existing.bin", updated.FilePath);
    }

    [Fact]
    public async Task CreateAsync_WithAdvancedCommitOptions_PostsEveryDeclaredFormField()
    {
        const string Json = """
                            {
                              "file_name": "tools/run.sh",
                              "file_path": "tools/run.sh"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        await repository.CreateAsync(42, "tools/run.sh",
            new CreateRepositoryFileRequest
            {
                Branch = "feature/new-branch",
                Content = "echo hello",
                CommitMessage = "Create executable",
                AuthorEmail = "octo@example.com",
                AuthorName = "Octo Cat",
                ExecuteFilemode = true,
                StartBranch = "main"
            }, TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        Assert.Contains("name=author_email", sentBody, StringComparison.Ordinal);
        Assert.Contains("octo@example.com", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=author_name", sentBody, StringComparison.Ordinal);
        Assert.Contains("Octo Cat", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=execute_filemode", sentBody, StringComparison.Ordinal);
        Assert.Contains("true", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=start_branch", sentBody, StringComparison.Ordinal);
        Assert.Contains("main", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAsync_WithConcurrencyOptions_PutsEveryDeclaredFormField()
    {
        const string Json = """
                            {
                              "file_name": "tools/run.sh",
                              "file_path": "tools/run.sh"
                            }
                            """;

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        await repository.UpdateAsync(42, "tools/run.sh",
            new UpdateRepositoryFileRequest
            {
                Branch = "main",
                Content = "echo updated",
                CommitMessage = "Update executable",
                AuthorEmail = "octo@example.com",
                AuthorName = "Octo Cat",
                ExecuteFilemode = false,
                LastCommitId = "e83c5163316f89bfbde7d9ab23ca2e25604af290",
                StartBranch = "release/1.0"
            }, TestContext.Current.CancellationToken);

        Assert.NotNull(sentBody);
        Assert.Contains("name=author_email", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=author_name", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=execute_filemode", sentBody, StringComparison.Ordinal);
        Assert.Contains("false", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=last_commit_id", sentBody, StringComparison.Ordinal);
        Assert.Contains("e83c5163316f89bfbde7d9ab23ca2e25604af290", sentBody, StringComparison.Ordinal);
        Assert.Contains("name=start_branch", sentBody, StringComparison.Ordinal);
        Assert.Contains("release/1.0", sentBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteRequest_WithBranchAndCommitMessageAsQueryParams()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        await repository.DeleteAsync(42, "docs/old.md", "main", "Remove old file",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/files/docs%2Fold.md?branch=main&commit_message=Remove%20old%20file",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_WithRequest_EncodesAdvancedCommitControls()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        await repository.DeleteAsync(42, "tools/run.sh",
            new DeleteRepositoryFileRequest
            {
                Branch = "feature/delete-file",
                CommitMessage = "Delete executable",
                StartBranch = "main",
                AuthorEmail = "octo@example.com",
                AuthorName = "Octo Cat",
                LastCommitId = "e83c5163316f89bfbde7d9ab23ca2e25604af290"
            }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/files/tools%2Frun.sh"
            + "?branch=feature%2Fdelete-file&commit_message=Delete%20executable&start_branch=main"
            + "&author_email=octo%40example.com&author_name=Octo%20Cat"
            + "&last_commit_id=e83c5163316f89bfbde7d9ab23ca2e25604af290",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_OnErrorResponse_ThrowsGitLabApiExceptionWithMessage()
    {
        const string Json = """{ "message": "400 Branch Does Not Exist" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabValidationException>(() =>
            repository.DeleteAsync(42, "docs/old.md", "missing-branch", "Remove old file",
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("400 Branch Does Not Exist", exception.Message);
    }

    [Fact]
    public async Task GetMetadataAsync_SendsHead_EscapesFilePath_AndSurfacesGitLabHeaders()
    {
        using StubHttpMessageHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.OK);
            response.Headers.Add("X-Gitlab-Blob-Id", "79f7bbd25901e8334750839545a9bd021f0e4c83");
            response.Headers.Add("X-Gitlab-File-Name", "App.cs");
            response.Headers.Add("X-Gitlab-File-Path", "src/App.cs");
            response.Headers.Add("X-Gitlab-Size", "1476");
            response.Headers.Add("X-Gitlab-Ref", "main");
            return response;
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        GitLabHeadResponse metadata =
            await repository.GetMetadataAsync(42, "src/App.cs", "main", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Head, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/files/src%2FApp.cs?ref=main",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(metadata.Exists);

        // Header lookup is case-insensitive, which is what makes GetHeaderValue usable against a header
        // set GitLab spells inconsistently across versions.
        Assert.Equal("79f7bbd25901e8334750839545a9bd021f0e4c83", metadata.GetHeaderValue("x-gitlab-blob-id"));
        Assert.Equal("1476", metadata.GetHeaderValue("X-Gitlab-Size"));
        Assert.Equal("src/App.cs", metadata.GetHeaderValue("X-Gitlab-File-Path"));
        Assert.Null(metadata.GetHeaderValue("X-Gitlab-Absent"));
    }

    [Fact]
    public async Task GetMetadataAsync_OnNotFound_ReportsDoesNotExist_RatherThanThrowing()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        GitLabHeadResponse metadata =
            await repository.GetMetadataAsync(42, "src/Missing.cs", "main", TestContext.Current.CancellationToken);

        Assert.False(metadata.Exists);
        Assert.Equal(HttpStatusCode.NotFound, metadata.StatusCode);
    }

    [Fact]
    public async Task GetBlameMetadataAsync_SendsHeadToTheBlameRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        GitLabHeadResponse metadata = await repository.GetBlameMetadataAsync(
            "gitlab-org/gitlab", "src/App.cs", "release/1.0", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Head, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/repository/files/src%2FApp.cs/blame"
            + "?ref=release%2F1.0",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.True(metadata.Exists);
    }

    [Fact]
    public async Task GetBlameAsync_SendsTheBracketedRangeParameters_AndDeserializesRanges()
    {
        const string Json = """
                            [
                              {
                                "commit": {
                                  "id": "d42409d56517157c48bf3bd97d3f75974dde19fb",
                                  "message": "Add feature\n\n",
                                  "parent_ids": ["cc6e14f9328fa6d7b5a0d3c30dc2002a3f2a3822"],
                                  "authored_date": "2015-12-31T09:41:21+01:00",
                                  "author_name": "Dmitriy Zaporozhets",
                                  "author_email": "dzaporozhets@example.com",
                                  "committed_date": "2015-12-31T09:41:21+01:00",
                                  "committer_name": "Dmitriy Zaporozhets",
                                  "committer_email": "dzaporozhets@example.com"
                                },
                                "lines": ["require 'fileutils'", "require 'open3'"]
                              }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        List<GitLabBlameRange> ranges = [];
        await foreach (GitLabBlameRange range in repository.GetBlameAsync(
                           42, "src/App.cs", "main", 1, 2, TestContext.Current.CancellationToken))
        {
            ranges.Add(range);
        }

        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/files/src%2FApp.cs/blame"
            + "?ref=main&range[start]=1&range[end]=2",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabBlameRange single = Assert.Single(ranges);
        Assert.Equal("d42409d56517157c48bf3bd97d3f75974dde19fb", single.Commit?.Id);
        Assert.Equal("Dmitriy Zaporozhets", single.Commit?.AuthorName);
        Assert.Equal(2, single.Lines?.Count);
    }

    [Fact]
    public void GetBlameAsync_WithOnlyOneRangeBound_RejectsMalformedGitLabRange()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            repository.GetBlameAsync(42, "src/App.cs", "main", 1,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("rangeEnd", exception.ParamName);
        Assert.Null(handler.LastRequest);
    }

    [Theory]
    [InlineData(0, 1, "rangeStart")]
    [InlineData(1, 0, "rangeEnd")]
    public void GetBlameAsync_WithNonPositiveRangeBound_RejectsMalformedGitLabRange(int rangeStart, int rangeEnd,
        string parameterName)
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            repository.GetBlameAsync(42, "src/App.cs", "main", rangeStart, rangeEnd,
                TestContext.Current.CancellationToken));

        Assert.Equal(parameterName, exception.ParamName);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task GetRawAsync_StreamsTheBody_WithRefAndLfsOnTheQuery()
    {
        const string Contents = "public sealed class App;\n";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Contents, Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        using GitLabFileResponse file = await repository.GetRawAsync(
            42, "src/App.cs", "release/1.0", true, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/projects/42/repository/files/src%2FApp.cs/raw"
            + "?ref=release%2F1.0&lfs=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("text/plain", file.ContentType);

        using StreamReader reader = new(file.Content, Encoding.UTF8);
        Assert.Equal(Contents, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetRawAsync_WithoutOptionalArguments_SendsNoQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("x", Encoding.UTF8, "text/plain")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        using GitLabFileResponse file =
            await repository.GetRawAsync(42, "README.md", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/projects/42/repository/files/README.md/raw",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.NotNull(file.Content);
    }

    [Fact]
    public async Task GetRawAsync_OnErrorResponse_ThrowsGitLabNotFoundException()
    {
        const string Json = """{ "message": "404 File Not Found" }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        RepositoryFilesClient repository = new(connection);

        GitLabApiException exception = await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
            repository.GetRawAsync(42, "src/Missing.cs", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("404 File Not Found", exception.Message);
    }
}