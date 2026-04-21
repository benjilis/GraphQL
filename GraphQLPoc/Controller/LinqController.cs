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

    [HttpGet("simple")]
    public async Task<IActionResult> GetSimple()
    {
        return Ok(await _context.Artists.AsNoTracking().ToListAsync());
    }

    [HttpGet("complex")]
    public async Task<IActionResult> GetComplex()
    {
        return Ok(await _context.Artists
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Albums)
                .ThenInclude(al => al.Tracks)
            .ToListAsync());
    }

    [HttpGet("filtered")]
    public async Task<IActionResult> GetFiltered()
    {
        return Ok(await _context.Artists
            .AsNoTracking()
            .Where(a => a.Name != null && a.Name.StartsWith("A"))
            .ToListAsync());
    }
}