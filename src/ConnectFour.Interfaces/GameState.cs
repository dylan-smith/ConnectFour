using System;
using System.Collections.Generic;
using System.Drawing;

namespace ConnectFour.Interfaces
{
    public class GameState
    {
        private PlayerEnum[][] _board;

        private int[] _chipCounts = new int[3];
        private List<WinningLine>[] _availableLines;

        private int[] _nextEmptyRow;

        public GameState()
            : this(PlayerEnum.Empty)
        {
        }

        public GameState(PlayerEnum startState)
        {
            _board = new PlayerEnum[7][];

            for (int x = 0; x < 7; x++)
            {
                _board[x] = new PlayerEnum[6];
                for (int y = 0; y < 6; y++)
                {
                    _board[x][y] = startState;
                }
            }

            _chipCounts[(int)startState] = 42;

            _nextEmptyRow = new int[7];

            _availableLines = InitializeAvailableLines();
        }

        private List<WinningLine>[] InitializeAvailableLines()
        {
            var result = new List<WinningLine>[3];

            result[(int)PlayerEnum.PlayerOne] = new List<WinningLine>();
            result[(int)PlayerEnum.PlayerTwo] = new List<WinningLine>();

            foreach (var line in WinningLines.GetAllWinningLines())
            {
                if (LineIsAvailable(line, PlayerEnum.PlayerOne))
                {
                    result[(int)PlayerEnum.PlayerOne].Add(line);
                }

                if (LineIsAvailable(line, PlayerEnum.PlayerTwo))
                {
                    result[(int)PlayerEnum.PlayerTwo].Add(line);
                }
            }

            return result;
        }

        private bool LineIsAvailable(WinningLine line, PlayerEnum whoAreYou)
        {
            for (var i = 0; i < 4; i ++)
            {
                var pos = GetPosition(line[i]);
                if (pos != whoAreYou && pos != PlayerEnum.Empty)
                {
                    return false;
                }
            }

            return true;
        }

        public List<WinningLine> GetAvailableLines(PlayerEnum player)
        {
            return _availableLines[(int)player];
        }

        public GameState(GameState source)
        {
            _board = new PlayerEnum[7][];
            _nextEmptyRow = new int[7];

            for (int x = 0; x < 7; x++)
            {
                _board[x] = new PlayerEnum[6];
                for (int y = 0; y < 6; y++)
                {
                    var sourcePlayer = source.GetPosition(x, y);

                    _board[x][y] = sourcePlayer;
                    _chipCounts[(int)sourcePlayer]++;

                    if (sourcePlayer == PlayerEnum.PlayerOne || sourcePlayer == PlayerEnum.PlayerTwo)
                    {
                        _nextEmptyRow[x] = y + 1;
                    }
                }
            }

            _availableLines = InitializeAvailableLines();
        }

        public int GetChipCount(PlayerEnum player)
        {
            return _chipCounts[(int)player];
        }

        public PlayerEnum GetPosition(int x, int y)
        {
            return _board[x][y];
        }

        public PlayerEnum GetPosition(Point p)
        {
            return _board[p.X][p.Y];
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
            return _chipCounts[(int)PlayerEnum.Empty] != 0;
        }

        public int AddMove(int move, PlayerEnum player)
        {
            var y = _nextEmptyRow[move];

            if (y > 5)
            {
                throw new ArgumentException("Invalid Move - that column is full");
            }

            _chipCounts[(int)PlayerEnum.Empty]--;
            _board[move][y] = player;
            _chipCounts[(int)player]++;
            _nextEmptyRow[move]++;

            var lines = WinningLines.GetLinesByPoint(move, y);
            var opponent = GetOpponent(player);

            foreach (var line in lines)
            {
                _availableLines[(int)opponent].Remove(line);
            }

            return y;
        }

        public void RemoveMove(int x, int y)
        {
            var player = _board[x][y];
            _chipCounts[(int)player]--;
            _board[x][y] = PlayerEnum.Empty;
            _chipCounts[(int)PlayerEnum.Empty]++;

            _nextEmptyRow[x] = y;

            var lines = WinningLines.GetLinesByPoint(x, y);
            
            var opponent = GetOpponent(player);

            foreach (var line in lines)
            {
                if (LineIsAvailable(line, opponent))
                {
                    _availableLines[(int)opponent].Add(line);
                }
            }
        }

        private PlayerEnum GetOpponent(PlayerEnum whoAreYou)
        {
            if (whoAreYou == PlayerEnum.PlayerOne)
            {
                return PlayerEnum.PlayerTwo;
            }

            return PlayerEnum.PlayerOne;
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
    }
}
