## Summary

<!-- What does this change do, and why? -->

## Related issue

<!-- Closes #... -->

## Checklist

- [ ] `dotnet build GitlabClient.slnx -c Release` builds with 0 warnings
- [ ] `dotnet test --solution GitlabClient.slnx` passes
- [ ] New/changed resources use a direct `I<Resource>Client → <Resource>Client → IGitLabApiConnection` route implementation; no Controller, Service, or Repository forwarding layer is introduced
- [ ] Routes are built via `GitLabRouteBuilder` — never hand-interpolated
- [ ] New or changed endpoint payloads have explicit request/response DTOs and are registered in `GitLabJsonContext`
- [ ] API-surface changes link the corresponding official GitLab REST API documentation and state any intentional unsupported behavior
- [ ] A transport, JSON, DI, or generator change preserves Native AOT/trimming safety; when applicable, `dotnet publish samples/GitLab.Client.AotHarness -c Release -r win-x64` succeeds
- [ ] If a package version changes, the published `GitLab.Client` package version and its embedded assembly versions remain aligned
- [ ] No secrets, tokens, or real credentials are included in code, tests, fixtures, or commit messages
