using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ConnectFour.Interfaces
{
    public class WinningLine : IEnumerable<Point>
    {
        private readonly Point[] _points = new Point[4];
        public long Mask = 0L;
        public long SpotOne = 0L;
        public long SpotTwo = 0L;
        public long SpotThree = 0L;
        public long SpotFour = 0L;

        public Point this[int i]
        {
            get { return _points[i]; }
            set { _points[i] = value; }
        }

        public void Initialize()
        {
            foreach (var p in _points)
            {
                var shift = p.Y * 7 + p.X;
                var pointMask = 1L << shift;

                Mask |= pointMask;
            }

            for (var i = 0; i < 4; i++)
            {
                var state = 0L;

                for (var j = 0; j < 4; j++)
                {
                    if (i != j)
                    {
                        var shift = _points[j].Y * 7 + _points[j].X;
                        var pointMask = 1L << shift;

                        state |= pointMask;
                    }
                }

                if (i == 0)
                {
                    SpotOne = state;
                }
                else if (i == 1)
                {
                    SpotTwo = state;
                }
                else if (i == 2)
                {
                    SpotThree = state;
                }
                else if (i == 3)
                {
                    SpotFour = state;
                }
            }
        }

        public PlayerEnum CheckLine(GameState state)
        {
            if (state.GetPosition(_points[0]) == state.GetPosition(_points[1]) &&
                state.GetPosition(_points[0]) == state.GetPosition(_points[2]) &&
                state.GetPosition(_points[0]) == state.GetPosition(_points[3]) &&
                (state.GetPosition(_points[0]) == PlayerEnum.PlayerOne || state.GetPosition(_points[0]) == PlayerEnum.PlayerTwo))
            {
                return state.GetPosition(_points[0]);
            }

            return PlayerEnum.GameNotDone;
        }

        public IEnumerator<Point> GetEnumerator()
        {
            yield return _points[0];
            yield return _points[1];
            yield return _points[2];
            yield return _points[3];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _points.GetEnumerator();
        }
    }
}
