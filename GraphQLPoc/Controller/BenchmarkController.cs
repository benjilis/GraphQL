using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

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
        const int iterations = 10;

        // --- TEST 1 : SIMPLE ---
        var linqSimple = await MeasureAsync(iterations, () => client.GetAsync("/api/linq/simple"), "LINQ Simple");
        var sqlSimple = await MeasureAsync(iterations, () => client.GetAsync("/api/sql/simple"), "SQL Simple");
        var gqlSimple = await MeasureAsync(iterations, () => client.PostAsync("/graphql", GetPayload("{ artists { name } }")), "GQL Simple");

        // --- TEST 2 : COMPLEXE ---
        var linqComplex = await MeasureAsync(iterations, () => client.GetAsync("/api/linq/complex"), "LINQ Complex");
        var sqlComplex = await MeasureAsync(iterations, () => client.GetAsync("/api/sql/complex"), "SQL Complex");
        var gqlComplex = await MeasureAsync(iterations, () => client.PostAsync("/graphql", GetPayload("{ artists { name albums { title tracks { name } } } }")), "GQL Complex");

        // --- TEST 3 : FILTRÉ ---
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

    private async Task<double> MeasureAsync(int iterations, Func<Task<HttpResponseMessage>> action, string label)
    {
        try
        {
            var warmUp = await action();
            if (!warmUp.IsSuccessStatusCode)
            {
                var error = await warmUp.Content.ReadAsStringAsync();
                // Si ça plante ici, tu verras exactement lequel dans la console
                throw new Exception($"Status: {warmUp.StatusCode}, Body: {error}");
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var res = await action();
                res.EnsureSuccessStatusCode();
            }
            return Math.Round(sw.Elapsed.TotalMilliseconds / iterations, 2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BENCHMARK FAIL] {label} : {ex.Message}");
            throw;
        }
    }

    private StringContent GetPayload(string query) =>
        new StringContent(System.Text.Json.JsonSerializer.Serialize(new { query }), System.Text.Encoding.UTF8, "application/json");
}