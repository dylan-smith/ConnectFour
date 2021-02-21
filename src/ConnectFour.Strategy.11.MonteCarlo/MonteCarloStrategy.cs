using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ConnectFour.Interfaces;

namespace ConnectFour.Strategy.MonteCarlo
{
    public class MonteCarloStrategy : IStrategy
    {
        private int MAX_DEPTH = 0;
        private int MONTE_CARLO_ITERATIONS = 0;

        public MonteCarloStrategy(int depth, int iterations)
        {
            MAX_DEPTH = depth;
            MONTE_CARLO_ITERATIONS = iterations;
        }

        public MonteCarloStrategy()
        {
            MAX_DEPTH = 5;
            MONTE_CARLO_ITERATIONS = 100;
        }

        public int MakeMove(GameState state, PlayerEnum whoAreYou)
        {
            //File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] STARTING\n");
            var result = EvaluateState(state, whoAreYou, 0);
            //File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] MAKING MOVE {result.move} [{result.score}]\n");

            return result.move;
        }

        private (int score, int move) EvaluateState(GameState state, PlayerEnum whoAreYou, int depth)
        {
            if (depth == 42)
            {
                return (0, 0);
            }

            if (depth >= MAX_DEPTH)
            {
                return (MonteCarlo(state, whoAreYou, depth, MONTE_CARLO_ITERATIONS), -1);
            }

            var winningMove = FindWinningMove(state, whoAreYou);

            if (winningMove != -1)
            {
                return (int.MaxValue, winningMove);
            }

            var opponent = GetOpponent(whoAreYou);
            var forcedMove = FindBlockingMove(state, whoAreYou);

            if (forcedMove != -1)
            {
                var y = state.AddMove(forcedMove, whoAreYou);
                var result = EvaluateState(state, opponent, depth + 1);
                state.RemoveMove(forcedMove, y, whoAreYou);

                return (-result.score, forcedMove);
            }

            var safeMoves = FindSafeMoves(state, whoAreYou);

            if (safeMoves.Length == 0)
            {
                var validMove = FindValidMove(state);
                return (int.MinValue, validMove);
            }

            var doubleThreatMove = FindDoubleThreatMove(state, safeMoves, whoAreYou);

            if (doubleThreatMove != -1)
            {
                return (int.MaxValue, doubleThreatMove);
            }

            var bestScore = int.MaxValue;
            var bestMove = -1;

            foreach (var move in safeMoves)
            {
                var y = state.AddMove(move, whoAreYou);
                var result = EvaluateState(state, opponent, depth + 1);
                state.RemoveMove(move, y, whoAreYou);

                if (result.score <= bestScore)
                {
                    bestScore = result.score;
                    bestMove = move;
                }
            }

            return (-bestScore, bestMove);
        }

        private int MonteCarlo(GameState state, PlayerEnum whoAreYou, int depth, int iterations)
        {
            var result = 0;

            for (var i = 0; i < iterations; i++)
            {
                result += MonteCarlo(state, whoAreYou, depth);
            }

            return result;
        }

        private int MonteCarlo(GameState state, PlayerEnum whoAreYou, int depth)
        {
            if (depth == 42)
            {
                return 0;
            }

            var winningMove = FindWinningMove(state, whoAreYou);

            if (winningMove != -1)
            {
                return 1;
            }

            var opponent = GetOpponent(whoAreYou);
            var forcedMove = FindBlockingMove(state, whoAreYou);

            if (forcedMove != -1)
            {
                var y = state.AddMove(forcedMove, whoAreYou);
                var result = MonteCarlo(state, opponent, depth + 1);
                state.RemoveMove(forcedMove, y, whoAreYou);

                return -result;
            }

            var safeMoves = FindSafeMoves(state, whoAreYou);

            if (safeMoves.Length == 0)
            {
                return -1;
            }

            var doubleThreatMove = FindDoubleThreatMove(state, safeMoves, whoAreYou);

            if (doubleThreatMove != -1)
            {
                return 1;
            }

            var move = safeMoves[RandomGenerator.Generator().Next(0, safeMoves.Length)];

            var row = state.AddMove(move, whoAreYou);
            var score = MonteCarlo(state, opponent, depth + 1);
            state.RemoveMove(move, row, whoAreYou);

            return -score;
        }

        public int FindWinningMove(GameState state, PlayerEnum whoAreYou)
        {
            foreach (var line in state.GetAvailableLines(whoAreYou))
            {
                var move = state.CanCompleteLine(line, whoAreYou);

                if (move != -1)
                {
                    return move;
                }
            }

            return -1;
        }

        public int FindBlockingMove(GameState state, PlayerEnum whoAreYou)
        {
            var opponent = GetOpponent(whoAreYou);

            foreach (var line in state.GetAvailableLines(opponent))
            {
                var move = state.CanCompleteLine(line, opponent);

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

        public int[] FindSafeMoves(GameState gameState, PlayerEnum whoAreYou)
        {
            var result = new List<int>();

            for (int col = 0; col < 7; col++)
            {
                if (gameState.FindFirstEmptyRow(col) != 6)
                {
                    if (CheckIfMoveIsSafe(gameState, col, whoAreYou))
                    {
                        result.Add(col);
                    }
                }
            }

            return result.ToArray();
        }

        private int FindValidMove(GameState gameState)
        {
            var result = new List<int>();

            for (int col = 0; col < 7; col++)
            {
                if (gameState.FindFirstEmptyRow(col) != 6)
                {
                    return col;
                }
            }

            return -1;
        }

        private bool CheckIfMoveIsSafe(GameState state, int x, PlayerEnum whoAreYou)
        {
            var opponent = GetOpponent(whoAreYou);
            var y = state.AddMove(x, whoAreYou);

            var winningMove = FindWinningMove(state, opponent);

            state.RemoveMove(x, y, whoAreYou);

            return winningMove == -1;
        }

        public int FindDoubleThreatMove(GameState state, int[] safeMoves, PlayerEnum whoAreYou)
        {
            foreach (var m in safeMoves)
            {
                var y = state.AddMove(m, whoAreYou);
                var isDoubleThreat = DoesDoubleThreatExist(state, whoAreYou);
                state.RemoveMove(m, y, whoAreYou);

                if (isDoubleThreat)
                {
                    return m;
                }
            }

            return -1;
        }

        private bool DoesDoubleThreatExist(GameState state, PlayerEnum whoAreYou)
        {
            var firstThreat = -1;

            foreach (var line in state.GetAvailableLines(whoAreYou))
            {
                var move = state.CanCompleteLine(line, whoAreYou);

                if (move != -1 && firstThreat >= 0 && move != firstThreat)
                {
                    return true;
                }

                if (move != -1)
                {
                    firstThreat = move;
                }
            }

            return false;
        }

        private int CountChipsInLine(GameState state, WinningLine line, PlayerEnum player)
        {
            return line.Count(p => state.GetPosition(p) == player);
        }
    }
}
