using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using ConnectFour.Interfaces;

namespace ConnectFour.Strategy.BasicSearch
{
    public class BasicSearchStrategy : IStrategy
    {
        private long _stateCount = 0;
        private HashSet<(ulong state1, ulong state2, PlayerEnum winner)> _decisions;
        private bool _generateDecisions = false;

        public void GenerateDatabase(GameState state, PlayerEnum player)
        {
            var opponent = GetOpponent(player);
            _decisions = new HashSet<(ulong state1, ulong state2, PlayerEnum winner)>();

            _generateDecisions = true;

            for (var a = 1; a <= 6; a++)
            {
                var y1 = state.AddMove(a, player);

                for (var b = 1; b <= 6; b++)
                {
                    var y2 = state.AddMove(b, opponent);
                    var threadState = state.Copy();

                    EvaluateState(threadState, player, 2);

                    state.RemoveMove(b, y2);
                }

                state.RemoveMove(a, y1);
            }

            _generateDecisions = false;

            WriteDecisionsToDatabase(_decisions);
        }

        private HashSet<(ulong state1, ulong state2, PlayerEnum winner)> ReadDecisionsFromDatabase()
        {
            var db = new SqlServerDatabaseLayer("Data Source = localhost; Initial Catalog = ConnectFour; Integrated Security = True;");

            var table = db.GetDataTable("SELECT * FROM Decisions");

            var result = new HashSet<(ulong state1, ulong state2, PlayerEnum winner)>();

            foreach (var row in table)
            {
                result.Add((Convert.ToUInt64((long)row["State1"]), Convert.ToUInt64((long)row["State2"]), (PlayerEnum)Convert.ToInt32((byte)row["Winner"])));
            }

            return result;
        }

        private void WriteDecisionsToDatabase(HashSet<(ulong state1, ulong state2, PlayerEnum winner)> decisions)
        {
            var dataTable = CreateDecisionsTable(decisions);
            var db = new SqlServerDatabaseLayer("Data Source = localhost; Initial Catalog = ConnectFour; Integrated Security = True;");

            foreach (var (state1, state2, winner) in decisions)
            {
                InsertRowToDecisionsTable(state1, state2, winner, dataTable);
            }

            db.ExecuteNonQuery("TRUNCATE TABLE Decisions");
            db.BulkCopy("Decisions", dataTable);
        }

        private DataTable CreateDecisionsTable(HashSet<(ulong, ulong, PlayerEnum)> decisions)
        {
            var result = new DataTable("Decisions");

            result.Columns.Add("State1", typeof(long));
            result.Columns.Add("State2", typeof(long));
            result.Columns.Add("Winner", typeof(byte));

            return result;
        }

        private void InsertRowToDecisionsTable(ulong state1, ulong state2, PlayerEnum winner, DataTable decisionsTable)
        {
            var newRow = decisionsTable.NewRow();

            newRow["State1"] = state1;
            newRow["State2"] = state2;
            newRow["Winner"] = (byte)winner;

            decisionsTable.Rows.Add(newRow);
        }

        public int MakeMove(GameState state, PlayerEnum whoAreYou)
        {
            _decisions = ReadDecisionsFromDatabase();

            var move = FindWinningMove(state, whoAreYou);

            if (move != -1)
            {
                return move;
            }

            move = FindBlockingMove(state, whoAreYou);

            if (move != -1)
            {
                return move;
            }

            var safeMoves = FindSafeMoves(state, whoAreYou);

            if (safeMoves.Length == 0)
            {
                return FindValidMoves(state).First();
            }

            move = FindDoubleThreatMoves(state, safeMoves, whoAreYou);

            if (move != -1)
            {
                return move;
            }

            var drawMove = -1;

            foreach (var m in safeMoves)
            {
                var y = state.AddMove(m, whoAreYou);

                var winner = EvaluateState(state, GetOpponent(whoAreYou), 1);

                state.RemoveMove(m, y);

                if (winner == whoAreYou)
                {
                    File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] MOVE DONE (Winner) =======================================================================\n");
                    return m;
                }

                if (winner == PlayerEnum.Stalemate)
                {
                    drawMove = m;
                }
            }

            if (drawMove != -1)
            {
                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] MOVE DONE (Draw) =======================================================================\n");
                return drawMove;
            }

            File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] MOVE DONE (Loser) =======================================================================\n");
            return safeMoves.First();
        }

        private PlayerEnum EvaluateState(GameState state, PlayerEnum whoAreYou, int depth)
        {
            _stateCount++;

            if (_stateCount % 1000000 == 0)
            {
                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] {_stateCount}\n");
            }

            if (CheckDecision(state))
            {
                return GetDecision(state);
            }

            if (FindWinningMove(state, whoAreYou) != -1)
            {
                return whoAreYou;
            }

            var forcedMove = FindBlockingMove(state, whoAreYou);

            if (forcedMove != -1)
            {
                var y = state.AddMove(forcedMove, whoAreYou);
                var result = EvaluateState(state, GetOpponent(whoAreYou), depth + 1);
                state.RemoveMove(forcedMove, y);

                if (depth <= 5)
                {
                    SaveDecision(state, result);
                }

                return result;
            }

            var safeMoves = FindSafeMoves(state, whoAreYou);

            if (safeMoves.Length == 0)
            {
                return GetOpponent(whoAreYou);
            }

            if (FindDoubleThreatMoves(state, safeMoves, whoAreYou) != -1)
            {
                return whoAreYou;
            }

            var canDraw = false;

            foreach (var move in safeMoves)
            {
                var y = state.AddMove(move, whoAreYou);
                var winner = EvaluateState(state, GetOpponent(whoAreYou), depth + 1);
                state.RemoveMove(move, y);

                if (winner == whoAreYou)
                {
                    if (depth <= 5)
                    {
                        SaveDecision(state, whoAreYou);
                    }

                    return whoAreYou;
                }

                if (winner == PlayerEnum.Stalemate)
                {
                    canDraw = true;
                }
            }

            if (canDraw)
            {
                if (depth <= 5)
                {
                    SaveDecision(state, PlayerEnum.Stalemate);
                }

                return PlayerEnum.Stalemate;
            }

            if (depth <= 5)
            {
                SaveDecision(state, GetOpponent(whoAreYou));
            }

            return GetOpponent(whoAreYou);
        }

        private PlayerEnum GetDecision(GameState state)
        {
            if (_decisions != null)
            {
                var encoding = EncodeState(state);

                if (_decisions.Contains((encoding.state1, encoding.state2, PlayerEnum.PlayerOne)))
                {
                    return PlayerEnum.PlayerOne;
                }

                if (_decisions.Contains((encoding.state1, encoding.state2, PlayerEnum.PlayerTwo)))
                {
                    return PlayerEnum.PlayerTwo;
                }

                if (_decisions.Contains((encoding.state1, encoding.state2, PlayerEnum.Stalemate)))
                {
                    return PlayerEnum.Stalemate;
                }
            }

            throw new ArgumentException("No decision found", nameof(state));
        }

        private bool CheckDecision(GameState state)
        {
            if (_decisions != null)
            {
                var encoding = EncodeState(state);
                if (_decisions.Contains((encoding.state1, encoding.state2, PlayerEnum.PlayerOne)) ||
                    _decisions.Contains((encoding.state1, encoding.state2, PlayerEnum.PlayerTwo)) ||
                    _decisions.Contains((encoding.state1, encoding.state2, PlayerEnum.Stalemate)))
                {
                    return true;
                }
            }

            return false;
        }

        private void SaveDecision(GameState state, PlayerEnum winner)
        {
            if (_generateDecisions)
            {
                var encoding = EncodeState(state);
                _decisions.Add((encoding.state1, encoding.state2, winner));
            }
        }

        private (ulong state1, ulong state2) EncodeState(GameState state)
        {
            var state1 = (ulong)0;
            var state2 = (ulong)0;

            for (var y = 0; y <= 2; y++)
            {
                for (var x = 0; x <= 6; x++)
                {
                    var pos = (ulong)state.GetPosition(x, y);
                    var shift = (x * 2) + (y * 14);
                    var mask = pos << shift;

                    state1 |= mask;
                }
            }

            for (var y = 3; y <= 5; y++)
            {
                for (var x = 0; x <= 6; x++)
                {
                    var pos = (ulong)state.GetPosition(x, y);
                    var shift = (x * 2) + (y * 14);
                    var mask = pos << shift;

                    state2 |= mask;
                }
            }

            return (state1, state2);
        }

        private int FindDoubleThreatMoves(GameState state, int[] safeMoves, PlayerEnum whoAreYou)
        {
            foreach (var m in safeMoves)
            {
                var y = state.AddMove(m, whoAreYou);
                var isDoubleThreat = DoesDoubleThreatExist(state, whoAreYou);
                state.RemoveMove(m, y);

                if (isDoubleThreat)
                {
                    return m;
                }
            }

            return -1;
        }

        private bool DoesDoubleThreatExist(GameState state, PlayerEnum whoAreYou)
        {
            var threats = FindThreatCount(state, whoAreYou);

            return threats > 1;
        }

        private int FindThreatCount(GameState state, PlayerEnum whoAreYou)
        {
            var threatCount = 0;

            foreach (var l in state.GetAvailableLines(whoAreYou))
            {
                if (CountPositionsInLine(l, state, whoAreYou) == 3)
                {
                    var emptyPos = FindEmptyPositionInLine(l, state);

                    if (state.FindFirstEmptyRow(emptyPos.X) == emptyPos.Y)
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

        private bool CheckIfMoveIsSafe(GameState state, int x, PlayerEnum whoAreYou)
        {
            var opponent = GetOpponent(whoAreYou);
            var y = state.AddMove(x, whoAreYou);

            var winningMove = FindWinningMove(state, opponent);

            state.RemoveMove(x, y);

            if (winningMove == -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int FindBlockingMove(GameState state, PlayerEnum whoAreYou)
        {
            var opponent = GetOpponent(whoAreYou);

            foreach (var line in state.GetAvailableLines(opponent))
            {
                var move = CanCompleteLine(line, state, opponent);

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

        private int FindWinningMove(GameState state, PlayerEnum whoAreYou)
        {
            foreach (var line in state.GetAvailableLines(whoAreYou))
            {
                var move = CanCompleteLine(line, state, whoAreYou);

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
