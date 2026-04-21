using Microsoft.EntityFrameworkCore;
using GraphQLPoc.Models;

namespace GraphQLPoc.GraphQL;

public class Query
{
    // [UseProjection] : Indispensable pour la performance. 
    // Il permet à HotChocolate d'analyser la requête GraphQL du client et de ne générer 
    // un "SELECT" SQL que pour les colonnes demandées (évite de récupérer toute la table).
    
    // [UseFiltering] : Active la puissance des filtres dynamiques (ex: where: { name: { startsWith: "A" } }).
    
    // [UseSorting] : Permet au client de trier les données directement depuis la requête (ex: order_by: { name: ASC }).

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Artist> GetArtists(ChinookContext context)
        // AsNoTracking() : Optimise la lecture en ne gardant pas les objets en mémoire cache (mode lecture seule).
        // AsSplitQuery() : Stratégie SQLite pour éviter l'explosion combinatoire des jointures complexes 
        // en séparant les requêtes de collections.
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