using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConnectFour.Interfaces;

namespace ConnectFour.Strategy.BruteForce
{
    public class BruteForceStrategy : IStrategy
    {
        private Dictionary<long, (PlayerEnum winner, int move)> _decisions;
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
        private const int STORAGE_DEPTH = 42;
        private const string WIP_SQL = "INSERT INTO DecisionsWIP(State, Winner) VALUES(@State, @Winner)";
        private const string CONNECTION_STRING = "Data Source = localhost; Initial Catalog = ConnectFour; Integrated Security = True;";
        private int MAX_DEPTH = 0;

        public void GenerateDatabase(GameState state, PlayerEnum player)
        {
            var opponent = GetOpponent(player);

            _decisions = new Dictionary<long, (PlayerEnum winner, int move)>();

            _generateDecisions = true;
            var depth = state.GetTotalMoves();
            MAX_DEPTH = depth;
            var result = PlayerEnum.GameNotDone;

            File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] STARTING\n");

            while (result == PlayerEnum.GameNotDone)
            {
                MAX_DEPTH++;
                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] DEPTH: {MAX_DEPTH}\n");
                result = EvaluateState(state, player, depth).Result.winner;
            }

            //var msg = $"EvaluateState: {_countEvaluateState}, MaxDepth: {_countMaxDepth}, CacheHit: {_countCacheHit}, CacheWait: {_countCacheWait}, WinningMove: {_countWinningMove}, BlockingMove: {_countBlockingMove}, NoSafeMoves: {_countNoSafeMoves}, DoubleThreat: {_countDoubleThreat}, FoundWinner: {_countFoundWinner}, FoundDraw: {_countFoundDraw}, NoWinnerFound: {_countNoWinnerFound}";
            //File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] {msg}\n");

            File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] Writing to DB...\n");
            _generateDecisions = false;

            WriteDecisionsToDatabase(_decisions);
            File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] DONE\n");
        }

        private Dictionary<long, (PlayerEnum winner, int move)> ReadDecisionsFromDatabase()
        {
            using (var db = new SqlServerDatabaseLayer(CONNECTION_STRING))
            {
                var table = db.GetDataTable("SELECT * FROM Decisions");

                var result = new Dictionary<long, (PlayerEnum winner, int move)>();

                foreach (var row in table)
                {
                    result.Add(Convert.ToInt64((long)row["State"]), ((PlayerEnum)Convert.ToInt32((byte)row["Winner"]), Convert.ToInt32((byte)row["Move"])));
                }

                return result;
            }
        }

        private void WriteDecisionsToDatabase(Dictionary<long, (PlayerEnum winner, int move)> decisions)
        {
            var dataTable = CreateDecisionsTable();
            var db = new SqlServerDatabaseLayer(CONNECTION_STRING);

            foreach (var decision in decisions)
            {
                InsertRowToDecisionsTable(decision.Key, decision.Value.winner, decision.Value.move, dataTable);
            }

            db.ExecuteNonQuery("TRUNCATE TABLE Decisions");
            db.BulkCopy("Decisions", dataTable);
        }

        private DataTable CreateDecisionsTable()
        {
            var result = new DataTable("Decisions");

            result.Columns.Add("State", typeof(long));
            result.Columns.Add("Winner", typeof(byte));
            result.Columns.Add("Move", typeof(byte));

            return result;
        }

        private void InsertRowToDecisionsTable(long state, PlayerEnum winner, int move, DataTable decisionsTable)
        {
            var newRow = decisionsTable.NewRow();

            newRow["State"] = state;
            newRow["Winner"] = (byte)winner;
            newRow["Move"] = (byte)move;

            decisionsTable.Rows.Add(newRow);
        }

        public int MakeMove(GameState state, PlayerEnum whoAreYou)
        {
            if (_decisions == null)
            {
                _decisions = ReadDecisionsFromDatabase();
            }

            MAX_DEPTH = 42;
            var (winner, move) = EvaluateState(state, whoAreYou, state.GetTotalMoves()).Result;

            if (winner == whoAreYou)
            {
                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] MOVE DONE (Winner) =======================================================================\n");
            }

            if (winner == PlayerEnum.Stalemate)
            {
                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] MOVE DONE (Draw) =======================================================================\n");
            }

            if (winner == GetOpponent(whoAreYou))
            {
                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] MOVE DONE (Loser) =======================================================================\n");
            }

            return move;
        }

        private async Task<(PlayerEnum winner, int move)> EvaluateState(GameState state, PlayerEnum whoAreYou, int depth)
        {
            //_countEvaluateState++;

            if (depth == 42)
            {
                //_countStalemate++;
                return (PlayerEnum.Stalemate, 0);
            }

            if (depth >= MAX_DEPTH)
            {
                //_countMaxDepth++;
                return (PlayerEnum.GameNotDone, 0);
            }

            var encoding = state.GetEncoding();

            if (_decisions.ContainsKey(encoding))
            {
                //_countCacheHit++;
                return _decisions[encoding];
            }

            var winningMove = FindWinningMove(state, whoAreYou);

            if (winningMove != -1)
            {
                //_countWinningMove++;
                SaveDecision(state, whoAreYou, winningMove, depth);
                return (whoAreYou, winningMove);
            }

            var forcedMove = FindBlockingMove(state, whoAreYou);

            if (forcedMove != -1)
            {
                //_countBlockingMove++;
                var y = state.AddMove(forcedMove, whoAreYou);
                var result = await EvaluateState(state, GetOpponent(whoAreYou), depth + 1);
                state.RemoveMove(forcedMove, y, whoAreYou);

                if (result.winner != PlayerEnum.GameNotDone)
                {
                    SaveDecision(state, result.winner, forcedMove, depth);
                }

                return result;
            }

            var safeMoves = FindSafeMoves(state, whoAreYou);

            if (safeMoves.Length == 0)
            {
                //_countNoSafeMoves++;
                var validMove = FindValidMove(state);
                SaveDecision(state, GetOpponent(whoAreYou), validMove, depth);
                return (GetOpponent(whoAreYou), validMove);
            }

            var doubleThreatMove = FindDoubleThreatMove(state, safeMoves, whoAreYou);

            if (doubleThreatMove != -1)
            {
                //_countDoubleThreat++;
                SaveDecision(state, whoAreYou, doubleThreatMove, depth);
                return (whoAreYou, doubleThreatMove);
            }

            var canDrawMove = -1;
            var notDone = false;

            var startIdx = safeMoves.Length / 2;

            for (var i = 0; i < safeMoves.Length; i++)
            {
                var move = safeMoves[(startIdx + i) % safeMoves.Length];
                var y = state.AddMove(move, whoAreYou);
                var result = await EvaluateState(state, GetOpponent(whoAreYou), depth + 1);
                state.RemoveMove(move, y, whoAreYou);

                if (result.winner == whoAreYou)
                {
                    //_countFoundWinner++;
                    SaveDecision(state, whoAreYou, move, depth);
                    return (whoAreYou, move);
                }

                if (result.winner == PlayerEnum.Stalemate)
                {
                    canDrawMove = move;
                }

                if (result.winner == PlayerEnum.GameNotDone)
                {
                    notDone = true;
                }
            }

            if (notDone)
            {
                return (PlayerEnum.GameNotDone, 0);
            }

            if (canDrawMove >= 0)
            {
                //_countFoundDraw++;
                SaveDecision(state, PlayerEnum.Stalemate, canDrawMove, depth);
                return (PlayerEnum.Stalemate, canDrawMove);
            }

            //_countNoWinnerFound++;
            SaveDecision(state, GetOpponent(whoAreYou), safeMoves[0], depth);
            return (GetOpponent(whoAreYou), safeMoves[0]);
        }

        private void SaveDecision(GameState state, PlayerEnum winner, int move, int depth)
        {
            if (_generateDecisions && depth <= STORAGE_DEPTH)
            {
                var encoding = state.GetEncoding();
                _decisions.Add(encoding, (winner, move));
            }
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
