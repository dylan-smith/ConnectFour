using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConnectFour.Interfaces;

namespace ConnectFour.Strategy.BasicSearch
{
    public class BasicSearchStrategy : IStrategy
    {
        private ConcurrentDictionary<long, PlayerEnum> _decisions;
        private bool _generateDecisions = false;
        //private long _countEvaluateState = 0;
        //private long _countMaxDepth = 0;
        //private long _countCacheHit = 0;
        //private long _countCacheWait = 0;
        //private long _countWinningMove = 0;
        //private long _countBlockingMove = 0;
        //private long _countNoSafeMoves = 0;
        //private long _countDoubleThreat = 0;
        //private long _countFoundWinner = 0;
        //private long _countFoundDraw = 0;
        //private long _countNoWinnerFound = 0;
        private const int STORAGE_DEPTH = 36;

        public void GenerateDatabase(GameState state, PlayerEnum player)
        {
            var opponent = GetOpponent(player);
            _decisions = new ConcurrentDictionary<long, PlayerEnum>();

            _generateDecisions = true;
            var tasks = new List<Task>();
            var depth = state.GetTotalMoves();

            File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] STARTING\n");

            for (var a = 0; a <= 6; a++)
            {
                var y1 = state.AddMove(a, player);

                //var threadState = state.Copy();

                //var task = new Task<PlayerEnum>(() => EvaluateState(threadState, opponent, depth + 1).Result);
                //task.Start();
                //tasks.Add(task);

                for (var b = 0; b <= 6; b++)
                {
                    var y2 = state.AddMove(b, opponent);

                    var threadState = state.Copy();

                    var task = new Task<PlayerEnum>(() => EvaluateState(threadState, player, depth + 2).Result);
                    task.Start();
                    tasks.Add(task);
                    task.Wait();

                    
                    File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] Thread Complete ({ tasks.Count } done)\n");
                    //var msg = $"EvaluateState: {_countEvaluateState}, MaxDepth: {_countMaxDepth}, CacheHit: {_countCacheHit}, CacheWait: {_countCacheWait}, WinningMove: {_countWinningMove}, BlockingMove: {_countBlockingMove}, NoSafeMoves: {_countNoSafeMoves}, DoubleThreat: {_countDoubleThreat}, FoundWinner: {_countFoundWinner}, FoundDraw: {_countFoundDraw}, NoWinnerFound: {_countNoWinnerFound}";
                    //File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] {msg}\n");

                    //for (var c = 0; c <= 6; c++)
                    //{
                    //    var y3 = state.AddMove(c, player);
                    //    var threadState = state.Copy();

                    //    var task = new Task<PlayerEnum>(() => EvaluateState(threadState, opponent, depth + 3).Result);
                    //    task.Start();
                    //    tasks.Add(task);

                    //    state.RemoveMove(c, y3);
                    //}

                    state.RemoveMove(b, y2, opponent);
                }

                state.RemoveMove(a, y1, player);
            }

            while (tasks.Any())
            {
                var done = Task.WaitAny(tasks.ToArray());
                tasks.RemoveAt(done);

                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] Thread Complete ({ tasks.Count } left)\n");
                //var msg = $"EvaluateState: {_countEvaluateState}, MaxDepth: {_countMaxDepth}, CacheHit: {_countCacheHit}, CacheWait: {_countCacheWait}, WinningMove: {_countWinningMove}, BlockingMove: {_countBlockingMove}, NoSafeMoves: {_countNoSafeMoves}, DoubleThreat: {_countDoubleThreat}, FoundWinner: {_countFoundWinner}, FoundDraw: {_countFoundDraw}, NoWinnerFound: {_countNoWinnerFound}";
                //File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] {msg}\n");
            }

            _generateDecisions = false;

             WriteDecisionsToDatabase(_decisions);
            File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] DONE\n");
        }

        private ConcurrentDictionary<long, PlayerEnum> ReadDecisionsFromDatabase()
        {
            var db = new SqlServerDatabaseLayer("Data Source = localhost; Initial Catalog = ConnectFour; Integrated Security = True;");

            var table = db.GetDataTable("SELECT * FROM Decisions");

            var result = new ConcurrentDictionary<long, PlayerEnum>();

            foreach (var row in table)
            {
                result.TryAdd(Convert.ToInt64((long)row["State1"]), (PlayerEnum)Convert.ToInt32((byte)row["Winner"]));
            }

            return result;
        }

        private void WriteDecisionsToDatabase(ConcurrentDictionary<long, PlayerEnum> decisions)
        {
            var dataTable = CreateDecisionsTable();
            var db = new SqlServerDatabaseLayer("Data Source = localhost; Initial Catalog = ConnectFour; Integrated Security = True;");

            foreach (var decision in decisions)
            {
                InsertRowToDecisionsTable(decision.Key, decision.Value, dataTable);
            }

            db.ExecuteNonQuery("TRUNCATE TABLE Decisions");
            db.BulkCopy("Decisions", dataTable);
        }

        private DataTable CreateDecisionsTable()
        {
            var result = new DataTable("Decisions");

            result.Columns.Add("State1", typeof(long));
            result.Columns.Add("State2", typeof(long));
            result.Columns.Add("Winner", typeof(byte));

            return result;
        }

        private void InsertRowToDecisionsTable(long state, PlayerEnum winner, DataTable decisionsTable)
        {
            var newRow = decisionsTable.NewRow();

            newRow["State1"] = state;
            newRow["State2"] = 0L;
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
                var winner = task.Result;

                state.RemoveMove(m, y, whoAreYou);

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
            //_countEvaluateState++;

            if (depth == 42)
            {
                //_countMaxDepth++;
                return PlayerEnum.Stalemate;
            }

            if (CheckDecision(state, depth))
            {
                //_countCacheHit++;

                var decision = GetDecision(state);

                while (decision == PlayerEnum.GameNotDone)
                {
                    //_countCacheWait++;
                    await Task.Yield();
                    decision = GetDecision(state);
                }

                return decision;
            }

            if (FindWinningMove(state, whoAreYou) != -1)
            {
                //_countWinningMove++;
                SaveDecision(state, whoAreYou, depth);
                return whoAreYou;
            }

            var forcedMove = FindBlockingMove(state, whoAreYou);

            if (forcedMove != -1)
            {
                //_countBlockingMove++;
                var y = state.AddMove(forcedMove, whoAreYou);
                var result = await EvaluateState(state, GetOpponent(whoAreYou), depth + 1);
                state.RemoveMove(forcedMove, y, whoAreYou);

                SaveDecision(state, result, depth);

                return result;
            }

            var safeMoves = FindSafeMoves(state, whoAreYou);

            if (safeMoves.Length == 0)
            {
                //_countNoSafeMoves++;
                SaveDecision(state, GetOpponent(whoAreYou), depth);
                return GetOpponent(whoAreYou);
            }

            if (FindDoubleThreatMoves(state, safeMoves, whoAreYou) != -1)
            {
                //_countDoubleThreat++;
                SaveDecision(state, whoAreYou, depth);
                return whoAreYou;
            }

            var canDraw = false;

            foreach (var move in safeMoves)
            {
                var y = state.AddMove(move, whoAreYou);
                var winner = await EvaluateState(state, GetOpponent(whoAreYou), depth + 1);
                state.RemoveMove(move, y, whoAreYou);

                if (winner == whoAreYou)
                {
                    //_countFoundWinner++;
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
                //_countFoundDraw++;
                SaveDecision(state, PlayerEnum.Stalemate, depth);

                return PlayerEnum.Stalemate;
            }

            //_countNoWinnerFound++;
            SaveDecision(state, GetOpponent(whoAreYou), depth);

            return GetOpponent(whoAreYou);
        }

        private PlayerEnum GetDecision(GameState state)
        {
            return _decisions[state.GetEncoding()];
        }

        private bool CheckDecision(GameState state, int depth)
        {
            if (_decisions != null && depth <= STORAGE_DEPTH)
            {
                if (_generateDecisions)
                {
                    var result = _decisions.TryAdd(state.GetEncoding(), PlayerEnum.GameNotDone);

                    return !result;
                }
                else
                {
                    return _decisions.ContainsKey(state.GetEncoding());
                }
            }

            return false;
        }

        private void SaveDecision(GameState state, PlayerEnum winner, int depth)
        {
            if (_generateDecisions && depth <= STORAGE_DEPTH)
            {
                _decisions.AddOrUpdate(state.GetEncoding(), winner, (a, b) => winner);
            }
        }

        public int FindDoubleThreatMoves(GameState state, int[] safeMoves, PlayerEnum whoAreYou)
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

        private int[] FindValidMoves(GameState gameState)
        {
            var result = new List<int>();

            for (int col = 0; col < 7; col++)
            {
                if (gameState.FindFirstEmptyRow(col) != 6)
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

            state.RemoveMove(x, y, whoAreYou);

            return winningMove == -1;
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
    }
}
