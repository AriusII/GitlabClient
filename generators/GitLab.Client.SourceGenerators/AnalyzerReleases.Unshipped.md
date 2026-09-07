; Unshipped analyzer
release ; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

 Rule ID | Category                 | Severity | Notes                                                                     
---------|--------------------------|----------|---------------------------------------------------------------------------
 GLC0001 | GitLab.Client.Generation | Error    | GenerateClientLayersGenerator: attribute argument is not an interface     
 GLC0002 | GitLab.Client.Generation | Error    | GenerateClientLayersGenerator: two repositories generate the same layer   
 GLC0003 | GitLab.Client.Generation | Error    | GenerateClientLayersGenerator: forwarding target lacks a matching member  
 GLC0004 | GitLab.Client.Generation | Error    | GenerateClientLayersGenerator: member cannot be forwarded                 
 GLC0005 | GitLab.Client.Generation | Warning  | GenerateClientLayersGenerator: repository interface naming convention     
 GLC0006 | GitLab.Client.Generation | Error    | GenerateClientLayersGenerator: unsupported repository interface shape     
 GLC0007 | GitLab.Client.Generation | Warning  | GenerateClientLayersGenerator: generated layer has no members             
 GLC0008 | GitLab.Client.Generation | Warning  | GenerateClientLayersGenerator: optional default cannot be reproduced      
 GLC0101 | GitLab.Client.Wiring     | Error    | GitLabClientWiringGenerator: duplicate root client property               
 GLC0102 | GitLab.Client.Wiring     | Error    | GitLabClientWiringGenerator: client interface is not public               
 GLC0103 | GitLab.Client.Wiring     | Error    | GitLabClientWiringGenerator: repository implementation not found          
 GLC0104 | GitLab.Client.Wiring     | Error    | GitLabClientWiringGenerator: root property name is not an identifier      
 GLQ0001 | GitLabQuery              | Error    | GitLabQueryGenerator: query option property type has no mapping           
 GLQ0002 | GitLabQuery              | Error    | GitLabQueryGenerator: query option property must be nullable              
 GLQ0003 | GitLabQuery              | Error    | GitLabQueryGenerator: invalid query-parameter name                        
 GLQ0004 | GitLabQuery              | Error    | GitLabQueryGenerator: enum member has no explicit wire name               
 GLQ0005 | GitLabQuery              | Error    | GitLabQueryGenerator: duplicate query-parameter name                      
 GLQ0006 | GitLabQuery              | Info     | GitLabQueryGenerator: type marked [GitLabQuery] contributes no parameters 
 GLQ0007 | GitLabQuery              | Error    | GitLabQueryGenerator: Repeated style requires a collection property       
