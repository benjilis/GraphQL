using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQLPoc.Models;

[Table("Genre")]
public class Genre
{
    [Key]
    public int GenreId { get; set; }
    
    public string? Name { get; set; }
    
    public List<Track> Tracks { get; set; } = new();
}
