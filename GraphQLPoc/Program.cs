using GraphQLPoc.GraphQL;
using GraphQLPoc.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configuration de la base de données SQLite
builder.Services.AddDbContext<ChinookContext>(options =>
    options.UseSqlite("Data Source=./chinook.db")
           // LogTo permet de voir les requêtes SQL générées par EF Core
           .LogTo(Console.WriteLine, LogLevel.Information));

builder.Services.AddHttpClient("BenchmarkClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5154");
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configuration de HotChocolate (Moteur GraphQL)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()    // Enregistre la classe contenant les requêtes GraphQL
    .AddProjections()        // Permet à GraphQL d'extraire uniquement les colonnes demandées (évite le SELECT *)
    .AddFiltering()          // Active les filtres dynamiques
    .AddSorting();           

var app = builder.Build();

// Mappe les routes des contrôleurs REST (api/linq, api/sql, api/benchmark)
app.MapControllers();

// Mappe le point d'entrée unique GraphQL (/graphql) et l'IDE Banana Cake Pop
app.MapGraphQL();

app.Run();