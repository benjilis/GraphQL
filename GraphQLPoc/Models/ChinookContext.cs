using Microsoft.EntityFrameworkCore;

namespace GraphQLPoc.Models;

public class ChinookContext : DbContext
{
    public ChinookContext(DbContextOptions<ChinookContext> options) : base(options) { }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Album → Tracks
        modelBuilder.Entity<Track>()
            .HasOne(t => t.Album)
            .WithMany(a => a.Tracks)
            .HasForeignKey(t => t.AlbumId);

        // Genre → Tracks
        modelBuilder.Entity<Track>()
            .HasOne(t => t.Genre)
            .WithMany(g => g.Tracks)
            .HasForeignKey(t => t.GenreId);
    }
}
