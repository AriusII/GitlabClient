// Native AOT smoke test: exercises DI registration, the root client, and one real resource
// call end to end. Publish with `dotnet publish -c Release -r <rid> -p:PublishAot=true` and
// treat any trim/AOT warning from this project or GitLab.Client as a build break.

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Batching;
using GitLab.Client.Configuration;
using GitLab.Client.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true)
    .AddEnvironmentVariables()
    .Build();

ServiceCollection services = new();

services.AddGitLabClient(configuration);

// appsettings.json ships with an empty AccessToken so the file can be committed; fall back to
// GITLAB_TOKEN (and finally a placeholder) whenever the bound configuration didn't supply one.
services.PostConfigure<GitLabClientOptions>(options =>
{
    if (string.IsNullOrEmpty(options.AccessToken))
    {
        options.AccessToken = Environment.GetEnvironmentVariable("GITLAB_TOKEN") ?? "glpat-example-token";
    }
});

await using ServiceProvider provider = services.BuildServiceProvider();

IGitLabClient gitLab = provider.GetRequiredService<IGitLabClient>();

// Resolve the GraphQL root and its curated Work Items entry point without sending a request. This
// keeps the Native AOT smoke test network-free for GraphQL while retaining the generated root-client
// wiring and direct GraphQL client implementation in the published application.
IGraphQLClient graphQL = gitLab.GraphQL;
ArgumentNullException.ThrowIfNull(graphQL.WorkItems);
Console.WriteLine("GitLab GraphQL Work Items client resolved without executing a request.");

// Exercise the client-side batch facade through the generated root client without creating network traffic. This
// keeps its generic contracts and generated DI wiring in the Native AOT reachability graph.
GitLabBatchExecution emptyBatch = await gitLab.Batches.Create().ExecuteAsync();
Console.WriteLine($"GitLab batch client resolved with {emptyBatch.Count} queued operations.");

try
{
    GitLabProject project = await gitLab.Projects.GetAsync("gitlab-org/gitlab");
    Console.WriteLine($"{project.PathWithNamespace} ({project.Visibility}): {project.WebUrl}");
}
catch (GitLabAuthenticationException ex)
{
    Console.WriteLine($"GitLab rejected the token: {ex.Message}");
}
catch (GitLabNotFoundException ex)
{
    Console.WriteLine($"Project not found or not visible to this token: {ex.Message}");
}
catch (GitLabRateLimitExceededException ex)
{
    Console.WriteLine($"GitLab throttled the request; retry after {ex.RetryAfter}.");
}
catch (GitLabValidationException ex)
{
    Console.WriteLine($"GitLab rejected the request ({ex.Errors.Count} field errors): {ex.Message}");
}
catch (GitLabServerException ex)
{
    Console.WriteLine($"GitLab failed to handle the request (transient: {ex.IsTransient}): {ex.Message}");
}
catch (GitLabApiException ex)
{
    Console.WriteLine($"GitLab API call failed ({(int)ex.StatusCode} {ex.StatusCode}): {ex.Message}");
}