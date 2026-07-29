using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Album
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Cover { get; set; }
        public Artist Artist { get; set; }
        public List<Track> Tracks { get; set; } = new List<Track>();
}
}
