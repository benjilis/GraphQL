using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphQLPoc.Models;
using GraphQLPoc.DTOs;

[ApiController]
[Route("api/linq")]
public class LinqController : ControllerBase
{
    private readonly ChinookContext _context;
    public LinqController(ChinookContext context) => _context = context;

    // 1. Simple : Liste des artistes
    [HttpGet("simple")]
    public async Task<IActionResult> GetSimple()
    {
        return Ok(await _context.Artists.AsNoTracking().ToListAsync());
    }

    // 2. Complexe : Arborescence complète
    [HttpGet("complex")]
    public async Task<IActionResult> GetComplex()
    {
        // Utilisation de AsSplitQuery pour éviter l'erreur APPLY sur SQLite
        return Ok(await _context.Artists
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Albums)
                .ThenInclude(al => al.Tracks)
            .ToListAsync());
    }

    // 3. Filtré : StartsWith "A"
    [HttpGet("filtered")]
    public async Task<IActionResult> GetFiltered()
    {
        return Ok(await _context.Artists
            .AsNoTracking()
            .Where(a => a.Name != null && a.Name.StartsWith("A"))
            .ToListAsync());
    }
}