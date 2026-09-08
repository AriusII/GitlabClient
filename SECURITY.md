# Security Policy

## Supported versions

This project tracks the latest commit on `main`. There are no maintained release branches at this time, so security fixes are only provided against `main` / the latest published NuGet package.

## Reporting a vulnerability

Please do **not** open a public GitHub issue for a security vulnerability.

Instead, use GitHub's private vulnerability reporting for this repository: go to the **Security** tab → **Report a vulnerability**, or use this link directly:

https://github.com/AriusII/GitlabClient/security/advisories/new

This opens a private security advisory visible only to the maintainer until a fix is ready. If private reporting is not enabled on the repository yet, please reach out to the maintainer, [@AriusII](https://github.com/AriusII), through their GitHub profile instead of filing a public issue.

## What to expect

- Acknowledgement of the report within a reasonable timeframe.
- A fix, mitigation plan, or explanation communicated back to the reporter.
- Public disclosure coordinated with the reporter, only after a fix has shipped.

## Scope

`GitLab.Client` is a .NET REST API client for GitLab. Security-relevant areas include, non-exhaustively:

- Credential handling (`PRIVATE-TOKEN`, `Authorization: Bearer`, `JOB-TOKEN`) — tokens must never leak into logs, exception messages, or serialized output.
- Request routing and encoding (`GitLabRouteBuilder`) — unescaped path segments or query values that could redirect a call to an unintended host or path.
- JSON (de)serialization via the `System.Text.Json` source-generated contexts.
- Anything that would only surface under Native AOT/trimming (e.g. a reflection fallback silently reintroduced).

Dependency vulnerabilities (NuGet advisories) are also in scope — see the tracking issue for enabling Dependabot alerts on this repository.
