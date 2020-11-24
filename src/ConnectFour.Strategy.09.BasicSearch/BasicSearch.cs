using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ConnectFour.Interfaces;

namespace ConnectFour.Strategy.BasicSearch
{
    public class BasicSearchStrategy : IStrategy
    {
        private long _stateCount = 0;

        public int MakeMove(GameState gameState, PlayerEnum whoAreYou)
        {
            var move = FindWinningMove(gameState, whoAreYou);

            if (move != -1)
            {
                return move;
            }

            move = FindBlockingMove(gameState, whoAreYou);

            if (move != -1)
            {
                return move;
            }

            var safeMoves = FindSafeMoves(gameState, whoAreYou);

            if (safeMoves.Length == 0)
            {
                return FindValidMoves(gameState).First();
            }

            move = FindDoubleThreatMoves(gameState, safeMoves, whoAreYou);

            if (move != -1)
            {
                return move;
            }

            var drawMove = -1;

            foreach (var m in safeMoves)
            {
                gameState.AddMove(m, whoAreYou);
                var winner = EvaluateState(gameState, GetOpponent(whoAreYou));
                gameState.RemoveMove(m);

                if (winner == whoAreYou)
                {
                    return m;
                }

                if (winner == PlayerEnum.Stalemate)
                {
                    drawMove = m;
                }
            }

            if (drawMove != -1)
            {
                return drawMove;
            }

            return safeMoves.First();
        }

        private PlayerEnum EvaluateState(GameState gameState, PlayerEnum whoAreYou)
        {
            _stateCount++;

            if (_stateCount % 1000000 == 0)
            {
                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] {_stateCount}\n");
            }

            if (FindWinningMove(gameState, whoAreYou) != -1)
            {
                return whoAreYou;
            }

            var forcedMove = FindBlockingMove(gameState, whoAreYou);

            if (forcedMove != -1)
            {
                gameState.AddMove(forcedMove, whoAreYou);
                var result = EvaluateState(gameState, GetOpponent(whoAreYou));
                gameState.RemoveMove(forcedMove);
                
                return result;
            }

            var safeMoves = FindSafeMoves(gameState, whoAreYou);

            if (safeMoves.Length == 0)
            {
                return GetOpponent(whoAreYou);
            }

            if (FindDoubleThreatMoves(gameState, safeMoves, whoAreYou) != -1)
            {
                return whoAreYou;
            }

            var canDraw = false;

            foreach (var move in safeMoves)
            {
                gameState.AddMove(move, whoAreYou);
                var winner = EvaluateState(gameState, GetOpponent(whoAreYou));
                gameState.RemoveMove(move);

                if (winner == whoAreYou)
                {
                    return whoAreYou;
                }

                if (winner == PlayerEnum.Stalemate)
                {
                    canDraw = true;
                }
            }

            if (canDraw)
            {
                return PlayerEnum.Stalemate;
            }

            return GetOpponent(whoAreYou);
        }

        // TODO: while the opponent only has one move to make the double-threat, there are a few choices of moves that would block it
        // TODO: this only detects double-threats moves before my move, need to make sure not to make a move that creates a new double-threat opportunity (constrain safe moves)
        private int BlockOpponentDoubleThreatMove(GameState gameState, int[] safeMoves, PlayerEnum whoAreYou)
        {
            var opponent = GetOpponent(whoAreYou);
            var oppSafeMoves = FindSafeMoves(gameState, opponent);

            var move = FindDoubleThreatMoves(gameState, oppSafeMoves, opponent);

            if (move >= 0 && safeMoves.Contains(move))
            {
                return move;
            }

            return -1;
        }

        private int FindDoubleThreatMoves(GameState gameState, int[] safeMoves, PlayerEnum whoAreYou)
        {
            foreach (var m in safeMoves)
            {
                var y = gameState.AddMove(m, whoAreYou);
                var isDoubleThreat = DoesDoubleThreatExist(gameState, whoAreYou);
                gameState.UpdateWithMove(m, y, PlayerEnum.Empty);

                if (isDoubleThreat)
                {
                    return m;
                }
            }

            return -1;
        }

        private bool DoesDoubleThreatExist(GameState gameState, PlayerEnum whoAreYou)
        {
            var threats = FindThreatCount(gameState, whoAreYou);

            return threats > 1;
        }

        private int FindThreatCount(GameState gameState, PlayerEnum whoAreYou)
        {
            var threatCount = 0;

            foreach (var l in WinningLines.GetAllWinningLines())
            {
                if (LineIsAvailable(l, gameState, whoAreYou))
                {
                    if (CountPositionsInLine(l, gameState, whoAreYou) == 3)
                    {
                        var emptyPos = FindEmptyPositionInLine(l, gameState);

                        if (gameState.FindFirstEmptyRow(emptyPos.X) == emptyPos.Y)
                        {
                            threatCount++;
                        }
                    }
                }
            }

            return threatCount;
        }

        private Point FindEmptyPositionInLine(WinningLine line, GameState gameState)
        {
            foreach (var p in line)
            {
                if (gameState.GetPosition(p) == PlayerEnum.Empty)
                {
                    return p;
                }
            }

            throw new ArgumentException("Line didn't contain any empty positions");
        }

        private int FindMoveWithMostLineChips(GameState gameState, int[] safeMoves, PlayerEnum whoAreYou)
        {
            var maxLineChipCount = int.MinValue;
            var result = 0;

            foreach (var m in safeMoves)
            {
                var y = gameState.FindFirstEmptyRow(m);
                var lines = WinningLines.GetLinesByPoint(m, y);

                var lineChipCount = CountLineChips(gameState, lines, whoAreYou);

                if (lineChipCount > maxLineChipCount)
                {
                    result = m;
                    maxLineChipCount = lineChipCount;
                }
            }

            return result;
        }

        private int CountLineChips(GameState gameState, IEnumerable<WinningLine> lines, PlayerEnum whoAreYou)
        {
            return lines.Sum(line => CountChipsInLine(gameState, whoAreYou, line));
        }

        private int CountChipsInLine(GameState gameState, PlayerEnum whoAreYou, WinningLine line)
        {
            var result = 1;

            if (!LineIsAvailable(line, gameState, whoAreYou))
            {
                return 0;
            }

            foreach (var p in line)
            {
                if (gameState.GetPosition(p) == whoAreYou)
                {
                    result++;
                }
            }

            return result;
        }

        private bool LineIsAvailable(WinningLine line, GameState gameState, PlayerEnum whoAreYou)
        {
            foreach (var p in line)
            {
                if (gameState.GetPosition(p) != whoAreYou && gameState.GetPosition(p) != PlayerEnum.Empty)
                {
                    return false;
                }
            }

            return true;
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

        private int[] FindValidMoves(GameState gameState)
        {
            var result = new List<int>();

            for (int col = 0; col < 7; col++)
            {
                if (gameState.FindFirstEmptyRow(col) != -1)
                {
                    result.Add(col);
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
    }
}
