using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphQLPoc.Models;

[ApiController]
[Route("api/sql")]
public class SqlController : ControllerBase
{
    private readonly ChinookContext _context;

    public SqlController(ChinookContext context) => _context = context;

    [HttpGet("simple")]
    public async Task<IActionResult> GetSimple()
    {
        var tableName = _context.Model.FindEntityType(typeof(Artist))?.GetTableName();

        var query = $"SELECT * FROM \"{tableName}\"";

        return Ok(await _context.Artists
            .FromSqlRaw(query)
            .AsNoTracking()
            .ToListAsync());
    }

    [HttpGet("complex")]
    public async Task<IActionResult> GetComplex()
    {
        var entityType = _context.Model.FindEntityType(typeof(Artist));
        var tableName = entityType.GetTableName();

        var query = $@"
        SELECT a.* FROM ""{tableName}"" a
        INNER JOIN ""Album"" al ON a.ArtistId = al.ArtistId
        INNER JOIN ""Track"" t ON al.AlbumId = t.AlbumId";

        return Ok(await _context.Artists
            .FromSqlRaw(query)
            .AsNoTracking()
            .Include(a => a.Albums)
                .ThenInclude(al => al.Tracks)
            .ToListAsync());
    }

    [HttpGet("filtered")]
    public async Task<IActionResult> GetFiltered()
    {
        var tableName = _context.Model.FindEntityType(typeof(Artist))?.GetTableName();
        var search = "A%";

        return Ok(await _context.Artists
            .FromSqlRaw($"SELECT * FROM \"{tableName}\" WHERE Name LIKE {{0}}", search)
            .AsNoTracking()
            .ToListAsync());
    }
}