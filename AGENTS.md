# GitLab.Client — Codex instructions

## Subagent collaboration

Use subagents whenever a task contains two or more independent, bounded
investigations, reviews, documentation checks, test runs, or implementation
workstreams and parallel work will materially improve speed or quality.

- Prefer parallel subagents for read-heavy work: API/spec audits, codebase
  exploration, test analysis, diagnostics, and documentation verification.
- Delegate write-heavy work only when the files and responsibilities are
  disjoint. Assign ownership of files before agents edit them.
- The primary agent owns the final integration, resolves overlaps, and runs the
  relevant end-to-end verification.
- Give every subagent a concrete scope, acceptance criteria, and a request to
  return a concise summary with changed files and verification results.
- Do not spawn subagents for trivial, sequential, or tightly coupled work.

The project configuration sets a maximum of 50 concurrent subagent threads and
uses `gpt-5.6-terra` with `xhigh` reasoning effort by default. Explicit spawn
settings and project-specific custom agents may override these defaults.

## Project baseline

This is a .NET 10 / C# 14 GitLab REST API v4 client. Keep the compatibility
floor at GitLab 19.x, model payloads from the pinned official OpenAPI spec, and
preserve the Native AOT/trimming-safe, source-generated JSON approach. Follow
the repository's `CLAUDE.md` before changing client architecture or API
coverage.
