using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectFour.Interfaces;

namespace ConnectFour.Strategy.BlockOpponentsLine
{
    public class DontGiveOpponentWinningMoveStrategy : IStrategy
    {
        public int MakeMove(GameState gameState, PlayerEnum whoAreYou)
        {
            var move = FindWinningMove(gameState, whoAreYou);

            if (move == -1)
            {
                move = FindBlockingMove(gameState, whoAreYou);
            }

            if (move == -1)
            {
                var safeMoves = FindSafeMoves(gameState, whoAreYou);

                if (safeMoves.Length > 0)
                {
                    move = FindRandomMove(safeMoves);
                }
                else
                {
                    move = FindRandomMove(gameState);
                }
            }

            return move;
        }

        private int FindRandomMove(int[] safeMoves)
        {
            var move = RandomGenerator.Generator().Next(0, safeMoves.Length);

            return safeMoves[move];
        }

        private int[] FindSafeMoves(GameState gameState, PlayerEnum whoAreYou)
        {
            var result = new List<int>();

            for (int col = 0; col < 7; col++)
            {
                if (gameState.FindFirstEmptyRow(col) != -1)
                {
                    if (CheckIfMoveIsSafe(gameState, col, whoAreYou))
                    {
                        result.Add(col);
                    }
                }
            }

            return result.ToArray();
        }

        private bool CheckIfMoveIsSafe(GameState gameState, int col, PlayerEnum whoAreYou)
        {
            var opponent = GetOpponent(whoAreYou);
            gameState.AddMove(col, whoAreYou);

            var winningMove = FindWinningMove(gameState, opponent);

            gameState.RemoveMove(col);

            if (winningMove == -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int FindBlockingMove(GameState gameState, PlayerEnum whoAreYou)
        {
            var opponent = GetOpponent(whoAreYou);

            foreach (var line in WinningLines.GetAllWinningLines())
            {
                var move = CanCompleteLine(line, gameState, opponent);

                if (move != -1)
                {
                    return move;
                }
            }

            return -1;
        }

        private PlayerEnum GetOpponent(PlayerEnum whoAreYou)
        {
            if (whoAreYou == PlayerEnum.PlayerOne)
            {
                return PlayerEnum.PlayerTwo;
            }

            return PlayerEnum.PlayerOne;
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
            var result = 0;

            foreach (var p in line)
            {
                if (gameState.GetPosition(p) == whoAreYou)
                {
                    result++;
                }
            }

            return result;
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
