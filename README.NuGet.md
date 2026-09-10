# GitLab.Client

SDK .NET typé pour l’API REST v4 et l’API GraphQL de GitLab. `GitLab.Client` cible GitLab 19.x et versions ultérieures, avec GitLab 19.0 comme version minimale prise en charge.

## Installation

```bash
dotnet add package GitLab.Client
```

Un seul package est nécessaire. Il contient la façade publique, les DTO, le route builder et le transport HTTP ; il n’existe pas de packages GitLab.Client complémentaires à installer.

## Prérequis

| Élément | Valeur |
| :--- | :--- |
| Runtime | .NET 10, `net10.0` |
| Langage | C# 14 |
| GitLab | 19.0 ou version ultérieure |
| APIs | REST v4 et GraphQL |
| Injection de dépendances | `Microsoft.Extensions.DependencyInjection` |

Un jeton GitLab est requis. Le SDK prend en charge les jetons personnels, OAuth Bearer et les jetons de job GitLab.

## Démarrage rapide

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
GitLabProject project = await gitLab.Projects.GetAsync("group/project");

Console.WriteLine(project.WebUrl);
```

Pour une instance auto gérée, configurez l’adresse REST avec son slash final.

```csharp
services.AddGitLabClient(options =>
{
    options.BaseAddress = new Uri("https://gitlab.example.com/api/v4/");
    options.AccessToken = Environment.GetEnvironmentVariable("GITLAB_TOKEN");
});
```

Une application peut aussi lier la section `GitLab` d’un `IConfiguration` existant.

```csharp
builder.Services.AddGitLabClient(builder.Configuration);
```

## Capacités principales

1. Clients REST directs, DTO et requêtes typés pour les domaines GitLab, disponibles via `IGitLabClient` ou par injection d’une interface précise comme `IProjectsClient`.

2. Pagination par `IAsyncEnumerable<T>` et annulation par `CancellationToken` sur les appels asynchrones.

3. Exceptions GitLab typées pour l’authentification, les autorisations, les ressources absentes, la validation, les conflits, les limites de débit et les erreurs serveur.

4. API GraphQL avec exécuteur de documents typés, multiplexage de documents de même forme et façade Work Items.

5. Batch client asynchrone à concurrence bornée pour réunir des appels REST et GraphQL indépendants sans simuler un endpoint REST batch inexistant.

6. Mappers purs pour composer localement des DTO déjà obtenus, sans I/O cachée.

## Native AOT et génération de source

La bibliothèque est conçue pour Native AOT et le trimming sur .NET 10. Les contrats JSON reposent sur `System.Text.Json` généré à la compilation, sans sérialisation JSON par réflexion. Le binding de configuration et le câblage DI utilisent également des générateurs de source ou du code déterministe, sans scan d’assembly ni `dynamic`.

Pour exécuter un document GraphQL personnalisé, fournissez le `JsonTypeInfo<T>` produit par le contexte `System.Text.Json` généré dans votre application. Le contrat reste ainsi visible au compilateur AOT.

## Documentation

Le README complet, les scénarios REST, GraphQL, batch et mappers sont disponibles dans le [dépôt GitHub](https://github.com/AriusII/GitlabClient).

Les références GitLab officielles sont disponibles pour [REST](https://docs.gitlab.com/api/rest/) et [GraphQL](https://docs.gitlab.com/api/graphql/).

## Licence

Distribué sous licence [MIT](https://github.com/AriusII/GitlabClient/blob/main/LICENSE).
