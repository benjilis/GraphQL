using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

[ApiController]
[Route("api/benchmark")]
public class BenchmarkController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;

    public BenchmarkController(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

    [HttpGet("run")]
    public async Task<IActionResult> RunFullTest()
    {
        var client = _clientFactory.CreateClient("BenchmarkClient");
        const int iterations = 20;

        // --- SCÉNARIO 1 : TEST SIMPLE ---
        // On récupère uniquement la liste des artistes
        var linqSimple = await MeasureAsync(iterations, () => client.GetAsync("/api/linq/simple"), "LINQ Simple");
        var sqlSimple = await MeasureAsync(iterations, () => client.GetAsync("/api/sql/simple"), "SQL Simple");
        var gqlSimple = await MeasureAsync(iterations, () => client.PostAsync("/graphql", GetPayload("{ artists { name } }")), "GQL Simple");

        // --- SCÉNARIO 2 : TEST COMPLEXE (ARBORESCENCE) ---
        // On récupère Artistes -> Albums -> Tracks (Jointures multiples)
        var linqComplex = await MeasureAsync(iterations, () => client.GetAsync("/api/linq/complex"), "LINQ Complex");
        var sqlComplex = await MeasureAsync(iterations, () => client.GetAsync("/api/sql/complex"), "SQL Complex");
        // En GraphQL, on définit précisément l'arborescence voulue dans le payload
        var gqlComplex = await MeasureAsync(iterations, () => client.PostAsync("/graphql", GetPayload("{ artists { name albums { title tracks { name } } } }")), "GQL Complex");

        // --- SCÉNARIO 3 : TEST FILTRÉ (WHERE) ---
        // Simulation d'une recherche dynamique (les artistes commençant par 'A')
        var linqFiltered = await MeasureAsync(iterations, () => client.GetAsync("/api/linq/filtered"), "LINQ Filtered");
        var sqlFiltered = await MeasureAsync(iterations, () => client.GetAsync("/api/sql/filtered"), "SQL Filtered");
        var gqlFiltered = await MeasureAsync(iterations, () => client.PostAsync("/graphql", GetPayload("{ artists(where: { name: { startsWith: \"A\" } }) { name } }")), "GQL Filtered");

        return Ok(new
        {
            Iterations = iterations,
            Results = new[] {
                new { Scenario = "Simple", Linq = linqSimple, Sql = sqlSimple, Gql = gqlSimple },
                new { Scenario = "Complexe", Linq = linqComplex, Sql = sqlComplex, Gql = gqlComplex },
                new { Scenario = "Filtré", Linq = linqFiltered, Sql = sqlFiltered, Gql = gqlFiltered }
            }
        });
    }

    /// <summary>
    /// Méthode de mesure de performance
    /// </summary>
    private async Task<double> MeasureAsync(int iterations, Func<Task<HttpResponseMessage>> action, string label)
    {
        try
        {
            //PHASE DE WARM-UP (Préchauffage) : 
            // On exécute la requête une première fois pour réveiller le moteur EF et la DB.
            // Cela évite de comptabiliser le temps de compilation JIT ou d'ouverture de connexion.
            var warmUp = await action();
            if (!warmUp.IsSuccessStatusCode)
            {
                var error = await warmUp.Content.ReadAsStringAsync();
                throw new Exception($"Status: {warmUp.StatusCode}, Body: {error}");
            }

            //PHASE DE MESURE : Chronométrage de la boucle d'itérations
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var res = await action();
                res.EnsureSuccessStatusCode();
            }
            sw.Stop();

            //On retourne la moyenne par requête en millisecondes
            return Math.Round(sw.Elapsed.TotalMilliseconds / iterations, 2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BENCHMARK FAIL] {label} : {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Formate la requête GraphQL au format JSON attendu par le serveur
    /// </summary>
    private StringContent GetPayload(string query) =>
        new StringContent(System.Text.Json.JsonSerializer.Serialize(new { query }), System.Text.Encoding.UTF8, "application/json");
}