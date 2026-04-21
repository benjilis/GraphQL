using GraphQLPoc.GraphQL;
using GraphQLPoc.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Ajout des contrôleurs pour ton comparatif API
builder.Services.AddControllers();

builder.Services.AddDbContext<ChinookContext>(options =>
    options.UseSqlite("Data Source=./chinook.db")
           .LogTo(Console.WriteLine, LogLevel.Information));

builder.Services.AddHttpClient("BenchmarkClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5154");
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Cette option permet d'ignorer les cycles d'objets au lieu de planter
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Optionnel : rend le JSON plus lisible en conservant les noms de propriétés
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();



app.MapControllers(); // Pour ton ComparisonController
app.MapGraphQL();
app.Run();