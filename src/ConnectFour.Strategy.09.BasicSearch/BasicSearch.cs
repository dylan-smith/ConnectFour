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
        private Dictionary<PlayerEnum, List<WinningLine>> _availableLines;

        public int MakeMove(GameState gameState, PlayerEnum whoAreYou)
        {
            _availableLines = new Dictionary<PlayerEnum, List<WinningLine>>();
            _availableLines.Add(PlayerEnum.PlayerOne, new List<WinningLine>());
            _availableLines.Add(PlayerEnum.PlayerTwo, new List<WinningLine>());

            foreach (var line in WinningLines.GetAllWinningLines())
            {
                if (LineIsAvailable(line, gameState, PlayerEnum.PlayerOne))
                {
                    _availableLines[PlayerEnum.PlayerOne].Add(line);
                }

                if (LineIsAvailable(line, gameState, PlayerEnum.PlayerTwo))
                {
                    _availableLines[PlayerEnum.PlayerTwo].Add(line);
                }
            }

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
                AddMove(gameState, m, whoAreYou);

                var winner = EvaluateState(gameState, GetOpponent(whoAreYou));
                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] MOVE DONE =======================================================================\n");
                RemoveMove(gameState, m);

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

        private void RemoveMove(GameState gameState, int x)
        {
            var (y, player) = gameState.RemoveMove(x);
            var lines = WinningLines.GetLinesByPoint(x, y);

            // TODO: if we had a count of each players chips in each line this could be quicker

            foreach (var line in lines)
            {
                if (LineIsAvailable(line, gameState, GetOpponent(player)))
                {
                    _availableLines[GetOpponent(player)].Add(line);
                }
            }
        }

        private int AddMove(GameState gameState, int x, PlayerEnum whoAreYou)
        {
            var y = gameState.AddMove(x, whoAreYou);
            var lines = WinningLines.GetLinesByPoint(x, y);

            foreach (var line in lines)
            {
                _availableLines[GetOpponent(whoAreYou)].Remove(line);
            }

            return y;
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
                AddMove(gameState, forcedMove, whoAreYou);
                var result = EvaluateState(gameState, GetOpponent(whoAreYou));
                RemoveMove(gameState, forcedMove);

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
                AddMove(gameState, move, whoAreYou);
                var winner = EvaluateState(gameState, GetOpponent(whoAreYou));
                RemoveMove(gameState, move);

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
        //private int BlockOpponentDoubleThreatMove(GameState gameState, int[] safeMoves, PlayerEnum whoAreYou)
        //{
        //    var opponent = GetOpponent(whoAreYou);
        //    var oppSafeMoves = FindSafeMoves(gameState, opponent);

        //    var move = FindDoubleThreatMoves(gameState, oppSafeMoves, opponent);

        //    if (move >= 0 && safeMoves.Contains(move))
        //    {
        //        return move;
        //    }

        //    return -1;
        //}

        private int FindDoubleThreatMoves(GameState gameState, int[] safeMoves, PlayerEnum whoAreYou)
        {
            foreach (var m in safeMoves)
            {
                var y = AddMove(gameState, m, whoAreYou);
                var isDoubleThreat = DoesDoubleThreatExist(gameState, whoAreYou);
                RemoveMove(gameState, m);

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

            foreach (var l in _availableLines[whoAreYou])
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

        //private int FindMoveWithMostLineChips(GameState gameState, int[] safeMoves, PlayerEnum whoAreYou)
        //{
        //    var maxLineChipCount = int.MinValue;
        //    var result = 0;

        //    foreach (var m in safeMoves)
        //    {
        //        var y = gameState.FindFirstEmptyRow(m);
        //        var lines = WinningLines.GetLinesByPoint(m, y);

        //        var lineChipCount = CountLineChips(gameState, lines, whoAreYou);

        //        if (lineChipCount > maxLineChipCount)
        //        {
        //            result = m;
        //            maxLineChipCount = lineChipCount;
        //        }
        //    }

        //    return result;
        //}

        //private int CountLineChips(GameState gameState, IEnumerable<WinningLine> lines, PlayerEnum whoAreYou)
        //{
        //    return lines.Sum(line => CountChipsInLine(gameState, whoAreYou, line));
        //}

        //private int CountChipsInLine(GameState gameState, PlayerEnum whoAreYou, WinningLine line)
        //{
        //    var result = 1;

        //    if (!LineIsAvailable(line, gameState, whoAreYou))
        //    {
        //        return 0;
        //    }

        //    foreach (var p in line)
        //    {
        //        if (gameState.GetPosition(p) == whoAreYou)
        //        {
        //            result++;
        //        }
        //    }

        //    return result;
        //}

        private bool LineIsAvailable(WinningLine line, GameState gameState, PlayerEnum whoAreYou)
        {
            foreach (var p in line)
            {
                var pos = gameState.GetPosition(p);
                if (pos != whoAreYou && pos != PlayerEnum.Empty)
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
            AddMove(gameState, col, whoAreYou);

            var winningMove = FindWinningMove(gameState, opponent);

            RemoveMove(gameState, col);

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

            foreach (var line in _availableLines[opponent])
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
            foreach (var line in _availableLines[whoAreYou])
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
