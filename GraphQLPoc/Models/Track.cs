using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQLPoc.Models;

[Table("Track")]
public class Track
{
    [Key]
    public int TrackId { get; set; }
    
    public string? Name { get; set; }
    
    public int AlbumId { get; set; }
    
    public Album Album { get; set; } = default!;
    
    public string? Composer { get; set; }
    
    public int Milliseconds { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public int GenreId { get; set; }
    
    public Genre Genre { get; set; } = default!;
}
