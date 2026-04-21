# 🚀 POC : GraphQL vs REST vs SQL Brut

## 1. Comprendre GraphQL

### Qu'est-ce que GraphQL ?
GraphQL est un langage de requête pour les API, ainsi qu'un environnement d'exécution pour répondre à ces requêtes. Contrairement à une API traditionnelle qui renvoie des données fixes, GraphQL permet au client de demander **exactement** ce dont il a besoin.

### GraphQL vs REST : La différence fondamentale

| Caractéristique | REST (Representational State Transfer) | GraphQL |
| :--- | :--- | :--- |
| **Endpoints** | Multiples (ex: `/artists`, `/albums`) | Unique (ex: `/graphql`) |
| **Gestion des données** | **Over-fetching** (reçoit trop de données) ou **Under-fetching** (pas assez) | Récupère uniquement les données voulues |
| **Structure** | Définie par le serveur | Définie par le client |
| **Flexibilité** | Rigide (nécessite un nouvel endpoint pour chaque besoin) | Agile (schéma auto-documenté et évolutif) |

---

## 2. À propos du Projet
Ce projet est une **Preuve de Concept (POC)**. L'objectif est de démontrer par les chiffres l'impact du passage à GraphQL dans un environnement .NET, en utilisant une base de données réelle (**Chinook**) sous SQLite.

---

## 3. Stack Technique & Bibliothèques
Pour réaliser ce comparatif, les bibliothèques suivantes ont été utilisées :

* **HotChocolate** (`HotChocolate.AspNetCore`) : Le moteur GraphQL pour .NET.
* **Entity Framework Core** (`Sqlite`) : L'ORM utilisé pour le mapping des données.
* **Microsoft.Extensions.Http** : Pour simuler des appels clients réels lors du benchmark.

---

## 4. Architecture du Projet
Le projet est organisé pour séparer clairement les approches et permettre une comparaison honnête :

* **`LinqController.cs`** : Implémentation REST utilisant les méthodes LINQ classiques.
* **`SqlController.cs`** : Implémentation REST utilisant des requêtes SQL brutes (`FromSqlRaw`).
* **`Query.cs` (GraphQL)** : Le point d'entrée unique exposant le schéma GraphQL.
* **`BenchmarkController.cs`** : Contrôleur de test automatisé calculant les moyennes de performance.
* **`Models/`** : Entités (Artist, Album, Track) mappées depuis la base de données.
* **`chinook.db`** : Base de données SQLite (Catalogue musical).

---

## 5. Méthodologie du Test
Nous testons trois scénarios représentatifs d'une application réelle :

1.  **Test Simple** : Récupération d'une liste plate d'artistes.
2.  **Test Complexe (Arborescence)** : Chargement des Artistes avec tous leurs Albums et toutes leurs Pistes (Tracks).
3.  **Test Filtré (Where)** : Recherche dynamique des artistes commençant par la lettre "A".

Chaque test est lancé **20 fois** consécutivement via un `HttpClient`. Le benchmark calcule la moyenne du temps de réponse (en millisecondes) incluant : le traitement serveur, l'accès base de données, la sérialisation JSON et le transit réseau local.

---

## 📊 6. Résultats des Benchmarks (Mesures Réelles)

| Scénario | LINQ (REST) | SQL / EF (REST) | GraphQL | Gagnant |
| :--- | :--- | :--- | :--- | :--- |
| **Simple** (Liste plate) | ~28 ms | ~19 ms | **~13 ms** | **GraphQL** |
| **Filtré** (Clause WHERE) | **~9.22 ms** | ~9.39 ms | ~10 ms | **LINQ** |
| **Complexe** (Relations) | ~126 ms | ~2415 ms | **~70 ms** | **GraphQL** |

---

## 🔍 7. Analyse des résultats

### 1. Efficacité sur les requêtes simples
Contrairement aux idées reçues, **GraphQL gagne sur la requête simple (~13ms)**. Cela s'explique par la légèreté du payload JSON : GraphQL ne retourne que le champ demandé (`name`), réduisant le temps de sérialisation par rapport à REST qui renvoie tous les champs de la table.

### 2. Performance du moteur LINQ sur le filtrage
Sur le filtrage simple (clause WHERE), **LINQ l'emporte de très peu (~9.22ms)**. L'optimisation du moteur de requêtes d'Entity Framework est ici extrêmement mature, dépassant d'un cheveu GraphQL qui doit parser la requête entrante.

### 3. Le gouffre de performance du SQL Brut sur les relations
C'est le résultat le plus frappant : **SQL Brut s'effondre avec ~2415ms** contre seulement **~70ms pour GraphQL** sur le test complexe.
* **Problème SQL/REST** : La jointure massive génère un volume de données énorme. Transformer ce résultat plat en JSON imbriqué est extrêmement coûteux en CPU.
* **Victoire GraphQL** : Grâce aux projections, GraphQL optimise la requête en ne récupérant que les branches nécessaires. Le gain de performance est ici de l'ordre de **34x**.

---

## 🎯 8. Conclusion du POC
Les tests démontrent que :
1. **GraphQL n'est pas "plus lent"** : Il peut surpasser REST même sur des tâches simples grâce à la réduction de la taille des données.
2. **Le SQL Brut est risqué en REST** : Sans une logique complexe de sélection de champs (DTO), le SQL traditionnel expose l'application à des chutes de performances critiques.
3. **Optimisation native** : GraphQL offre nativement les meilleures performances sur les modèles de données complexes tout en offrant une flexibilité totale au client.

---

## 🚀 Lancement
1. `dotnet build`
2. `dotnet run`
3. Exécuter le benchmark : `GET http://localhost:<port>/api/benchmark/run`