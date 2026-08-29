using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class GameSessionQuestion
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GameSessionId { get; set; }
        public string TrackId { get; set; }
        public Track Track { get; set; }
        public string QuestionType { get; set; }
        public int Order { get; set; }
    }
}
