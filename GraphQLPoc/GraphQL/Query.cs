using Microsoft.EntityFrameworkCore;
using GraphQLPoc.Models;

namespace GraphQLPoc.GraphQL;

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