// Native AOT smoke test: exercises DI registration, the root client, and one real resource
// call end to end. Publish with `dotnet publish -c Release -r <rid> -p:PublishAot=true` and
// treat any trim/AOT warning from this project or GitLab.Client as a build break.

using GitLab.Client.Abstractions;
using GitLab.Client.Abstractions.Exceptions;
using GitLab.Client.Models;

using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();

services.AddGitLabClient(options =>
    options.AccessToken = Environment.GetEnvironmentVariable("GITLAB_TOKEN") ?? "glpat-example-token");

await using ServiceProvider provider = services.BuildServiceProvider();

IGitLabClient gitLab = provider.GetRequiredService<IGitLabClient>();

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