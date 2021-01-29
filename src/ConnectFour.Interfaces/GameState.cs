using System;
using System.Collections.Generic;
using System.Drawing;

namespace ConnectFour.Interfaces
{
    public class GameState
    {
        //private PlayerEnum[][] _board;
        private long _state = 0L;
        private long _playerOneChips = 0L;
        private long _playerTwoChips = 0L;

        //private int[] _chipCounts = new int[3];
        //private List<WinningLine>[] _availableLines;
        private bool[][] _availableLines;

        //private int[] _nextEmptyRow;

        private const long ONES = 0b_111_111_111_111_111_111_111_1111111_1111111_1111111_1111111_1111111_1111111;
        private long[] ZERO_OUT_NEXT_EMPTY = new long[7];
        // private const long PLAYER_ONE_CHIPS_MASK = 0b_1111111_1111111_1111111_1111111_1111111_1111111;

        public GameState()
        {
            ZERO_OUT_NEXT_EMPTY[0] = 0b_111_111_111_111_111_111_000_1111111_1111111_1111111_1111111_1111111_1111111;
            ZERO_OUT_NEXT_EMPTY[1] = 0b_111_111_111_111_111_000_111_1111111_1111111_1111111_1111111_1111111_1111111;
            ZERO_OUT_NEXT_EMPTY[2] = 0b_111_111_111_111_000_111_111_1111111_1111111_1111111_1111111_1111111_1111111;
            ZERO_OUT_NEXT_EMPTY[3] = 0b_111_111_111_000_111_111_111_1111111_1111111_1111111_1111111_1111111_1111111;
            ZERO_OUT_NEXT_EMPTY[4] = 0b_111_111_000_111_111_111_111_1111111_1111111_1111111_1111111_1111111_1111111;
            ZERO_OUT_NEXT_EMPTY[5] = 0b_111_000_111_111_111_111_111_1111111_1111111_1111111_1111111_1111111_1111111;
            ZERO_OUT_NEXT_EMPTY[6] = 0b_000_111_111_111_111_111_111_1111111_1111111_1111111_1111111_1111111_1111111;

            _availableLines = InitializeAvailableLines();
        }

        private bool[][] InitializeAvailableLines()
        {
            var result = new bool[3][];

            result[(int)PlayerEnum.PlayerOne] = new bool[WinningLines.GetAllWinningLines().Length];
            result[(int)PlayerEnum.PlayerTwo] = new bool[WinningLines.GetAllWinningLines().Length];

            for (var i = 0; i < WinningLines.GetAllWinningLines().Length; i++)
            {
                var line = WinningLines.GetAllWinningLines()[i];

                result[(int)PlayerEnum.PlayerOne][i] = LineIsAvailable(line, PlayerEnum.PlayerOne);
                result[(int)PlayerEnum.PlayerTwo][i] = LineIsAvailable(line, PlayerEnum.PlayerTwo);
            }

            return result;
        }

        private bool LineIsAvailable(WinningLine line, PlayerEnum whoAreYou)
        {
            for (var i = 0; i < 4; i++)
            {
                var pos = GetPosition(line[i]);
                if (pos != whoAreYou && pos != PlayerEnum.Empty)
                {
                    return false;
                }
            }

            return true;
        }

        public GameState(GameState source)
        {
            _state = source._state;
            _playerOneChips = source._playerOneChips;
            _playerTwoChips = source._playerTwoChips;

            ZERO_OUT_NEXT_EMPTY[0] = 0b_111_111_111_111_111_111_000___11_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;
            ZERO_OUT_NEXT_EMPTY[1] = 0b_111_111_111_111_111_000_111___11_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;
            ZERO_OUT_NEXT_EMPTY[2] = 0b_111_111_111_111_000_111_111___11_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;
            ZERO_OUT_NEXT_EMPTY[3] = 0b_111_111_111_000_111_111_111___11_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;
            ZERO_OUT_NEXT_EMPTY[4] = 0b_111_111_000_111_111_111_111___11_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;
            ZERO_OUT_NEXT_EMPTY[5] = 0b_111_000_111_111_111_111_111___11_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;
            ZERO_OUT_NEXT_EMPTY[6] = 0b_000_111_111_111_111_111_111___11_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;

            _availableLines = InitializeAvailableLines();
        }

        public long GetEncoding()
        {
            // TODO: Symmetry check
            var symmetry = GetSymmetryState();

            if (symmetry < _state)
            {
                return symmetry;
            }

            return _state;
        }

        private long GetSymmetryState()
        {
            return _state;
        }

        public PlayerEnum GetPosition(int x, int y)
        {
            // TODO: Might be able to skip this check if we know we're only checking non-empty positions
            var empty = FindFirstEmptyRow(x);

            if (y >= empty)
            {
                return PlayerEnum.Empty;
            }

            var shift = y * 7 + x;
            var mask = 1L << shift;

            var result = _state & mask;

            if (result > 0)
            {
                return PlayerEnum.PlayerOne;
            }

            return PlayerEnum.PlayerTwo;
        }

        public PlayerEnum GetPosition(Point p)
        {
            return GetPosition(p.X, p.Y);
        }

        public bool IsEmpty(Point p)
        {
            return IsEmpty(p.X, p.Y);
        }

        private bool IsEmpty(int x, int y)
        {
            return y >= FindFirstEmptyRow(x);
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

        public IEnumerable<WinningLine> GetAvailableLines(PlayerEnum player)
        {
            for (var i = 0; i < WinningLines.TOTAL_LINES; i++)
            {
                if (_availableLines[(int)player][i])
                {
                    yield return WinningLines.GetAllWinningLines()[i];
                }
            }
        }

        public bool AreMovesAvailable()
        {
            var empty = _state >> 42;

            return empty != 0b110_110_110_110_110_110_110;
        }

        public int AddMove(int move, PlayerEnum player)
        {
            var y = FindFirstEmptyRow(move);

            if (y > 5)
            {
                throw new ArgumentException("Invalid Move - that column is full");
            }

            SetFirstEmptyRow(move, y + 1);

            if (player == PlayerEnum.PlayerOne)
            {
                SetPlayerOneMove(move, y);
            }
            else
            {
                SetPlayerTwoMove(move, y);
            }

            var lines = WinningLines.GetLineIndexesByPoint(move, y);
            var opponent = GetOpponent(player);

            foreach (var line in lines)
            {
                _availableLines[(int)opponent][line] = false;
            }

            return y;
        }

        private void SetPlayerOneMove(int x, int y)
        {
            var shift = y * 7 + x;
            var mask = 1L << shift;

            _state |= mask;
            _playerOneChips |= mask;
        }

        private void SetPlayerTwoMove(int x, int y)
        {
            var shift = y * 7 + x;
            var mask = 1L << shift;

            _playerTwoChips |= mask;
        }

        private void SetPositionToZero(int x, int y)
        {
            var shift = y * 7 + x;
            var mask = (1L << shift) ^ ONES;

            _state &= mask;
            _playerOneChips &= mask;
            _playerTwoChips &= mask;
        }

        private void SetFirstEmptyRow(int x, int y)
        {
            var mask = (long)y << (42 + x * 3);
            _state = _state & ZERO_OUT_NEXT_EMPTY[x] | mask;
        }

        public void RemoveMove(int x, int y)
        {
            SetFirstEmptyRow(x, y);
            SetPositionToZero(x, y);

            // TODO: this doesn't properly update available lines
        }

        public void RemoveMove(int x, int y, PlayerEnum player)
        {
            SetFirstEmptyRow(x, y);
            SetPositionToZero(x, y);

            var lines = WinningLines.GetLineIndexesByPoint(x, y);

            var opponent = GetOpponent(player);

            foreach (var line in lines)
            {
                if (LineIsAvailable(WinningLines.GetAllWinningLines()[line], opponent))
                {
                    _availableLines[(int)opponent][line] = true;
                }
            }
        }

        public int CanCompleteLine(WinningLine line, PlayerEnum player)
        {
            var chips = 0L;

            if (player == PlayerEnum.PlayerOne)
            {
                chips = _playerOneChips;
            }
            else
            {
                chips = _playerTwoChips;
            }

            var result = chips & line.Mask;

            if (result == line.SpotOne)
            {
                if (FindFirstEmptyRow(line[0].X) == line[0].Y)
                {
                    return line[0].X;
                }
            }
            else if (result == line.SpotTwo)
            {
                if (FindFirstEmptyRow(line[1].X) == line[1].Y)
                {
                    return line[1].X;
                }
            }
            else if (result == line.SpotThree)
            {
                if (FindFirstEmptyRow(line[2].X) == line[2].Y)
                {
                    return line[2].X;
                }
            }
            else if (result == line.SpotFour)
            {
                if (FindFirstEmptyRow(line[3].X) == line[3].Y)
                {
                    return line[3].X;
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

        public int FindFirstEmptyRow(int column)
        {
            var shift = 42 + column * 3;

            return (int)((_state >> shift) & 0b_111);
        }

        public GameState Copy()
        {
            return new GameState(this);
        }

        public int GetTotalMoves()
        {
            var result = 0;

            for (var x = 0; x <= 6; x++)
            {
                result += FindFirstEmptyRow(x);
            }

            return result;
        }

        public override string ToString()
        {
            var binaryString = Convert.ToString(_state, 2);

            binaryString = binaryString.PadLeft(63, '0');

            var result = _state.ToString() + Environment.NewLine;
            result += $"{binaryString.Substring(0, 3)}_{binaryString.Substring(3, 3)}_{binaryString.Substring(6, 3)}_{binaryString.Substring(9, 3)}_{binaryString.Substring(12, 3)}_{binaryString.Substring(15, 3)}_{binaryString.Substring(18, 3)}_{binaryString.Substring(21, 7)}_{binaryString.Substring(28, 7)}_{binaryString.Substring(35, 7)}_{binaryString.Substring(42, 7)}_{binaryString.Substring(49, 7)}_{binaryString.Substring(56, 7)}";
            result += Environment.NewLine;

            for (var y = 5; y >= 0; y--)
            {
                result += Environment.NewLine;

                for (var x = 0; x <= 6; x++)
                {
                    if (GetPosition(x, y) == PlayerEnum.Empty)
                    {
                        result += "x";
                    }

                    if (GetPosition(x, y) == PlayerEnum.PlayerOne)
                    {
                        result += "1";
                    }

                    if (GetPosition(x, y) == PlayerEnum.PlayerTwo)
                    {
                        result += "0";
                    }
                }
            }

            return result;
        }
    }
}
