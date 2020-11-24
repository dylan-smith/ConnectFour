using ConnectFour.Interfaces;

namespace ConnectFour.Engine
{
    public class ThreadArg
    {
        public GameState CurrentState { get; set; }

        public int NextPosition { get; set; }
    }
}
