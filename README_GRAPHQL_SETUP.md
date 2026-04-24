# 📊 GraphQL POC - Mise en Place et Configuration

## Table des matières
1. [Vue d'ensemble](#vue-densemble)
2. [Architecture du projet](#architecture-du-projet)
3. [Mise en place de GraphQL](#mise-en-place-de-graphql)
4. [Configuration détaillée](#configuration-détaillée)
5. [Utilisation](#utilisation)
6. [Performance et optimisations](#performance-et-optimisations)
7. [Conclusion](#conclusion)

---

## Vue d'ensemble

### Objectif du projet
Ce projet est une **Preuve de Concept (POC)** ayant pour but de démontrer les **avantages et performances de GraphQL** par rapport aux approches REST traditionnelles et SQL brut, en utilisant un environnement **.NET moderne**.

### Stack technique
- **Framework** : ASP.NET Core (avec .NET 10.0)
- **Moteur GraphQL** : [HotChocolate](https://chillicream.com/docs/hotchocolate) v15.1.14
- **ORM** : Entity Framework Core 10.0.6
- **Base de données** : SQLite (Chinook)
- **IDE GraphQL** : Banana Cake Pop (intégré)

### Avantages de GraphQL démontrés
| Aspect | REST | GraphQL |
|--------|------|---------|
| **Over-fetching** | ❌ Reçoit toutes les données | ✅ Reçoit uniquement ce qui est demandé |
| **Under-fetching** | ❌ Risque de manquer de données | ✅ Récupère tout en une seule requête |
| **Flexibilité** | ❌ Besoin d'un nouvel endpoint pour chaque besoin | ✅ Un seul endpoint pour tous les besoins |
| **Performance** | ⚠️ Dépend de l'implémentation | ✅ Optimisée au niveau de la base de données |
| **Documentation** | ❌ Manuel ou Swagger | ✅ Auto-documentée et explorable |

---

## Architecture du projet

### Structure des dossiers
```
GraphQLPoc/
├── Program.cs                    # Configuration et initialisation
├── appsettings.json             # Configuration de l'application
├── chinook.db                   # Base de données SQLite
├── chinook_dump.sql             # Dump SQL pour restaurer la BD
│
├── Models/                      # Entités EF Core
│   ├── ChinookContext.cs        # DbContext - Contexte de la BD
│   ├── Artist.cs                # Entité Artiste
│   ├── Album.cs                 # Entité Album
│   ├── Track.cs                 # Entité Piste
│   └── Genre.cs                 # Entité Genre
│
├── GraphQL/                     # Schéma et résolveurs GraphQL
│   └── Query.cs                 # Racine GraphQL (GET Artistes, Albums, etc.)
│
├── Controller/                  # Endpoints REST pour comparaison
│   ├── LinqController.cs        # Implémentation LINQ
│   ├── SqlController.cs         # Implémentation SQL brut
│   └── BenchmarkController.cs   # Tests de performance automatisés
│
└── DTOs/                        # Data Transfer Objects
    └── ComparisonDtos.cs        # DTOs pour les comparaisons
```

### Modèle de données (Chinook)
```
Artist (Artiste)
  ├─ ArtistId (PK)
  ├─ Name
  └─ Albums (collection)

Album
  ├─ AlbumId (PK)
  ├─ Title
  ├─ ArtistId (FK)
  └─ Tracks (collection)

Track (Piste)
  ├─ TrackId (PK)
  ├─ Name
  ├─ AlbumId (FK)
  ├─ GenreId (FK)
  └─ ...

Genre (Genre)
  ├─ GenreId (PK)
  ├─ Name
  └─ Tracks (collection)
```

---

## Mise en place de GraphQL

### 1. Installation des paquets NuGet

Les paquets GraphQL essentiels du projet :

```xml
<ItemGroup>
    <!-- Moteur GraphQL pour ASP.NET Core -->
    <PackageReference Include="HotChocolate.AspNetCore" Version="15.1.14" />
    
    <!-- Intégration Entity Framework pour les projections et filtres -->
    <PackageReference Include="HotChocolate.Data.EntityFramework" Version="15.1.14" />
    
    <!-- Support SQLite via EF Core -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.6" />
</ItemGroup>
```

### 2. Configuration dans Program.cs

Le cœur de la configuration GraphQL se fait dans [Program.cs](GraphQLPoc/Program.cs) :

```csharp
// Enregistrement du service GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()      // Enregistre la classe Query comme racine
    .AddProjections()           // Active les projections (sélection des colonnes)
    .AddFiltering()             // Active les filtres dynamiques (WHERE)
    .AddSorting();              // Active le tri côté serveur (ORDER BY)
```

#### Explication des modules activés :

| Module | Rôle | Bénéfice |
|--------|------|----------|
| **AddQueryType\<Query>()** | Définit les points d'entrée GraphQL | Expose les requêtes disponibles |
| **AddProjections()** | Analyse la requête GraphQL et ne sélectionne que les colonnes demandées | Génère `SELECT col1, col2` au lieu de `SELECT *` |
| **AddFiltering()** | Permet les conditions `where` dynamiques dans GraphQL | Traduction directe vers `WHERE SQL` |
| **AddSorting()** | Permet le tri (`order_by`) depuis GraphQL | Traduction directe vers `ORDER BY SQL` |

### 3. Classe Query - Définition du schéma

[GraphQL/Query.cs](GraphQLPoc/GraphQL/Query.cs) définit les points d'entrée GraphQL :

```csharp
public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Artist> GetArtists(ChinookContext context)
        => context.Artists.AsNoTracking().AsSplitQuery();

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Album> GetAlbums(ChinookContext context)
        => context.Albums.AsNoTracking().AsSplitQuery();

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Track> GetTracks(ChinookContext context)
        => context.Tracks.AsNoTracking().AsSplitQuery();

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Genre> GetGenres(ChinookContext context)
        => context.Genres.AsNoTracking().AsSplitQuery();
}
```

#### Décorateurs essentiels :

- **`[UseProjection]`** : ⭐ **Critique pour la performance**
  - HotChocolate analyse la requête GraphQL du client
  - Génère uniquement les colonnes demandées (pas de `SELECT *`)
  - Réduit la bande passante et la charge CPU

- **`[UseFiltering]`** : Filtrages dynamiques
  - Ex: `{ artists(where: { name: { startsWith: "The" } }) { name } }`
  - Traduit directement en `WHERE name LIKE 'The%'`

- **`[UseSorting]`** : Tri côté serveur
  - Ex: `{ artists(order_by: { name: ASC }) { name } }`
  - Évite de trier en mémoire

#### Optimisations EF Core appliquées :

```csharp
// AsNoTracking() : Mode lecture seule
// → EF Core n'entretient pas le cache d'identité
// → Moins d'allocation mémoire, requête plus rapide

// AsSplitQuery() : Stratégie pour les collections
// → Évite le "Cartesian Product" (explosion combinatoire)
// → Envoie plusieurs requêtes simples au lieu d'une complexe avec JOINs
// → Essentiel avec SQLite (n'optimise pas les jointures comme SQL Server)
```

### 4. Configuration de la base de données

```csharp
builder.Services.AddDbContext<ChinookContext>(options =>
    options.UseSqlite("Data Source=./chinook.db")
           .LogTo(Console.WriteLine, LogLevel.Information)); // Affiche les requêtes SQL
```

### 5. Mapping des routes

```csharp
app.MapControllers();  // Endpoints REST : /api/linq, /api/sql, /api/benchmark
app.MapGraphQL();      // Point d'entrée GraphQL : /graphql
```

---

## Configuration détaillée

### Database Context (EF Core)

[Models/ChinookContext.cs](GraphQLPoc/Models/ChinookContext.cs) configure les relations :

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Album → Tracks (1 à N)
    modelBuilder.Entity<Track>()
        .HasOne(t => t.Album)
        .WithMany(a => a.Tracks)
        .HasForeignKey(t => t.AlbumId);

    // Genre → Tracks (1 à N)
    modelBuilder.Entity<Track>()
        .HasOne(t => t.Genre)
        .WithMany(g => g.Tracks)
        .HasForeignKey(t => t.GenreId);
}
```

### Accès à GraphQL

L'endpoint GraphQL est accessible sur :
- **URL** : `http://localhost:5154/graphql`
- **IDE** : Banana Cake Pop (interfacce graphique intégrée)

---

## Utilisation

### Exemple 1 : Requête simple
```graphql
{
  artists {
    name
  }
}
```

**Résultat SQL généré** :
```sql
SELECT a."Name"
FROM "Artist" a
```

### Exemple 2 : Requête imbriquée (Artist → Album → Track)
```graphql
{
  artists(first: 5) {
    name
    albums {
      title
      tracks {
        name
      }
    }
  }
}
```

**Points importants** :
- GraphQL récupère exactement ce qui est demandé
- Sans `[UseProjection]`, EF aurait fait `SELECT * ...`
- `AsSplitQuery()` évite les JOINs complexes

### Exemple 3 : Filtrage et tri
```graphql
{
  artists(
    where: { name: { startsWith: "The" } }
    order_by: { name: ASC }
  ) {
    name
  }
}
```

**Résultat SQL généré** :
```sql
SELECT a."Name"
FROM "Artist" a
WHERE a."Name" LIKE 'The%'
ORDER BY a."Name" ASC
```

---

## Performance et optimisations

### Stratégies d'optimisation appliquées

#### 1. **Projections (UseProjection)**
```csharp
// ✅ Bon : GraphQL demande seulement 'name'
// SELECT "Name" FROM "Artist"

// ❌ Mauvais : Sans projection
// SELECT * FROM "Artist"
```

#### 2. **AsNoTracking()**
```csharp
// ✅ Optimisé : Pas de cache EF, requête plus légère
context.Artists.AsNoTracking()

// ❌ Plus lourd : EF maintient le cache
context.Artists
```

#### 3. **AsSplitQuery()**
```csharp
// ✅ SQLite : Deux requêtes simples
// SELECT * FROM Artist
// SELECT * FROM Album WHERE ArtistId IN (...)

// ❌ Mauvais avec SQLite : JOIN complexe et Cartesian Product
// SELECT * FROM Artist 
// LEFT JOIN Album ...
```

#### 4. **Filtres côté serveur**
```graphql
// ✅ Optimisé : Filtre dans la BD
{ artists(where: { name: { startsWith: "A" } }) { name } }

// ❌ Mauvais : Récupérer tout et filtrer en mémoire
{ artists { name } } // puis filtrer en C#
```

### Tests de performance

[Controller/BenchmarkController.cs](GraphQLPoc/Controller/BenchmarkController.cs) automatise les benchmarks :

```
GET /api/benchmark/run
```

Teste 3 scénarios sur 20 itérations :
1. **Simple** : Récupération simple d'artistes
2. **Complexe** : Arborescence Artiste → Album → Tracks
3. **Filtré** : Recherche avec WHERE

---

## Conclusion

### Résumé de la mise en place GraphQL

1. **Configuration minimale** via HotChocolate
2. **Optimisations intelligentes** avec les projections
3. **Intégration EF Core** pour les filtres dynamiques
4. **Performance** grâce aux stratégies SQLite (AsSplitQuery)

### Avantages démontrés
✅ **Moins de données** : Récupération précise des colonnes  
✅ **Flexibilité** : Un endpoint pour tous les besoins  
✅ **Performance** : Requêtes SQL optimisées automatiquement  
✅ **Maintenabilité** : Schéma auto-documenté  
✅ **Évolutivité** : Ajout de champs sans breaking change  

### Prochaines étapes possibles
- Ajout de mutations (CREATE, UPDATE, DELETE)
- Subscriptions GraphQL (real-time)
- Cache côté serveur
- Authentification et autorisation

---

**Créé avec ❤️ pour démontrer la puissance de GraphQL en .NET**
