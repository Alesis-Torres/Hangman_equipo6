using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangmanServerLibrary.Model
{
    class GameMatchDTO
    {
        public int IdPlayerChallenger { get; internal set; }
        public int IdPlayerGuesser { get; internal set; }
        public int IdWord { get; internal set; }
    }
}
