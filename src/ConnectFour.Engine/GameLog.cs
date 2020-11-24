using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectFour.Interfaces;

namespace ConnectFour.Engine
{
    public class GameLog
    {
        private readonly IList<int> _moves;

        public GameLog(PlayerEnum whoGoesFirst)
        {
            WhoGoesFirst = whoGoesFirst;
            _moves = new List<int>();
            Winner = PlayerEnum.GameNotDone;
        }
        
        public PlayerEnum WhoGoesFirst { get; private set; }

        public PlayerEnum Winner { get; set; }

        public IList<int> Moves
        {
            get
            {
                return _moves;
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            if (Winner == PlayerEnum.PlayerOne)
            {
                result.Append("Player One");
            }

            if (Winner == PlayerEnum.PlayerTwo)
            {
                result.Append("Player Two");
            }

            if (Winner == PlayerEnum.Stalemate)
            {
                result.Append("Stalemate");
            }

            result.Append(" - ");
            result.Append(_moves.Count.ToString());

            return result.ToString();
        }
    }
}
