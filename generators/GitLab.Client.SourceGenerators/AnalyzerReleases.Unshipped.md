; Unshipped analyzer
release ; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

 Rule ID | Category             | Severity | Notes
---------|----------------------|----------|---------------------------------------------------------------------------
 GLC0201 | GitLab.Client.Wiring | Error    | GitLabClientWiringGenerator: invalid root resource-client property
 GLC0202 | GitLab.Client.Wiring | Error    | GitLabClientWiringGenerator: direct endpoint is missing or incompatible
 GLQ0001 | GitLabQuery          | Error    | GitLabQueryGenerator: query option property type has no mapping
 GLQ0002 | GitLabQuery          | Error    | GitLabQueryGenerator: query option property must be nullable
 GLQ0003 | GitLabQuery          | Error    | GitLabQueryGenerator: invalid query-parameter name
 GLQ0004 | GitLabQuery          | Error    | GitLabQueryGenerator: enum member has no explicit wire name
 GLQ0005 | GitLabQuery          | Error    | GitLabQueryGenerator: duplicate query-parameter name
 GLQ0006 | GitLabQuery          | Info     | GitLabQueryGenerator: type marked [GitLabQuery] contributes no parameters
 GLQ0007 | GitLabQuery          | Error    | GitLabQueryGenerator: Repeated style requires a collection property
