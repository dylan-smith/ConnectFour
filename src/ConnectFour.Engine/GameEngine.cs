using System;
using System.IO;
using ConnectFour.Interfaces;

namespace ConnectFour.Engine
{
    public class GameEngine
    {
        // TODO: Use a background thread and a progress bar
        // TODO: Use multi-threading to run multiple games at once
        // TODO: Capture timings for each strategy
        public SimulationResult RunSimulation(int numGames, IStrategy playerOne, IStrategy playerTwo)
        {
            var results = new SimulationResult();
            WinningLines.Initialize();

            var whoGoesFirst = RandomlyDecideWhoGoesFirst();
            File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] Starting Simulation {numGames} games...\n");

            for (int i = 0; i < numGames; i++)
            {
                var gameResult = SimulateGame(playerOne, playerTwo, whoGoesFirst);
                results.AddGameResult(gameResult);

                File.AppendAllText(@"C:\git\ConnectFour\ConnectFour.log", $"[{DateTime.Now}] Game Complete {i} / {numGames} (Winner: {gameResult.Winner.ToString()})\n");

                whoGoesFirst = DecideWhoGoesFirst(gameResult);
            }

            return results;
        }

        private PlayerEnum DecideWhoGoesFirst(GameLog gameResult)
        {
            if (gameResult.Winner == PlayerEnum.PlayerOne)
            {
                return PlayerEnum.PlayerTwo;
            }

            if (gameResult.Winner == PlayerEnum.PlayerTwo)
            {
                return PlayerEnum.PlayerOne;
            }

            if (gameResult.Winner == PlayerEnum.Stalemate)
            {
                if (gameResult.WhoGoesFirst == PlayerEnum.PlayerOne)
                {
                    return PlayerEnum.PlayerTwo;
                }
                else
                {
                    return PlayerEnum.PlayerOne;
                }
            }

            throw new ArgumentException("Unexpected Value for Winner");
        }

        private PlayerEnum RandomlyDecideWhoGoesFirst()
        {
            var rnd = new Random();

            var x = rnd.Next(0, 2);

            if (x == 0)
            {
                return PlayerEnum.PlayerOne;
            }
            else
            {
                return PlayerEnum.PlayerTwo;
            }
        }

        private GameLog SimulateGame(IStrategy playerOne, IStrategy playerTwo, PlayerEnum whoGoesNext)
        {
            var state = new GameState();
            var log = new GameLog(whoGoesNext);

            while (true)
            {
                int nextMove;

                // TODO: make a copy of state before giving it to the strategy
                if (whoGoesNext == PlayerEnum.PlayerOne)
                {
                    nextMove = playerOne.MakeMove(state, whoGoesNext);
                }
                else
                {
                    nextMove = playerTwo.MakeMove(state, whoGoesNext);
                }

                state.AddMove(nextMove, whoGoesNext);
                log.Moves.Add(nextMove);

                var whoWon = state.CheckForWinner();

                if (whoWon != PlayerEnum.GameNotDone)
                {
                    log.Winner = whoWon;
                    return log;
                }

                if (whoGoesNext == PlayerEnum.PlayerOne)
                {
                    whoGoesNext = PlayerEnum.PlayerTwo;
                }
                else
                {
                    whoGoesNext = PlayerEnum.PlayerOne;
                }
            }
        }
    }
}
