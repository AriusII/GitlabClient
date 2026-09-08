# GitLab.Client

A fully-typed, dependency-injection-first C# client for the [GitLab REST API (v4)](https://docs.gitlab.com/ee/api/rest/), targeting **GitLab 19.x and above**. Built on **C# 14 / .NET 10**, it is **Native AOT and trimming friendly** end to end — every DTO is serialized through `System.Text.Json` source generators, there is no reflection-based JSON, configuration binding, or DI scanning anywhere in the library, and the public surface publishes as a single `GitLab.Client` NuGet package consumed purely through `services.AddGitLabClient(...)`.

Every resource client (Projects, MergeRequests, Pipelines, Issues, and well over a hundred more) follows the same generated `Controllers → Services → Repositories` layering on top of a shared HTTP/serialization core, is independently injectable, and is also reachable off a single root `IGitLabClient`. See [Coverage](#coverage) for exactly how much of the GitLab API surface that spans today.

## Table of contents

- [Installation](#installation)
- [Quickstart](#quickstart)
- [Configuration](#configuration)
- [Architecture](#architecture)
  - [Every resource, two ways to reach it](#every-resource-two-ways-to-reach-it)
  - [Typed exceptions](#typed-exceptions)
  - [Streaming pagination](#streaming-pagination)
  - [Native AOT and trimming](#native-aot-and-trimming)
- [Coverage](#coverage)
- [Contributing](#contributing)
- [Security](#security)
- [License](#license)

## Installation

```bash
dotnet add package GitLab.Client
```

The library targets `net10.0` and has no dependency beyond `Microsoft.Extensions.Http`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Options`, and `Microsoft.Extensions.Logging.Abstractions`.

## Quickstart

Register the client with dependency injection, then resolve `IGitLabClient` (or any single resource client) from the container. This example is adapted from the Native AOT smoke test at `samples/GitLab.Client.AotHarness/Program.cs`, which is published with `PublishAot=true` and treats any trim/AOT warning as a build break:

```csharp
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
```

`gitLab.Projects` is one property among 143 on `IGitLabClient` — every resource this library wraps is reachable the same way (`gitLab.MergeRequests`, `gitLab.Pipelines`, `gitLab.Issues`, and so on). Each of those is also independently injectable — see [Every resource, two ways to reach it](#every-resource-two-ways-to-reach-it).

## Configuration

As shown above, `AddGitLabClient` currently takes a delegate against `GitLabClientOptions`:

```csharp
services.AddGitLabClient(options =>
{
    options.BaseAddress = new Uri("https://gitlab.example.com/api/v4/"); // defaults to https://gitlab.com/api/v4/
    options.AccessToken = "glpat-...";
    options.AuthenticationMode = GitLabAuthenticationMode.PersonalAccessToken; // or OAuthBearer, JobToken
    options.UserAgent = "MyApp/1.0";
    options.Timeout = TimeSpan.FromSeconds(100);
});
```

| Option | Default | Notes |
|---|---|---|
| `BaseAddress` | `https://gitlab.com/api/v4/` | Point this at a self-managed instance's `/api/v4/` root. Must keep the trailing slash. |
| `AccessToken` | `null` | A personal access token, OAuth token, or CI job token, depending on `AuthenticationMode`. |
| `AuthenticationMode` | `PersonalAccessToken` | Selects the header: `PRIVATE-TOKEN`, `Authorization: Bearer`, or `JOB-TOKEN` respectively. |
| `UserAgent` | `GitLab.Client/1.0` | Sent as the `User-Agent` header on every request. |
| `Timeout` | 100 seconds | Applied via a linked cancellation token, so it also bounds a response whose headers arrived but whose body stalls. |

### From `appsettings.json`

`AddGitLabClient` also has an `IConfiguration` overload, binding `GitLabClientOptions` from a configuration section (`"GitLab"` by default):

```json
{
  "GitLab": {
    "BaseAddress": "https://gitlab.com/api/v4/",
    "AccessToken": ""
  }
}
```

```csharp
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

services.AddGitLabClient(configuration); // binds the "GitLab" section; pass a second string argument for a different section name
```

Any key omitted from the section keeps `GitLabClientOptions`'s own default (`BaseAddress` still resolves to `gitlab.com` if left out). Binding goes through the source-generated Configuration Binder (`EnableConfigurationBindingGenerator`), not reflection, so it stays Native AOT/trim-safe. See [`samples/GitLab.Client.AotHarness`](samples/GitLab.Client.AotHarness) for a complete example that falls back to a `GITLAB_TOKEN` environment variable when the configured token is empty.

`AddGitLabClient` returns an `IHttpClientBuilder`, so you can chain your own handlers or resilience policies (e.g. `Microsoft.Extensions.Http.Resilience`) onto the same named HTTP client the library uses internally.

## Architecture

The library is organized as a DDD-flavoured `Controllers → Services → Repositories` chain per resource, on top of a shared HTTP/serialization core. Three Roslyn incremental source generators keep that repetitive per-resource plumbing out of the hand-written surface: one emits the `Controller`/`Service` forwarding pair for every repository interface marked `[GenerateClientLayers]`, one collects all of those to generate both the DI registrations and the `IGitLabClient` root aggregate, and one turns `*ListOptions` records into query-string builders. The full design — including why each of these generators exists and the conventions every resource follows — is documented in [`CLAUDE.md`](CLAUDE.md); the summary below covers what matters to a consumer.

### Every resource, two ways to reach it

Every resource is exposed as a property on the root `IGitLabClient` (`gitLab.Projects`, `gitLab.MergeRequests`, `gitLab.Pipelines`, ...) **and** independently injectable via its own interface (`IProjectsClient`, `IMergeRequestsClient`, ...), so a consumer that only needs one or two resources doesn't have to take on the whole aggregate.

### Typed exceptions

Every failed call throws from a hierarchy rooted at `GitLabApiException`, with seven derived types so callers can catch exactly the failure mode they care about:

- `GitLabAuthenticationException` — 401
- `GitLabForbiddenException` — 403
- `GitLabNotFoundException` — 404
- `GitLabConflictException` — 409
- `GitLabValidationException` — 400 / 422, carrying GitLab's per-field validation errors
- `GitLabRateLimitExceededException` — 429, carrying `RetryAfter` and a rate-limit snapshot
- `GitLabServerException` — 5xx

`catch (GitLabApiException)` still catches all of them. Transport-level failures are deliberately left unwrapped: DNS/TLS/socket errors surface as `HttpRequestException`, and cancellation surfaces as `OperationCanceledException`.

### Streaming pagination

List operations return `IAsyncEnumerable<T>`, following GitLab's RFC 5988 `Link: rel="next"` header page by page rather than buffering an eager `List<T>` of the whole collection.

### Native AOT and trimming

The library builds with `IsAotCompatible` and `IsTrimmable` enabled and `TreatWarningsAsErrors` on IL trim/AOT diagnostics — no `dynamic`, no reflection-based JSON or configuration binding, no assembly scanning for DI, and a `sealed`-by-default public surface. `samples/GitLab.Client.AotHarness` is a real Native AOT publish target (`dotnet publish samples/GitLab.Client.AotHarness -c Release -r win-x64`) that exercises DI registration, the root client, and one live resource call end to end, and is the gate this repository verifies against for every change.

## Coverage

Coverage against GitLab's OpenAPI spec (vendored at `spec/openapi_v3.yaml`) is tracked in [`ROADMAP.md`](ROADMAP.md), which is regenerated from a full per-tag audit rather than hand-counted. As of the most recent audit:

| | |
|---|---|
| Resources implemented | **143** distinct repository interfaces, each with the full generated layering |
| Operations in the spec | 1847, across 170 tags |
| Operations verified implemented | **1683** (**91.1%** of the whole spec) |
| Coverage of the in-scope surface (excluding deliberately-parked deprecated tags/operations) | **1683 / 1683 = 100%** |
| Tests | **2387**, all passing |
| Build warnings | **0** (`TreatWarningsAsErrors` + `AnalysisLevel=latest-all`) |
| Native AOT publish | Clean — **0** IL2xxx/IL3xxx trim/AOT warnings |

See [`ROADMAP.md`](ROADMAP.md) for the remaining genuine gaps, what is deliberately out of scope (deprecated GitLab API surface, per the version-baseline policy below), and why.

GitLab 19.x is the floor: anything the spec marks as introduced in 19.0–19.4 is fair game, and prose- or flag-deprecated surface is deliberately excluded rather than wrapped — see `CLAUDE.md`'s *Version baseline* section for the exact policy.

## Contributing

The architecture, generator internals, routing/JSON/error-handling conventions, and build/test commands are documented in [`CLAUDE.md`](CLAUDE.md) at the repository root — read it before making a change, especially before adding a new resource or touching the source generators. Pull requests are reviewed against [`CODEOWNERS`](CODEOWNERS), and the templates under [`.github/ISSUE_TEMPLATE/`](.github/ISSUE_TEMPLATE) and [`.github/PULL_REQUEST_TEMPLATE.md`](.github/PULL_REQUEST_TEMPLATE.md) describe the information an issue or PR should include.

## Security

See [`SECURITY.md`](SECURITY.md) for the supported-versions policy and how to report a vulnerability privately.

## License

Licensed under the [MIT License](LICENSE).
