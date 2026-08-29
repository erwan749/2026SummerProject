using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class GameSession
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid BlindTestId { get; set; }
        public int CurrentPosition { get;  set; } = 0;
        public List<GameSessionQuestion> Questions { get; private set; } = new List<GameSessionQuestion>();
    }
}
