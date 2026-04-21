using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphQLPoc.Models;

[ApiController]
[Route("api/sql")]
public class SqlController : ControllerBase
{
    private readonly ChinookContext _context;
    public SqlController(ChinookContext context) => _context = context;

    // 1. Simple : SQL Pur
    [HttpGet("simple")]
    public async Task<IActionResult> GetSimple()
    {
        // On demande à EF le nom exact de la table mappée pour l'entité Artist
        var tableName = _context.Model.FindEntityType(typeof(Artist))?.GetTableName();

        // On construit la requête avec le nom récupéré (souvent "Artist" ou "artists")
        var query = $"SELECT * FROM \"{tableName}\"";

        return Ok(await _context.Artists
            .FromSqlRaw(query)
            .AsNoTracking()
            .ToListAsync());
    }

    // 2. Complexe : SQL avec Jointures
    [HttpGet("complex")]
    public async Task<IActionResult> GetComplex()
    {
        // On demande à EF Core le nom de la table tel qu'il est mappé en base
        var entityType = _context.Model.FindEntityType(typeof(Artist));
        var tableName = entityType.GetTableName(); // Récupère "Artist" ou "artists" ou "Artists"

        // On construit la requête SQL avec le bon nom d'identifiant
        var query = $@"
        SELECT a.* FROM ""{tableName}"" a
        INNER JOIN ""Album"" al ON a.ArtistId = al.ArtistId
        INNER JOIN ""Track"" t ON al.AlbumId = t.AlbumId";

        // Note: Pour les jointures Album et Track, SQLite Chinook utilise souvent le singulier.
        // Si ça replante sur "Album", remplace par "albums".

        return Ok(await _context.Artists
            .FromSqlRaw(query)
            .AsNoTracking()
            .Include(a => a.Albums)
                .ThenInclude(al => al.Tracks)
            .ToListAsync());
    }

    // 3. Filtré : SQL avec clause WHERE et paramètre
    [HttpGet("filtered")]
    public async Task<IActionResult> GetFiltered()
    {
        // On récupère le nom de table configuré dans EF (évite les erreurs de typo)
        var tableName = _context.Model.FindEntityType(typeof(Artist))?.GetTableName();
        var search = "A%";

        // Utilisation du nom dynamique avec guillemets
        return Ok(await _context.Artists
            .FromSqlRaw($"SELECT * FROM \"{tableName}\" WHERE Name LIKE {{0}}", search)
            .AsNoTracking()
            .ToListAsync());
    }
}