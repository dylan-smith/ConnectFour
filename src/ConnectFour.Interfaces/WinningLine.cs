using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ConnectFour.Interfaces
{
    public class WinningLine: IEnumerable<Point>
    {
        private readonly Point[] _points = new Point[4];

        public Point this[int i]
        {
            get { return _points[i]; }
            set { _points[i] = value; }
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
