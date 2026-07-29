using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Artist
    {
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string PictureSmall { get; set; }
        public string PictureMedium { get; set; }
        public string PictureBig { get; set; }
        public string PictureXl { get; set; }
        public Guid Id { get; private set; } = Guid.NewGuid();
        public List<Album> Albums { get; set; } = new List<Album>();
    }

}
