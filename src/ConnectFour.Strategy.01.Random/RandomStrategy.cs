using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectFour.Interfaces;

namespace ConnectFour.Strategy.Random
{
    public class RandomStrategy : IStrategy
    {
        public int MakeMove(GameState gameState, PlayerEnum whoAreYou)
        {
            while (true)
            {
                var move = RandomGenerator.Generator().Next(0, 7);

                if (gameState.FindFirstEmptyRow(move) != -1)
                {
                    return move;
                }
            }
        }
    }
}
