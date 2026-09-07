using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class SnippetsRepositoryTests
{
    private const string SnippetJson = """
                                       {
                                         "id": 65,
                                         "title": "Sample snippet",
                                         "description": "Ruby test snippet",
                                         "visibility": "internal",
                                         "author": {
                                           "id": 1,
                                           "username": "john_smith",
                                           "name": "John Smith",
                                           "state": "active",
                                           "avatar_url": "https://gitlab.example/uploads/user/avatar/1/index.jpg",
                                           "web_url": "https://gitlab.example/john_smith"
                                         },
                                         "created_at": "2012-06-28T10:52:04Z",
                                         "updated_at": "2012-06-28T11:53:05Z",
                                         "project_id": null,
                                         "web_url": "https://gitlab.example/-/snippets/65",
                                         "raw_url": "https://gitlab.example/-/snippets/65/raw",
                                         "ssh_url_to_repo": "git@gitlab.example:snippets/65.git",
                                         "http_url_to_repo": "https://gitlab.example/snippets/65.git",
                                         "file_name": "add.rb",
                                         "files": [
                                           {
                                             "path": "src/App.cs",
                                             "raw_url": "https://gitlab.example/-/snippets/65/raw/main/src%2FApp.cs"
                                           },
                                           {
                                             "path": "add.rb",
                                             "raw_url": "https://gitlab.example/-/snippets/65/raw/main/add.rb"
                                           }
                                         ],
                                         "imported": false,
                                         "imported_from": "none",
                                         "repository_storage": "default"
                                       }
                                       """;

    private const string UserAgentDetailJson = """
                                               {
                                                 "user_agent": "AppleWebKit/537.36",
                                                 "ip_address": "127.0.0.1",
                                                 "akismet_submitted": false
                                               }
                                               """;

    private static SnippetsRepository CreateRepository(HttpMessageHandler handler, out HttpClient httpClient)
    {
        httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        return new SnippetsRepository(new GitLabApiConnection(httpClient));
    }

    [Fact]
    public async Task ListAsync_BuildsTheSnippetsRoute_AndDeserializesTheFullPayload()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{SnippetJson}]", Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            List<GitLabSnippet> snippets = [];
            await foreach (GitLabSnippet snippet in repository.ListAsync(
                               cancellationToken: TestContext.Current.CancellationToken))
            {
                snippets.Add(snippet);
            }

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/snippets", handler.LastRequest?.RequestUri?.AbsoluteUri);

            GitLabSnippet only = Assert.Single(snippets);
            Assert.Equal(65, only.Id);
            Assert.Equal("Sample snippet", only.Title);
            Assert.Equal("Ruby test snippet", only.Description);
            Assert.Equal(GitLabVisibility.Internal, only.Visibility);
            Assert.Equal("john_smith", only.Author?.Username);
            Assert.Equal(new DateTimeOffset(2012, 6, 28, 10, 52, 4, TimeSpan.Zero), only.CreatedAt);
            Assert.Null(only.ProjectId);
            Assert.Equal(new Uri("https://gitlab.example/-/snippets/65"), only.WebUrl);
            Assert.Equal(new Uri("https://gitlab.example/-/snippets/65/raw"), only.RawUrl);

            // Kept as a string on purpose: an SCP-style remote is not a parsable absolute URI.
            Assert.Equal("git@gitlab.example:snippets/65.git", only.SshUrlToRepo);
            Assert.Equal(new Uri("https://gitlab.example/snippets/65.git"), only.HttpUrlToRepo);
            Assert.Equal("add.rb", only.FileName);
            Assert.False(only.Imported);
            Assert.Equal("none", only.ImportedFrom);
            Assert.Equal("default", only.RepositoryStorage);

            Assert.NotNull(only.Files);
            Assert.Equal(2, only.Files!.Count);
            Assert.Equal("src/App.cs", only.Files[0].Path);
            Assert.Equal(new Uri("https://gitlab.example/-/snippets/65/raw/main/src%2FApp.cs"),
                only.Files[0].RawUrl);
            Assert.Equal("add.rb", only.Files[1].Path);
        }
    }

    [Fact]
    public async Task ListAsync_ProjectsTheListOptionsOntoTheQueryString()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            SnippetListOptions options = new()
            {
                CreatedAfter = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
                CreatedBefore = new DateTimeOffset(2024, 6, 7, 8, 9, 10, TimeSpan.Zero),
                PerPage = 50
            };

            await foreach (GitLabSnippet _ in repository.ListAsync(options, TestContext.Current.CancellationToken))
            {
                // Draining the sequence is what issues the request.
            }

            Assert.Equal(
                "https://gitlab.example/api/v4/snippets"
                + "?created_after=2024-01-02T03:04:05Z&created_before=2024-06-07T08:09:10Z&per_page=50",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task ListAllAsync_BuildsTheAllRoute_AndFiltersByRepositoryStorage()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            AllSnippetListOptions options = new() { RepositoryStorage = "nfs-01", PerPage = 20 };

            await foreach (GitLabSnippet _ in repository.ListAllAsync(options, TestContext.Current.CancellationToken))
            {
                // Draining the sequence is what issues the request.
            }

            Assert.Equal("https://gitlab.example/api/v4/snippets/all?repository_storage=nfs-01&per_page=20",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task ListPublicAsync_BuildsThePublicRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"[{SnippetJson}]", Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            List<GitLabSnippet> snippets = [];
            await foreach (GitLabSnippet snippet in repository.ListPublicAsync(
                               cancellationToken: TestContext.Current.CancellationToken))
            {
                snippets.Add(snippet);
            }

            Assert.Equal("https://gitlab.example/api/v4/snippets/public",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Single(snippets);
        }
    }

    [Fact]
    public async Task GetAsync_BuildsTheSingleSnippetRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SnippetJson, Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabSnippet snippet = await repository.GetAsync(65, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/snippets/65", handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal(65, snippet.Id);
        }
    }

    [Fact]
    public async Task GetAsync_WhenTheSnippetIsMissing_ThrowsGitLabNotFoundException()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"404 Snippet Not Found"}""", Encoding.UTF8,
                "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabNotFoundException exception =
                await Assert.ThrowsAsync<GitLabNotFoundException>(() =>
                    repository.GetAsync(65, TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }
    }

    [Fact]
    public async Task CreateAsync_SendsTheMultiFileForm()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SnippetJson, Encoding.UTF8, "application/json")
            };
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            CreateSnippetRequest request = new()
            {
                Title = "Sample snippet",
                Visibility = GitLabVisibility.Public,
                Files =
                [
                    new CreateSnippetFileRequest { FilePath = "src/App.cs", Content = "class App;" }
                ]
            };

            GitLabSnippet snippet = await repository.CreateAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/snippets", handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal(
                """{"title":"Sample snippet","visibility":"public","files":[{"file_path":"src/App.cs","content":"class App;"}]}""",
                sentBody);
            Assert.Equal(65, snippet.Id);
        }
    }

    [Fact]
    public async Task CreateForProjectAsync_SendsTheLegacySingleFileForm_OnTheProjectRoute()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(SnippetJson, Encoding.UTF8, "application/json")
            };
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            CreateSnippetRequest request = new()
            {
                Title = "Sample snippet",
                Visibility = GitLabVisibility.Private,
                Content = "puts hello",
                FileName = "add.rb"
            };

            await repository.CreateForProjectAsync(
                ProjectId.FromPath("gitlab-org/gitlab"),
                request,
                TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/snippets",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal(
                """{"title":"Sample snippet","visibility":"private","content":"puts hello","file_name":"add.rb"}""",
                sentBody);
        }
    }

    [Fact]
    public async Task UpdateAsync_SerializesTheFileActionsAsTheirWireValues()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SnippetJson, Encoding.UTF8, "application/json")
            };
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            UpdateSnippetRequest request = new()
            {
                Title = "Renamed",
                Files =
                [
                    new UpdateSnippetFileRequest
                    {
                        Action = GitLabSnippetFileAction.Move, FilePath = "src/App.cs", PreviousPath = "App.cs"
                    },
                    new UpdateSnippetFileRequest { Action = GitLabSnippetFileAction.Delete, FilePath = "old.rb" }
                ]
            };

            await repository.UpdateAsync(65, request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/snippets/65", handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal(
                """{"title":"Renamed","files":[{"action":"move","file_path":"src/App.cs","previous_path":"App.cs"},{"action":"delete","file_path":"old.rb"}]}""",
                sentBody);
        }
    }

    [Fact]
    public async Task UpdateForProjectAsync_BuildsTheProjectSnippetRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SnippetJson, Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.UpdateForProjectAsync(
                7,
                65,
                new UpdateSnippetRequest { Visibility = GitLabVisibility.Public },
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/7/snippets/65",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task DeleteAsync_IssuesADeleteAgainstTheSnippetRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeleteAsync(65, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/snippets/65", handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task DeleteForProjectAsync_IssuesADeleteAgainstTheProjectSnippetRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await repository.DeleteForProjectAsync(
                ProjectId.FromPath("gitlab-org/gitlab"),
                65,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Delete, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/snippets/65",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task ListForProjectAsync_EncodesANamespacedProjectPath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            await foreach (GitLabSnippet _ in repository.ListForProjectAsync(
                               ProjectId.FromPath("gitlab-org/sub/gitlab"),
                               TestContext.Current.CancellationToken))
            {
                // Draining the sequence is what issues the request.
            }

            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fsub%2Fgitlab/snippets",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
        }
    }

    [Fact]
    public async Task GetForProjectAsync_BuildsTheProjectSnippetRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(SnippetJson, Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabSnippet snippet =
                await repository.GetForProjectAsync(7, 65, TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/7/snippets/65",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal(65, snippet.Id);
        }
    }

    [Fact]
    public async Task GetRawAsync_StreamsThePlainTextBody_WithoutTouchingTheSerializer()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("puts hello", Encoding.UTF8, "text/plain")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file =
                await repository.GetRawAsync(65, TestContext.Current.CancellationToken);

            Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
            Assert.Equal("https://gitlab.example/api/v4/snippets/65/raw",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("text/plain", file.ContentType);

            using StreamReader reader = new(file.Content, Encoding.UTF8);
            Assert.Equal("puts hello", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task GetRawFileAsync_PercentEncodesTheRefAndTheFilePath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("class App;", Encoding.UTF8, "text/plain")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file = await repository.GetRawFileAsync(
                65,
                "feature/multi-file",
                "src/App.cs",
                TestContext.Current.CancellationToken);

            Assert.Equal(
                "https://gitlab.example/api/v4/snippets/65/files/feature%2Fmulti-file/src%2FApp.cs/raw",
                handler.LastRequest?.RequestUri?.AbsoluteUri);

            using StreamReader reader = new(file.Content, Encoding.UTF8);
            Assert.Equal("class App;", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task GetRawForProjectAsync_BuildsTheProjectRawRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("puts hello", Encoding.UTF8, "text/plain")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file = await repository.GetRawForProjectAsync(
                ProjectId.FromPath("gitlab-org/gitlab"),
                65,
                TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/snippets/65/raw",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        }
    }

    [Fact]
    public async Task GetRawFileForProjectAsync_PercentEncodesTheRefAndTheFilePath()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("class App;", Encoding.UTF8, "text/plain")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            using GitLabFileResponse file = await repository.GetRawFileForProjectAsync(
                7,
                65,
                "main",
                "src/App.cs",
                TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/projects/7/snippets/65/files/main/src%2FApp.cs/raw",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.NotNull(file.Content);
        }
    }

    [Fact]
    public async Task GetUserAgentDetailAsync_BuildsTheRoute_AndDeserializesTheDetail()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(UserAgentDetailJson, Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabUserAgentDetail detail =
                await repository.GetUserAgentDetailAsync(65, TestContext.Current.CancellationToken);

            Assert.Equal("https://gitlab.example/api/v4/snippets/65/user_agent_detail",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("AppleWebKit/537.36", detail.UserAgent);
            Assert.Equal("127.0.0.1", detail.IpAddress);
            Assert.False(detail.AkismetSubmitted);
        }
    }

    [Fact]
    public async Task GetUserAgentDetailForProjectAsync_BuildsTheProjectRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(UserAgentDetailJson, Encoding.UTF8, "application/json")
        });

        SnippetsRepository repository = CreateRepository(handler, out HttpClient httpClient);
        using (httpClient)
        {
            GitLabUserAgentDetail detail = await repository.GetUserAgentDetailForProjectAsync(
                ProjectId.FromPath("gitlab-org/gitlab"),
                65,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                "https://gitlab.example/api/v4/projects/gitlab-org%2Fgitlab/snippets/65/user_agent_detail",
                handler.LastRequest?.RequestUri?.AbsoluteUri);
            Assert.Equal("127.0.0.1", detail.IpAddress);
        }
    }
}