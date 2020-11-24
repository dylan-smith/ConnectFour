using System.Collections.Generic;
using System.Drawing;

namespace ConnectFour.Interfaces
{
    public class WinningLine : List<Point>
    {
        public WinningLine() : base(4)
        {
        }

        public PlayerEnum CheckLine(GameState state)
        {
            if (state.GetPosition(base[0]) == state.GetPosition(base[1]) &&
                state.GetPosition(base[0]) == state.GetPosition(base[2]) &&
                state.GetPosition(base[0]) == state.GetPosition(base[3]) &&
                (state.GetPosition(base[0]) == PlayerEnum.PlayerOne || state.GetPosition(base[0]) == PlayerEnum.PlayerTwo))
            {
                return state.GetPosition(base[0]);
            }

            return PlayerEnum.GameNotDone;
        }
    }
}
