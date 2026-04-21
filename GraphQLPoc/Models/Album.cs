using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQLPoc.Models;

[Table("Album")]
public class Album
{
    [Key]
    public int AlbumId { get; set; }
    
    public string? Title { get; set; }
    
    public int ArtistId { get; set; }
    
    public Artist Artist { get; set; } = default!;
    
    public List<Track> Tracks { get; set; } = new();
}
