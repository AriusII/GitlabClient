## Summary

<!-- What does this change do, and why? -->

## Related issue

<!-- Closes #... -->

## Checklist

- [ ] `dotnet build GitlabClient.slnx -c Release` builds with 0 warnings
- [ ] `dotnet test GitlabClient.slnx` passes
- [ ] New/changed resources follow the `Controllers → Services → Repositories` pattern (see `CLAUDE.md`)
- [ ] Routes are built via `GitLabRouteBuilder` — never hand-interpolated
- [ ] New DTOs are registered in `GitLabJsonContext`
- [ ] No secrets, tokens, or real credentials are included in code, tests, fixtures, or commit messages
