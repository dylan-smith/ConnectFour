using System;
using System.Collections.Generic;
using System.Drawing;

namespace ConnectFour.Interfaces
{
    public class GameState
    {
        private PlayerEnum[][] board;

        private Dictionary<PlayerEnum, int> chipCounts = new Dictionary<PlayerEnum, int>(5);

        private Dictionary<WinningLine, (int playerOne, int playerTwo, int empty)> _lineChipCounts = new Dictionary<WinningLine, (int playerOne, int playerTwo, int empty)>();

        public GameState()
            : this(PlayerEnum.Empty)
        {
        }

        public GameState(PlayerEnum startState)
        {
            InitializeChipCounts();

            board = new PlayerEnum[7][];

            for (int x = 0; x < 7; x++)
            {
                board[x] = new PlayerEnum[6];
                for (int y = 0; y < 6; y++)
                {
                    board[x][y] = startState;
                }
            }

            chipCounts[startState] = 42;

            foreach (var line in WinningLines.GetAllWinningLines())
            {
                _lineChipCounts.Add(line, (0, 0, 4));
            }
        }

        public GameState(GameState source)
        {
            InitializeChipCounts();

            board = new PlayerEnum[7][];

            for (int x = 0; x < 7; x++)
            {
                board[x] = new PlayerEnum[6];
                for (int y = 0; y < 6; y++)
                {
                    board[x][y] = source.GetPosition(x, y);
                    chipCounts[source.GetPosition(x, y)]++;
                }
            }
        }

        public int GetChipCount(PlayerEnum player)
        {
            return chipCounts[player];
        }

        public PlayerEnum GetPosition(int x, int y)
        {
            return board[x][y];
        }

        public PlayerEnum GetPosition(Point p)
        {
            return board[p.X][p.Y];
        }

        public PlayerEnum CheckForWinner()
        {
            foreach (var line in WinningLines.GetAllWinningLines())
            {
                var winner = line.CheckLine(this);

                if (winner != PlayerEnum.GameNotDone)
                {
                    return winner;
                }
            }

            if (AreMovesAvailable())
            {
                return PlayerEnum.GameNotDone;
            }
            else
            {
                return PlayerEnum.Stalemate;
            }
        }

        public bool AreMovesAvailable()
        {
            for (int i = 0; i < 7; i++)
            {
                if (FindFirstEmptyRow(i) != -1)
                {
                    return true;
                }
            }

            return false;
        }

        public int AddMove(int move, PlayerEnum player)
        {
            var y = FindFirstEmptyRow(move);

            if (y == -1)
            {
                throw new ArgumentException("Invalid Move - that column is full");
            }

            chipCounts[board[move][y]]--;
            board[move][y] = player;
            chipCounts[player]++;

            return y;
        }

        public void UpdateWithMove(int x, int y, PlayerEnum player)
        {
            chipCounts[board[x][y]]--;
            board[x][y] = player;
            chipCounts[player]++;
        }

        public (int y, PlayerEnum player) RemoveMove(int col)
        {
            var y = FindFirstEmptyRow(col);

            if (y == -1)
            {
                y = 5;
            }
            else
            {
                y = y - 1;
            }

            var player = board[col][y];
            chipCounts[player]--;
            board[col][y] = PlayerEnum.Empty;
            chipCounts[PlayerEnum.Empty]++;

            return (y, player);
        }

        public int FindFirstEmptyRow(int column)
        {
            int y = 0;

            while (y < 6)
            {
                if (board[column][y] == PlayerEnum.Empty)
                {
                    return y;
                }

                y++;
            }

            return -1;
        }

        public GameState Copy()
        {
            return new GameState(this);
        }

        private void InitializeChipCounts()
        {
            foreach (var x in Enum.GetValues(typeof(PlayerEnum)))
            {
                chipCounts.Add((PlayerEnum)x, 0);
            }
        }
    }
}
