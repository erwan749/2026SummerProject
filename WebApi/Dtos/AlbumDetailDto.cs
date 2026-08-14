using System.Collections.Generic;
namespace WebApi.Dtos
{
    public class AlbumDetailDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string ArtistId { get; set; }
        public string ArtistName { get; set; }
        public List<TrackSummaryDto> Tracks { get; set; }
    }
}
