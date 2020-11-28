using System;
using System.Collections.Generic;
using System.Drawing;

namespace ConnectFour.Interfaces
{
    public class GameState
    {
        private PlayerEnum[][] board;

        private Dictionary<PlayerEnum, int> chipCounts = new Dictionary<PlayerEnum, int>(5);

        private int[] _nextEmptyRow;

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

            _nextEmptyRow = new int[7];
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
                if (_nextEmptyRow[i] <= 5)
                {
                    return true;
                }
            }

            return false;
        }

        public int AddMove(int move, PlayerEnum player)
        {
            var y = _nextEmptyRow[move];

            if (y > 5)
            {
                throw new ArgumentException("Invalid Move - that column is full");
            }

            chipCounts[board[move][y]]--;
            board[move][y] = player;
            chipCounts[player]++;
            _nextEmptyRow[move]++;

            return y;
        }

        public void RemoveMove(int x, int y)
        {
            chipCounts[board[x][y]]--;
            board[x][y] = PlayerEnum.Empty;
            chipCounts[PlayerEnum.Empty]++;

            _nextEmptyRow[x] = y;
        }

        public int FindFirstEmptyRow(int column)
        {
            if (_nextEmptyRow[column] <= 5)
            {
                return _nextEmptyRow[column];
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
