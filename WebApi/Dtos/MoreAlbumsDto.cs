using System.Collections.Generic;
namespace WebApi.Dtos
{
    public class MoreAlbumsDto
    {
        public List<AlbumSummaryDto> Albums { get; set; }
        public bool HasMoreAlbums { get; set; }
    }
}