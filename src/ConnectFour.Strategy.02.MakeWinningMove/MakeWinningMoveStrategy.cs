using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectFour.Interfaces;

namespace ConnectFour.Strategy.MakeWinningMove
{
    public class MakeWinningMoveStrategy : IStrategy
    {
        public int MakeMove(GameState gameState, PlayerEnum whoAreYou)
        {
            var move = FindWinningMove(gameState, whoAreYou);

            if (move == -1)
            {
                move = FindRandomMove(gameState);
            }

            return move;
        }

        private int FindWinningMove(GameState gameState, PlayerEnum whoAreYou)
        {
            foreach (var line in WinningLines.GetAllWinningLines())
            {
                var move = CanCompleteLine(line, gameState, whoAreYou);
                if (move != -1)
                {
                    return move;
                }
            }

            return -1;
        }

        private int CanCompleteLine(WinningLine line, GameState gameState, PlayerEnum whoAreYou)
        {
            var count = CountPositionsInLine(line, gameState, whoAreYou);

            if (count < 3)
            {
                return -1;
            }

            var winningPosition = FindMissingPositionInLine(line, gameState);

            if (winningPosition.X != -1)
            {
                if (gameState.FindFirstEmptyRow(winningPosition.X) == winningPosition.Y)
                {
                    return winningPosition.X;
                }
            }

            return -1;
        }

        private Point FindMissingPositionInLine(WinningLine line, GameState gameState)
        {
            foreach (var p in line)
            {
                if (gameState.GetPosition(p) == PlayerEnum.Empty)
                {
                    return p;
                }
            }

            return new Point(-1, -1);
        }

        private int CountPositionsInLine(WinningLine line, GameState gameState, PlayerEnum whoAreYou)
        {
            return line.Count(p => gameState.GetPosition(p) == whoAreYou);
        }

        private int FindRandomMove(GameState gameState)
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
