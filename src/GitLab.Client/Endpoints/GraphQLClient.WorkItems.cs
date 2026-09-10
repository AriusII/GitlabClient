using System.Text.Json.Serialization.Metadata;

using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.GraphQL.WorkItems;
using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Mutations;
using GitLab.Client.GraphQL.WorkItems.Queries;

using GitLabGraphQLJsonContext = GitLab.Client.GraphQL.Serialization.GitLabGraphQLJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed partial class GraphQLClient
{
    // The portable default deliberately requests no widgets. Widgets vary by type, tier, and permissions; callers
    // opt into one of the closed profiles below rather than paying for or depending on every widget implicitly.
    private const string CoreWorkItemSelection = """
                                                 id
                                                 iid
                                                 title
                                                 state
                                                 confidential
                                                 description
                                                 descriptionHtml
                                                 createdAt
                                                 updatedAt
                                                 closedAt
                                                 webUrl
                                                 author {
                                                   id
                                                   name
                                                   username
                                                   webUrl
                                                 }
                                                 workItemType {
                                                   id
                                                   name
                                                   iconName
                                                 }
                                                 """;

    private const string StandardWidgetFragments = """
                                                     ... on WorkItemWidgetDescription {
                                                       description
                                                       descriptionHtml
                                                     }
                                                     ... on WorkItemWidgetAssignees {
                                                       assignees {
                                                         nodes {
                                                           id
                                                           name
                                                           username
                                                           webUrl
                                                         }
                                                         pageInfo {
                                                           hasNextPage
                                                           hasPreviousPage
                                                           startCursor
                                                           endCursor
                                                         }
                                                       }
                                                     }
                                                     ... on WorkItemWidgetLabels {
                                                       labels {
                                                         nodes {
                                                           id
                                                           title
                                                           description
                                                           color
                                                           textColor
                                                         }
                                                         pageInfo {
                                                           hasNextPage
                                                           hasPreviousPage
                                                           startCursor
                                                           endCursor
                                                         }
                                                       }
                                                     }
                                                     ... on WorkItemWidgetMilestone {
                                                       milestone {
                                                         id
                                                         title
                                                         description
                                                         startDate
                                                         dueDate
                                                         webUrl
                                                       }
                                                     }
                                                   """;

    private const string PlanningWidgetFragments = """
                                                     ... on WorkItemWidgetColor {
                                                       color
                                                       textColor
                                                     }
                                                     ... on WorkItemWidgetStartAndDueDate {
                                                       startDate
                                                       dueDate
                                                       isFixed
                                                     }
                                                     ... on WorkItemWidgetIteration {
                                                       iteration {
                                                         id
                                                         title
                                                         description
                                                         startDate
                                                         dueDate
                                                         webUrl
                                                       }
                                                     }
                                                     ... on WorkItemWidgetHierarchy {
                                                       parent {
                                                         id
                                                         iid
                                                         title
                                                         state
                                                         webUrl
                                                         workItemType {
                                                           id
                                                           name
                                                           iconName
                                                         }
                                                       }
                                                       children {
                                                         nodes {
                                                           id
                                                           iid
                                                           title
                                                           state
                                                           webUrl
                                                           workItemType {
                                                             id
                                                             name
                                                             iconName
                                                           }
                                                         }
                                                         pageInfo {
                                                           hasNextPage
                                                           hasPreviousPage
                                                           startCursor
                                                           endCursor
                                                         }
                                                       }
                                                       ancestors {
                                                         nodes {
                                                           id
                                                           iid
                                                           title
                                                           state
                                                           webUrl
                                                           workItemType {
                                                             id
                                                             name
                                                             iconName
                                                           }
                                                         }
                                                         pageInfo {
                                                           hasNextPage
                                                           hasPreviousPage
                                                           startCursor
                                                           endCursor
                                                         }
                                                       }
                                                       hasChildren
                                                       hasParent
                                                     }
                                                   """;

    private const string UltimateWidgetFragments = """
                                                     ... on WorkItemWidgetHealthStatus {
                                                       healthStatus
                                                     }
                                                   """;

    private const string StandardWorkItemSelection = CoreWorkItemSelection + """
                                                                             widgets(onlyTypes: [DESCRIPTION, ASSIGNEES, LABELS, MILESTONE]) {
                                                                               __typename
                                                                             """ + StandardWidgetFragments + """
        }
        """;

    private const string PlanningWorkItemSelection = CoreWorkItemSelection + """
                                                                             widgets(onlyTypes: [COLOR, START_AND_DUE_DATE, ITERATION, HIERARCHY]) {
                                                                               __typename
                                                                             """ + PlanningWidgetFragments + """
        }
        """;

    private const string UltimateWorkItemSelection = CoreWorkItemSelection + """
                                                                             widgets(onlyTypes: [HEALTH_STATUS]) {
                                                                               __typename
                                                                             """ + UltimateWidgetFragments + """
        }
        """;

    private const string ComprehensiveWorkItemSelection = CoreWorkItemSelection + """
        widgets(onlyTypes: [DESCRIPTION, ASSIGNEES, LABELS, MILESTONE, COLOR, START_AND_DUE_DATE, ITERATION, HIERARCHY, HEALTH_STATUS]) {
          __typename
        """ + StandardWidgetFragments + PlanningWidgetFragments + UltimateWidgetFragments + """
        }
        """;

    private const string GetWorkItemTypesDocument = """
                                                    query GetWorkItemTypes($fullPath: ID!, $first: Int, $after: String) {
                                                      namespace(fullPath: $fullPath) {
                                                        id
                                                        fullPath
                                                        workItemTypes(first: $first, after: $after) {
                                                          nodes {
                                                            id
                                                            name
                                                            iconName
                                                          }
                                                          pageInfo {
                                                            hasNextPage
                                                            hasPreviousPage
                                                            startCursor
                                                            endCursor
                                                          }
                                                        }
                                                      }
                                                    }
                                                    """;

    // A profile document intentionally contains every read operation. GraphQL executes only the named operation in
    // the envelope's operationName, so there are exactly five closed documents rather than run-time-composed query
    // strings for each method/profile pair.
    private const string CoreWorkItemsQueryDocument = """
                                                      query GetWorkItemById($id: WorkItemID!) {
                                                        workItem(id: $id) {
                                                      """ + CoreWorkItemSelection + """
          }
        }

        query GetWorkItemByLocator($fullPath: ID!, $iid: String!) {
          namespace(fullPath: $fullPath) {
            id
            fullPath
            workItem(iid: $iid) {
        """ + CoreWorkItemSelection + """
                                          }
                                        }
                                      }

                                      query ListWorkItems($fullPath: ID!, $first: Int, $after: String) {
                                        namespace(fullPath: $fullPath) {
                                          id
                                          fullPath
                                          workItems(first: $first, after: $after) {
                                            nodes {
                                      """ + CoreWorkItemSelection + """
                                                                          }
                                                                          pageInfo {
                                                                            hasNextPage
                                                                            hasPreviousPage
                                                                            startCursor
                                                                            endCursor
                                                                          }
                                                                        }
                                                                      }
                                                                    }
                                                                    """;

    private const string StandardWorkItemsQueryDocument = """
                                                          query GetWorkItemById($id: WorkItemID!) {
                                                            workItem(id: $id) {
                                                          """ + StandardWorkItemSelection + """
          }
        }

        query GetWorkItemByLocator($fullPath: ID!, $iid: String!) {
          namespace(fullPath: $fullPath) {
            id
            fullPath
            workItem(iid: $iid) {
        """ + StandardWorkItemSelection + """
                                              }
                                            }
                                          }

                                          query ListWorkItems($fullPath: ID!, $first: Int, $after: String) {
                                            namespace(fullPath: $fullPath) {
                                              id
                                              fullPath
                                              workItems(first: $first, after: $after) {
                                                nodes {
                                          """ + StandardWorkItemSelection + """
                                                                                  }
                                                                                  pageInfo {
                                                                                    hasNextPage
                                                                                    hasPreviousPage
                                                                                    startCursor
                                                                                    endCursor
                                                                                  }
                                                                                }
                                                                              }
                                                                            }
                                                                            """;

    private const string PlanningWorkItemsQueryDocument = """
                                                          query GetWorkItemById($id: WorkItemID!) {
                                                            workItem(id: $id) {
                                                          """ + PlanningWorkItemSelection + """
          }
        }

        query GetWorkItemByLocator($fullPath: ID!, $iid: String!) {
          namespace(fullPath: $fullPath) {
            id
            fullPath
            workItem(iid: $iid) {
        """ + PlanningWorkItemSelection + """
                                              }
                                            }
                                          }

                                          query ListWorkItems($fullPath: ID!, $first: Int, $after: String) {
                                            namespace(fullPath: $fullPath) {
                                              id
                                              fullPath
                                              workItems(first: $first, after: $after) {
                                                nodes {
                                          """ + PlanningWorkItemSelection + """
                                                                                  }
                                                                                  pageInfo {
                                                                                    hasNextPage
                                                                                    hasPreviousPage
                                                                                    startCursor
                                                                                    endCursor
                                                                                  }
                                                                                }
                                                                              }
                                                                            }
                                                                            """;

    private const string UltimateWorkItemsQueryDocument = """
                                                          query GetWorkItemById($id: WorkItemID!) {
                                                            workItem(id: $id) {
                                                          """ + UltimateWorkItemSelection + """
          }
        }

        query GetWorkItemByLocator($fullPath: ID!, $iid: String!) {
          namespace(fullPath: $fullPath) {
            id
            fullPath
            workItem(iid: $iid) {
        """ + UltimateWorkItemSelection + """
                                              }
                                            }
                                          }

                                          query ListWorkItems($fullPath: ID!, $first: Int, $after: String) {
                                            namespace(fullPath: $fullPath) {
                                              id
                                              fullPath
                                              workItems(first: $first, after: $after) {
                                                nodes {
                                          """ + UltimateWorkItemSelection + """
                                                                                  }
                                                                                  pageInfo {
                                                                                    hasNextPage
                                                                                    hasPreviousPage
                                                                                    startCursor
                                                                                    endCursor
                                                                                  }
                                                                                }
                                                                              }
                                                                            }
                                                                            """;

    private const string ComprehensiveWorkItemsQueryDocument = """
                                                               query GetWorkItemById($id: WorkItemID!) {
                                                                 workItem(id: $id) {
                                                               """ + ComprehensiveWorkItemSelection + """
          }
        }

        query GetWorkItemByLocator($fullPath: ID!, $iid: String!) {
          namespace(fullPath: $fullPath) {
            id
            fullPath
            workItem(iid: $iid) {
        """ + ComprehensiveWorkItemSelection + """
                                                   }
                                                 }
                                               }

                                               query ListWorkItems($fullPath: ID!, $first: Int, $after: String) {
                                                 namespace(fullPath: $fullPath) {
                                                   id
                                                   fullPath
                                                   workItems(first: $first, after: $after) {
                                                     nodes {
                                               """ + ComprehensiveWorkItemSelection + """
              }
              pageInfo {
                hasNextPage
                hasPreviousPage
                startCursor
                endCursor
              }
            }
          }
        }
        """;

    private const string CreateWorkItemDocument = """
                                                  mutation CreateWorkItem($input: WorkItemCreateInput!) {
                                                    workItemCreate(input: $input) {
                                                      clientMutationId
                                                      errors
                                                      workItem {
                                                  """ + CoreWorkItemSelection + """
            }
          }
        }
        """;

    private const string UpdateWorkItemDocument = """
                                                  mutation UpdateWorkItem($input: WorkItemUpdateInput!) {
                                                    workItemUpdate(input: $input) {
                                                      clientMutationId
                                                      errors
                                                      workItem {
                                                  """ + CoreWorkItemSelection + """
            }
          }
        }
        """;

    private const string DeleteWorkItemDocument = """
                                                  mutation DeleteWorkItem($input: WorkItemDeleteInput!) {
                                                    workItemDelete(input: $input) {
                                                      clientMutationId
                                                      errors
                                                      namespace {
                                                        id
                                                        fullPath
                                                      }
                                                    }
                                                  }
                                                  """;

    public Task<GitLabGraphQLResponse<GitLabWorkItemTypesQueryData>> GetTypesAsync(
        string namespacePath,
        int? first = null,
        string? after = null,
        CancellationToken cancellationToken = default)
    {
        GitLabWorkItemTypesQueryVariables variables = new(namespacePath, first, after);

        return ExecuteWorkItemAsync(
            GetWorkItemTypesDocument,
            variables,
            GitLabGraphQLJsonContext.Default.GitLabWorkItemTypesQueryVariables,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemTypesQueryData,
            "GetWorkItemTypes",
            cancellationToken);
    }

    public Task<GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>> GetAsync(
        GitLabGraphQLGlobalId id,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(id, GitLabWorkItemWidgetProfile.Core, cancellationToken);
    }

    public Task<GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>> GetAsync(
        GitLabGraphQLGlobalId id,
        GitLabWorkItemWidgetProfile widgetProfile,
        CancellationToken cancellationToken = default)
    {
        GitLabWorkItemByIdQueryVariables variables = new(id);

        return ExecuteWorkItemAsync(
            GetWorkItemsQueryDocument(widgetProfile),
            variables,
            GitLabGraphQLJsonContext.Default.GitLabWorkItemByIdQueryVariables,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemByIdQueryData,
            "GetWorkItemById",
            cancellationToken);
    }

    public Task<GitLabGraphQLResponse<GitLabWorkItemByLocatorQueryData>> GetAsync(
        GitLabWorkItemLocator locator,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(locator, GitLabWorkItemWidgetProfile.Core, cancellationToken);
    }

    public Task<GitLabGraphQLResponse<GitLabWorkItemByLocatorQueryData>> GetAsync(
        GitLabWorkItemLocator locator,
        GitLabWorkItemWidgetProfile widgetProfile,
        CancellationToken cancellationToken = default)
    {
        GitLabWorkItemByLocatorQueryVariables variables = new(locator);

        return ExecuteWorkItemAsync(
            GetWorkItemsQueryDocument(widgetProfile),
            variables,
            GitLabGraphQLJsonContext.Default.GitLabWorkItemByLocatorQueryVariables,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemByLocatorQueryData,
            "GetWorkItemByLocator",
            cancellationToken);
    }

    public Task<GitLabGraphQLResponse<GitLabWorkItemsQueryData>> ListAsync(
        string namespacePath,
        int? first = null,
        string? after = null,
        CancellationToken cancellationToken = default)
    {
        return ListAsync(namespacePath, first, after, GitLabWorkItemWidgetProfile.Core, cancellationToken);
    }

    public Task<GitLabGraphQLResponse<GitLabWorkItemsQueryData>> ListAsync(
        string namespacePath,
        int? first,
        string? after,
        GitLabWorkItemWidgetProfile widgetProfile,
        CancellationToken cancellationToken = default)
    {
        GitLabWorkItemsQueryVariables variables = new(namespacePath, first, after);

        return ExecuteWorkItemAsync(
            GetWorkItemsQueryDocument(widgetProfile),
            variables,
            GitLabGraphQLJsonContext.Default.GitLabWorkItemsQueryVariables,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemsQueryData,
            "ListWorkItems",
            cancellationToken);
    }

    public Task<GitLabGraphQLResponse<GitLabWorkItemCreateMutationData>> CreateAsync(
        GitLabWorkItemCreateInput input,
        CancellationToken cancellationToken = default)
    {
        GitLabWorkItemCreateMutationVariables variables = new(input);

        return ExecuteWorkItemAsync(
            CreateWorkItemDocument,
            variables,
            GitLabGraphQLJsonContext.Default.GitLabWorkItemCreateMutationVariables,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemCreateMutationData,
            "CreateWorkItem",
            cancellationToken);
    }

    public Task<GitLabGraphQLResponse<GitLabWorkItemUpdateMutationData>> UpdateAsync(
        GitLabWorkItemUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        GitLabWorkItemUpdateMutationVariables variables = new(input);

        return ExecuteWorkItemAsync(
            UpdateWorkItemDocument,
            variables,
            GitLabGraphQLJsonContext.Default.GitLabWorkItemUpdateMutationVariables,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemUpdateMutationData,
            "UpdateWorkItem",
            cancellationToken);
    }

    public Task<GitLabGraphQLResponse<GitLabWorkItemDeleteMutationData>> DeleteAsync(
        GitLabWorkItemDeleteInput input,
        CancellationToken cancellationToken = default)
    {
        GitLabWorkItemDeleteMutationVariables variables = new(input);

        return ExecuteWorkItemAsync(
            DeleteWorkItemDocument,
            variables,
            GitLabGraphQLJsonContext.Default.GitLabWorkItemDeleteMutationVariables,
            GitLabGraphQLJsonContext.Default.GitLabGraphQLResponseGitLabWorkItemDeleteMutationData,
            "DeleteWorkItem",
            cancellationToken);
    }

    private static string GetWorkItemsQueryDocument(GitLabWorkItemWidgetProfile widgetProfile)
    {
        return widgetProfile switch
        {
            GitLabWorkItemWidgetProfile.Core => CoreWorkItemsQueryDocument,
            GitLabWorkItemWidgetProfile.Standard => StandardWorkItemsQueryDocument,
            GitLabWorkItemWidgetProfile.Planning => PlanningWorkItemsQueryDocument,
            GitLabWorkItemWidgetProfile.Ultimate => UltimateWorkItemsQueryDocument,
            GitLabWorkItemWidgetProfile.Comprehensive => ComprehensiveWorkItemsQueryDocument,
            _ => throw new ArgumentOutOfRangeException(nameof(widgetProfile), widgetProfile,
                "The work-item widget profile must be one of the curated profiles.")
        };
    }

    private Task<GitLabGraphQLResponse<TData>> ExecuteWorkItemAsync<TVariables, TData>(
        string document,
        TVariables variables,
        JsonTypeInfo<TVariables> variablesTypeInfo,
        JsonTypeInfo<GitLabGraphQLResponse<TData>> responseTypeInfo,
        string operationName,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            GitLabGraphQLRequest.Create(document, variables, variablesTypeInfo, operationName),
            responseTypeInfo,
            cancellationToken);
    }
}