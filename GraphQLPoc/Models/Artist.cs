using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQLPoc.Models;

[Table("Artist")]
public class Artist
{
    [Key]
    public int ArtistId { get; set; }
    
    public string? Name { get; set; }
    
    public List<Album> Albums { get; set; } = new();
}
