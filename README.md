# GitLab.Client

`GitLab.Client` est un SDK .NET typé pour communiquer avec l’API REST v4 et l’API GraphQL officielle de GitLab. Il permet à une application .NET d’intégrer GitLab.com ou une instance GitLab auto gérée avec des contrats C# explicites, une intégration native à l’injection de dépendances et une exécution asynchrone.

Le projet cible GitLab 19.x et versions ultérieures. GitLab 19.0 est la version minimale prise en charge ; les contrats REST sont construits à partir d’une spécification OpenAPI GitLab 19.4 épinglée dans le dépôt. L’API REST GitLab reste versionnée `v4`, tandis que GraphQL utilise son endpoint versionless `/api/graphql`.

## Ce que fournit la bibliothèque

1. Des clients REST directs et typés pour plus d’une centaine de domaines GitLab, accessibles depuis `IGitLabClient` ou injectables individuellement.

2. Des DTO, requêtes, réponses, identifiants de projet et de groupe, options de filtrage et exceptions HTTP spécialisés pour les routes GitLab.

3. Une intégration `IHttpClientFactory` via `services.AddGitLabClient(...)`, avec authentification par jeton personnel, OAuth Bearer ou jeton de job GitLab.

4. La pagination asynchrone par `IAsyncEnumerable<T>`, qui suit les liens de pagination GitLab sans charger toutes les pages en mémoire.

5. Une façade GraphQL comprenant un exécuteur typé pour vos documents, le multiplexage GitLab et une surface dédiée aux Work Items.

6. Des plans de batch asynchrones à concurrence bornée pour exécuter ensemble des appels REST et GraphQL indépendants.

7. Des mappers purs qui assemblent des DTO déjà chargés en objets de composition riches, sans appel HTTP caché, cache implicite ni lazy loading.

## Compatibilité

| Élément | Valeur |
| :--- | :--- |
| Package NuGet | `GitLab.Client` |
| Runtime consommateur | .NET 10, `net10.0` |
| Langage | C# 14 |
| GitLab minimum | GitLab 19.0 |
| Référence OpenAPI REST incluse | GitLab 19.4 |
| APIs | REST v4 et GraphQL |
| Licence | MIT |

Le package final est unique. Il contient les assemblies Contracts, Routing, Http et la façade `GitLab.Client` ; aucune dépendance NuGet GitLab interne supplémentaire ne doit être installée.

## Installation

```bash
dotnet add package GitLab.Client
```

Le point d’entrée est l’extension `AddGitLabClient`. Une application inscrit la bibliothèque une seule fois dans son conteneur de services.

## Démarrage rapide

Le code suivant configure un client pour GitLab.com, résout le client racine et charge un projet par son chemin d’espace de noms.

```csharp
using GitLab.Client.Abstractions;
using GitLab.Client.Models;

using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();

services.AddGitLabClient(options =>
{
    options.AccessToken = Environment.GetEnvironmentVariable("GITLAB_TOKEN")
        ?? throw new InvalidOperationException("The GITLAB_TOKEN environment variable is required.");
});

await using ServiceProvider provider = services.BuildServiceProvider();

IGitLabClient gitLab = provider.GetRequiredService<IGitLabClient>();
GitLabProject project = await gitLab.Projects.GetAsync("group/subgroup/project");

Console.WriteLine(project.WebUrl);
```

Le client accepte un identifiant numérique ou un chemin GitLab pour les projets et groupes lorsque la route GitLab le permet. Les segments de chemin sont encodés par la bibliothèque, ce qui évite les erreurs fréquentes avec les espaces de noms imbriqués.

## Configuration

Par défaut, le client utilise `https://gitlab.com/api/v4/`. Pour une instance auto gérée, configurez la racine REST avec son slash final.

```csharp
services.AddGitLabClient(options =>
{
    options.BaseAddress = new Uri("https://gitlab.example.com/api/v4/");
    options.AccessToken = Environment.GetEnvironmentVariable("GITLAB_TOKEN");
    options.UserAgent = "MyProduct/1.0";
    options.Timeout = TimeSpan.FromSeconds(100);
});
```

L’authentification utilise un jeton personnel par défaut. `GitLabAuthenticationMode` permet aussi de sélectionner OAuth Bearer ou un jeton de job GitLab. Les jetons doivent être fournis par un mécanisme adapté à votre application, par exemple des variables d’environnement, le gestionnaire de secrets .NET ou un coffre de secrets.

L’endpoint GraphQL est automatiquement dérivé de l’adresse REST. Il peut être configuré explicitement pour une installation inhabituelle, mais il doit rester sur la même origine afin que les identifiants ne soient jamais envoyés à un autre hôte.

### Configuration avec `IConfiguration`

L’intégration avec une application ASP.NET Core ou un hôte .NET peut s’appuyer directement sur une section de configuration nommée `GitLab`.

```json
{
  "GitLab": {
    "BaseAddress": "https://gitlab.example.com/api/v4/",
    "AccessToken": ""
  }
}
```

```csharp
builder.Services.AddGitLabClient(builder.Configuration);
```

Vous pouvez fournir un nom de section différent avec `AddGitLabClient(configuration, "MyGitLab")`.

## Utiliser l’API REST

Chaque domaine GitLab est une propriété du client racine. Les clients sont également injectables individuellement lorsqu’un composant n’a besoin que d’une seule zone API.

```csharp
using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Query;

public sealed class ProjectCatalog(IProjectsClient projects)
{
    public async Task<IReadOnlyList<GitLabProject>> GetMembershipAsync(
        CancellationToken cancellationToken)
    {
        List<GitLabProject> result = [];

        await foreach (GitLabProject project in projects.ListAsync(
            new ProjectListOptions { Membership = true },
            cancellationToken))
        {
            result.Add(project);
        }

        return result;
    }
}
```

Les méthodes de lecture, création, mise à jour et suppression utilisent des DTO propres à leur route. Les listes retournent des séquences asynchrones. Les méthodes asynchrones acceptent un `CancellationToken` en dernier paramètre afin que l’annulation de votre application soit propagée au transport HTTP.

## Gérer les erreurs GitLab

Les réponses HTTP non réussies sont converties en exceptions spécialisées dérivées de `GitLabApiException`. Une application peut distinguer une erreur d’authentification, une absence de ressource, une validation, une limite de débit ou une erreur serveur.

```csharp
using GitLab.Client.Abstractions.Exceptions;

try
{
    await gitLab.Projects.GetAsync("group/project", cancellationToken);
}
catch (GitLabAuthenticationException)
{
    // Le jeton est absent, invalide ou expiré.
}
catch (GitLabNotFoundException)
{
    // Le projet est absent ou non visible pour ce jeton.
}
catch (GitLabRateLimitExceededException exception)
{
    // exception.RetryAfter contient l’information fournie par GitLab lorsqu’elle existe.
}
```

Les erreurs réseau restent des `HttpRequestException` et une annulation reste une `OperationCanceledException`. Cela préserve les conventions .NET habituelles.

## GraphQL et Work Items

`gitLab.GraphQL` donne accès à l’API GraphQL officielle sur `/api/graphql`. Les opérations GraphQL renvoient l’enveloppe complète, car GitLab peut répondre avec des données partielles et des erreurs GraphQL dans une réponse HTTP réussie.

La façade `gitLab.GraphQL.WorkItems` propose des opérations typées pour les Work Items. Cet exemple recherche un Work Item par espace de noms et IID.

```csharp
using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.GraphQL.WorkItems;

GitLabWorkItemLocator locator = new("group/subgroup", 42);

GitLabGraphQLResponse<GitLabWorkItemByLocatorQueryData> response =
    await gitLab.GraphQL.WorkItems.GetAsync(locator, cancellationToken);

GitLabWorkItem? workItem = response.Data?.Namespace?.WorkItem;

if (response.HasErrors)
{
    // Inspectez response.Errors tout en conservant les données partielles disponibles.
}
```

Pour un document GraphQL propre à votre application, `ExecuteAsync<TData>` et `ExecuteBatchAsync<TData>` demandent un `JsonTypeInfo<T>` produit par votre propre contexte `System.Text.Json` généré à la compilation. Le type de réponse est ainsi connu au compilateur, au trimmer et au compilateur Native AOT.

GitLab supporte le multiplexage GraphQL. `ExecuteBatchAsync<TData>` transmet plusieurs documents de même forme dans une seule requête HTTP et conserve l’ordre des enveloppes retournées. Ce mécanisme est distinct d’un batch REST et ne contourne ni les limites de complexité, ni les autorisations, ni les quotas GitLab.

## Batch asynchrone et parallélisme

`gitLab.Batches` orchestre des appels indépendants avec une concurrence maximale explicite. Ce mécanisme est local au client : GitLab REST v4 ne propose pas une route universelle qui combine arbitrairement plusieurs endpoints REST en une seule requête HTTP.

```csharp
using GitLab.Client.Batching;
using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.GraphQL.WorkItems;
using GitLab.Client.Models;

GitLabWorkItemLocator locator = new("group", 42);

GitLabBatchPlan plan = gitLab.Batches.Create(new GitLabBatchOptions
{
    MaxConcurrency = 4,
    FailureMode = GitLabBatchFailureMode.CollectAll
});

GitLabBatchOperation<GitLabProject> projectOperation =
    plan.Add(ct => gitLab.Projects.GetAsync("group/project", ct));

GitLabBatchOperation<GitLabGraphQLResponse<GitLabWorkItemByLocatorQueryData>> workItemOperation =
    plan.Add(ct => gitLab.GraphQL.WorkItems.GetAsync(locator, ct));

GitLabBatchExecution execution = await plan.ExecuteAsync(cancellationToken);

if (projectOperation.TryGetResult(execution, out GitLabProject? project) && project is not null)
{
    Console.WriteLine(project.WebUrl);
}
```

`CollectAll` conserve les résultats réussis et les erreurs individuelles. `FailFast` arrête la planification et annule coopérativement les opérations qui sont encore en cours après le premier échec. Un plan est différé et à usage unique : l’ajout d’une opération ne démarre pas de requête et une exécution ne rejoue jamais une mutation par accident.

La limite est appliquée par plan, non comme un rate limiter global. Le suivi des limites GitLab reste observateur afin que votre application garde le contrôle de sa stratégie de quota et de reprise.

## Mappers et compositions

`gitLab.Mappers` assemble localement des DTO issus de routes REST et GraphQL en compositions plus riches. Un mapper ne fait jamais de requête, ne charge jamais de données implicitement et ne modifie pas les DTO reçus. Les appels, la pagination, la mise en cache et le parallélisme restent donc visibles dans votre application.

```csharp
using GitLab.Client.Composition;
using GitLab.Client.Models;

GitLabProject project = await gitLab.Projects.GetAsync("group/project", cancellationToken);

GitLabProjectComposition composition = gitLab.Mappers.Projects.Map(
    new GitLabProjectCompositionInput { Project = project },
    GitLabProjectCompositionLoadPlan.ProjectOnly);
```

Les batches sont particulièrement utiles avant une composition : chargez les DTO indépendants en parallèle, inspectez les résultats disponibles, puis passez uniquement les données effectivement obtenues au mapper adapté.

## Native AOT, trimming et générateurs de source

La bibliothèque cible .NET 10 et C# 14. Elle est conçue pour être compatible avec Native AOT et le trimming.

1. Les DTO REST et GraphQL utilisent des contextes `System.Text.Json` générés à la compilation. La bibliothèque n’utilise pas la sérialisation JSON par réflexion.

2. Le binding de configuration de `AddGitLabClient(IConfiguration)` utilise le générateur de configuration .NET.

3. Les générateurs Roslyn internes produisent le câblage déterministe du client racine et de l’injection de dépendances, ainsi que la projection des options de requête vers les paramètres HTTP.

4. Aucun scan d’assembly, `dynamic`, génération de code à l’exécution ou découverte de services par réflexion n’est nécessaire au fonctionnement normal du SDK.

Le choix de `System.Text.Json` généré à la compilation améliore le démarrage, limite les allocations privées et facilite le trimming. Consultez la documentation Microsoft sur la [sérialisation par réflexion et la génération de source](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/reflection-vs-source-generation) pour le contexte général de cette approche.

## Transport HTTP

Le SDK s’appuie sur `IHttpClientFactory` et un client HTTP nommé. Il réutilise le pipeline standard de .NET pour l’authentification, les délais, la décompression, l’observation des en têtes de rate limit et les exceptions typées.

Le transport demande HTTP/2 lorsque l’instance le propose et peut revenir à HTTP/1.1 pour les installations qui ne le prennent pas en charge. Les nouvelles tentatives sont réservées aux méthodes HTTP sûres ; une mutation REST ou GraphQL n’est pas rejouée implicitement.

## Promesse de compatibilité

`GitLab.Client` promet une intégration .NET 10 moderne, des contrats C# explicites et une consommation directe des APIs REST v4 et GraphQL de GitLab 19.x et ultérieures.

Le SDK ne prétend pas rendre immuable le schéma GraphQL GitLab ni prendre en charge les instances antérieures à GitLab 19.0. Pour les zones GraphQL évolutives qui ne disposent pas encore d’une façade spécialisée, utilisez l’exécuteur générique avec un contrat de réponse et un contexte JSON généré dans votre application.

Les références officielles GitLab restent la source fonctionnelle pour les droits requis, les champs disponibles, les limites et les évolutions serveur : [REST API](https://docs.gitlab.com/api/rest/) et [GraphQL API](https://docs.gitlab.com/api/graphql/).

## Licence et sécurité

Le projet est distribué sous [licence MIT](LICENSE). Consultez [SECURITY.md](SECURITY.md) pour signaler une vulnérabilité.
