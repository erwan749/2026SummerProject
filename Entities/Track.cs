using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Track
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public Album Album { get; set; }
        public int Duration {get; set; }
        public int TrackPosition { get; set; }
        public string Preview { get; set; }
        public List<BlindTest> BlindTests { get; set; } = new List<BlindTest>();
    }
}
