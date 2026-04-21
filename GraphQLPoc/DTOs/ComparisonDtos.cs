namespace GraphQLPoc.DTOs;

public class TrackDto
{
    public string? Name { get; set; }
    public string? Composer { get; set; }
    public decimal UnitPrice { get; set; }
}

public class ArtistDto
{
    public int ArtistId { get; set; }
    public string? Name { get; set; }
}

public class AlbumDto
{
    public string? Title { get; set; }
    public ArtistDto? Artist { get; set; }
    public List<TrackDto> Tracks { get; set; } = new();
}

public class ArtistWithAlbumsDto
{
    public string? Name { get; set; }
    public List<AlbumDto> Albums { get; set; } = new();
}

public class ComparisonResultDto
{
    public long ExecutionTimeMs { get; set; }
    public List<ArtistWithAlbumsDto> Data { get; set; } = new();
}
