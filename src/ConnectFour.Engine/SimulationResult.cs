using System.Collections.Generic;
using ConnectFour.Interfaces;

namespace ConnectFour.Engine
{
    public class SimulationResult
    {
        private readonly IList<GameLog> _gameLogs;

        public SimulationResult()
        {
            PlayerOneWins = 0;
            PlayerTwoWins = 0;
            Stalemates = 0;

            _gameLogs = new List<GameLog>();
        }

        public int PlayerOneWins { get; set; }

        public int PlayerTwoWins { get; set; }

        public int Stalemates { get; set; }

        public IEnumerable<GameLog> GameLogs
        {
            get
            {
                return _gameLogs;
            }
        }

        public void AddGameResult(GameLog gameResult)
        {
            if (gameResult.Winner == PlayerEnum.PlayerOne)
            {
                PlayerOneWins++;
            }

            if (gameResult.Winner == PlayerEnum.PlayerTwo)
            {
                PlayerTwoWins++;
            }

            if (gameResult.Winner == PlayerEnum.Stalemate)
            {
                Stalemates++;
            }

            _gameLogs.Add(gameResult);
        }
    }
}
