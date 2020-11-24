using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFour.Interfaces
{
    public interface IStrategy
    {
        int MakeMove(GameState gameState, PlayerEnum whoAreYou);
    }
}
