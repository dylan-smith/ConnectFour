using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConnectFour.Interfaces;

namespace ConnectFour.Strategy.BasicSearch
{
    public class BasicSearchStrategy : IStrategy
    {
        private ConcurrentDictionary<(ulong state1, ulong state2), PlayerEnum> _decisions;
        private bool _generateDecisions = false;
        private const int STORAGE_DEPTH = 16;

        public void GenerateDatabase(GameState state, PlayerEnum player)
        {
            var opponent = GetOpponent(player);
            _decisions = new ConcurrentDictionary<(ulong state1, ulong state2), PlayerEnum>();

            _generateDecisions = true;
            var tasks = new List<Task>();

            for (var a = 0; a <= 6; a++)
            {
                var y1 = state.AddMove(a, player);

                for (var b = 0; b <= 6; b++)
                {
                    var y2 = state.AddMove(b, opponent);

                    var threadState = state.Copy();

                    //var task = new Task(async () => await EvaluateState(threadState, opponent, 3));
                    //var task = EvaluateState(threadState, opponent, 3);
                    var task = new Task<PlayerEnum>(() => EvaluateState(threadState, player, 2).Result);
                    task.Start();
                    tasks.Add(task);

                    //for (var c = 0; c <= 6; c++)
                    //{
                    //    var y3 = state.AddMove(c, player);
                    //    var threadState = state.Copy();

                    //    //var task = new Task(async () => await EvaluateState(threadState, opponent, 3));
                    //    //var task = EvaluateState(threadState, opponent, 3);
                    //    var task = new Task<PlayerEnum>(() => EvaluateState(threadState, opponent, 3).Result);
                    //    task.Start();
                    //    tasks.Add(task);
                        
                    //    //task.Wait();

                    //    state.RemoveMove(c, y3);
                    //}

                    state.RemoveMove(b, y2);
                }

                state.RemoveMove(a, y1);
            }

            while (tasks.Any())
            {
                var done = Task.WaitAny(tasks.ToArray());
                tasks.RemoveAt(done);

                lock (_decisions)
                {
                    File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] Thread Complete ({ tasks.Count } left)\n");
                }
            }

            _generateDecisions = false;

            WriteDecisionsToDatabase(_decisions);
        }

        private ConcurrentDictionary<(ulong state1, ulong state2), PlayerEnum> ReadDecisionsFromDatabase()
        {
            var db = new SqlServerDatabaseLayer("Data Source = localhost; Initial Catalog = ConnectFour; Integrated Security = True;");

            var table = db.GetDataTable("SELECT * FROM Decisions");

            var result = new ConcurrentDictionary<(ulong state1, ulong state2), PlayerEnum>();

            foreach (var row in table)
            {
                result.TryAdd((Convert.ToUInt64((long)row["State1"]), Convert.ToUInt64((long)row["State2"])), (PlayerEnum)Convert.ToInt32((byte)row["Winner"]));
            }

            return result;
        }

        private void WriteDecisionsToDatabase(ConcurrentDictionary<(ulong state1, ulong state2), PlayerEnum> decisions)
        {
            var dataTable = CreateDecisionsTable();
            var db = new SqlServerDatabaseLayer("Data Source = localhost; Initial Catalog = ConnectFour; Integrated Security = True;");

            foreach (var decision in decisions)
            {
                InsertRowToDecisionsTable(decision.Key.state1, decision.Key.state2, decision.Value, dataTable);
            }

            db.ExecuteNonQuery("TRUNCATE TABLE Decisions");
            db.BulkCopy("Decisions", dataTable);
        }

        private DataTable CreateDecisionsTable()
        {
            var result = new DataTable("Decisions");

            result.Columns.Add("State1", typeof(ulong));
            result.Columns.Add("State2", typeof(ulong));
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
            if (_decisions == null)
            {
                _decisions = ReadDecisionsFromDatabase();
            }

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

                var task = EvaluateState(state, GetOpponent(whoAreYou), 1);
                //task.Wait();
                var winner = task.Result;

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

        private async Task<PlayerEnum> EvaluateState(GameState state, PlayerEnum whoAreYou, int depth)
        {
            if (CheckDecision(state, depth))
            {
                var decision = GetDecision(state);

                while (decision == PlayerEnum.GameNotDone)
                {
                    //System.Diagnostics.Debug.WriteLine($"[{System.Threading.Thread.CurrentThread.ManagedThreadId}] WAITING ON ({EncodeState(state).state1}, {EncodeState(state).state2})");
                    await Task.Yield();
                    //await Task.Delay(1000);
                    decision = GetDecision(state);
                }

                //System.Diagnostics.Debug.WriteLine($"[{System.Threading.Thread.CurrentThread.ManagedThreadId}] WAIT DONE ({EncodeState(state).state1}, {EncodeState(state).state2})");
                return decision;
            }

            if (FindWinningMove(state, whoAreYou) != -1)
            {
                SaveDecision(state, whoAreYou, depth);
                return whoAreYou;
            }

            var forcedMove = FindBlockingMove(state, whoAreYou);

            if (forcedMove != -1)
            {
                var y = state.AddMove(forcedMove, whoAreYou);
                var result = await EvaluateState(state, GetOpponent(whoAreYou), depth + 1);
                state.RemoveMove(forcedMove, y);

                SaveDecision(state, result, depth);

                return result;
            }

            var safeMoves = FindSafeMoves(state, whoAreYou);

            if (safeMoves.Length == 0)
            {
                SaveDecision(state, GetOpponent(whoAreYou), depth);
                return GetOpponent(whoAreYou);
            }

            if (FindDoubleThreatMoves(state, safeMoves, whoAreYou) != -1)
            {
                SaveDecision(state, whoAreYou, depth);
                return whoAreYou;
            }

            var canDraw = false;

            foreach (var move in safeMoves)
            {
                var y = state.AddMove(move, whoAreYou);
                var winner = await EvaluateState(state, GetOpponent(whoAreYou), depth + 1);
                state.RemoveMove(move, y);

                if (winner == whoAreYou)
                {
                    SaveDecision(state, whoAreYou, depth);

                    return whoAreYou;
                }

                if (winner == PlayerEnum.Stalemate)
                {
                    canDraw = true;
                }
            }

            if (canDraw)
            {
                SaveDecision(state, PlayerEnum.Stalemate, depth);

                return PlayerEnum.Stalemate;
            }

            SaveDecision(state, GetOpponent(whoAreYou), depth);

            return GetOpponent(whoAreYou);
        }

        private PlayerEnum GetDecision(GameState state)
        {
            var encoding = EncodeState(state);

            return _decisions[(encoding.state1, encoding.state2)];
        }

        private bool CheckDecision(GameState state, int depth)
        {
            if (_decisions != null && depth <= STORAGE_DEPTH)
            {
                var encoding = EncodeState(state);

                //if (_decisions.Count % 100000 == 0)
                //{
                //    lock (_decisions)
                //    {
                //        File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] {_decisions.Count}\n");
                //    }
                //}

                if (_generateDecisions)
                {
                    var result = _decisions.TryAdd((encoding.state1, encoding.state2), PlayerEnum.GameNotDone);

                    //if (result)
                    //{
                    //    System.Diagnostics.Debug.WriteLine($"[{System.Threading.Thread.CurrentThread.ManagedThreadId}] ({encoding.state1}, {encoding.state2}) ADDED");
                    //} else
                    //{
                    //    System.Diagnostics.Debug.WriteLine($"[{System.Threading.Thread.CurrentThread.ManagedThreadId}] ({encoding.state1}, {encoding.state2}) WAITING");
                    //}

                    return !result;
                }
                else
                {
                    return _decisions.ContainsKey((encoding.state1, encoding.state2));
                }
            }

            return false;
        }

        private void SaveDecision(GameState state, PlayerEnum winner, int depth)
        {
            if (_generateDecisions && depth <= STORAGE_DEPTH)
            {
                //if (_decisions.Count % 100000 == 0)
                //{
                //    lock (_decisions)
                //    {
                //        File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] {_decisions.Count}\n");
                //    }
                //}

                var encoding = EncodeState(state);
                _decisions.AddOrUpdate((encoding.state1, encoding.state2), winner, (a, b) => winner);

                //System.Diagnostics.Debug.WriteLine($"[{System.Threading.Thread.CurrentThread.ManagedThreadId}] ({encoding.state1}, {encoding.state2}) UPDATED TO {winner}");
            }
        }

        private (ulong state1, ulong state2) EncodeState(GameState state)
        {
            var state1 = (ulong)0;
            var state2 = (ulong)0;

            for (var y = 0; y <= 2; y++)
            {
                var yShift = y * 14;

                for (var x = 0; x <= 6; x++)
                {
                    var pos = (ulong)state.GetPosition(x, y);
                    var shift = (x * 2) + yShift;
                    var mask = pos << shift;

                    state1 |= mask;
                }
            }

            for (var y = 3; y <= 5; y++)
            {
                var yShift = (y - 3) * 14;

                for (var x = 0; x <= 6; x++)
                {
                    var pos = (ulong)state.GetPosition(x, y);
                    var shift = (x * 2) + yShift;
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
