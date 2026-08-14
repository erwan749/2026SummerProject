using System.Collections.Generic;
namespace WebApi.Dtos
{
    public class ArtistDetailDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public List<AlbumSummaryDto> Albums {  get; set; }
        public bool HasMoreAlbums { get; set; }
    }
}
