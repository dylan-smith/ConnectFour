using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConnectFour.Engine;
using ConnectFour.Interfaces;
using ConnectFour.Strategy.BasicSearch;
using ConnectFour.Strategy.BlockDoubleThreat;
using ConnectFour.Strategy.BlockOpponentsLine;
using ConnectFour.Strategy.CountLineChips;
using ConnectFour.Strategy.FindDoubleThreat;
using ConnectFour.Strategy.FindMoveWithHighestLineCount;
using ConnectFour.Strategy.MakeWinningMove;
using ConnectFour.Strategy.Random;

namespace ConnectFour.UI
{
    // TODO: have a private GameState object that we manipulate, then just write a method to render it into the panels
    // this way we don't have to reinvent GameState functions in the form with panels
    public partial class Form1 : Form
    {
        private Panel[][] _boardPanels;
        private GameLog _currentGame;
        private int _currentMove = 0;
        private Color _nextColor = Color.Red;
        private IStrategy _playerOne;
        private IStrategy _playerTwo;

        public Form1()
        {
            InitializeComponent();

            _boardPanels = new Panel[7][];

            for (int x = 0; x < 7; x++)
            {
                _boardPanels[x] = new Panel[6];
            }

            _boardPanels[0][0] = Position1;
            _boardPanels[1][0] = Position2;
            _boardPanels[2][0] = Position3;
            _boardPanels[3][0] = Position4;
            _boardPanels[4][0] = Position5;
            _boardPanels[5][0] = Position6;
            _boardPanels[6][0] = Position7;
            _boardPanels[0][1] = Position8;
            _boardPanels[1][1] = Position9;
            _boardPanels[2][1] = Position10;
            _boardPanels[3][1] = Position11;
            _boardPanels[4][1] = Position12;
            _boardPanels[5][1] = Position13;
            _boardPanels[6][1] = Position14;
            _boardPanels[0][2] = Position15;
            _boardPanels[1][2] = Position16;
            _boardPanels[2][2] = Position17;
            _boardPanels[3][2] = Position18;
            _boardPanels[4][2] = Position19;
            _boardPanels[5][2] = Position20;
            _boardPanels[6][2] = Position21;
            _boardPanels[0][3] = Position22;
            _boardPanels[1][3] = Position23;
            _boardPanels[2][3] = Position24;
            _boardPanels[3][3] = Position25;
            _boardPanels[4][3] = Position26;
            _boardPanels[5][3] = Position27;
            _boardPanels[6][3] = Position28;
            _boardPanels[0][4] = Position29;
            _boardPanels[1][4] = Position30;
            _boardPanels[2][4] = Position31;
            _boardPanels[3][4] = Position32;
            _boardPanels[4][4] = Position33;
            _boardPanels[5][4] = Position34;
            _boardPanels[6][4] = Position35;
            _boardPanels[0][5] = Position36;
            _boardPanels[1][5] = Position37;
            _boardPanels[2][5] = Position38;
            _boardPanels[3][5] = Position39;
            _boardPanels[4][5] = Position40;
            _boardPanels[5][5] = Position41;
            _boardPanels[6][5] = Position42;
        }

        private void RunSimulationButton_Click(object sender, EventArgs e)
        {
            //var engine = new GameEngine();

            //_playerOne = new BlockDoubleThreatStrategy();
            //_playerTwo = new BasicSearchStrategy();

            //var results = engine.RunSimulation(int.Parse(NumberOfGamesTextBox.Text), _playerOne, _playerTwo);

            //ShowResults(results);


            var playerOne = new BasicSearchStrategy();
            var playerTwo = new BlockDoubleThreatStrategy();

            WinningLines.Initialize();
            var state = new GameState();
            var log = new GameLog(PlayerEnum.PlayerOne);
            var results = new SimulationResult();

            state.AddMove(0, PlayerEnum.PlayerOne);
            log.Moves.Add(0);
            state.AddMove(1, PlayerEnum.PlayerTwo);
            log.Moves.Add(1);
            state.AddMove(2, PlayerEnum.PlayerOne);
            log.Moves.Add(2);
            state.AddMove(3, PlayerEnum.PlayerTwo);
            log.Moves.Add(3);
            state.AddMove(4, PlayerEnum.PlayerOne);
            log.Moves.Add(4);
            state.AddMove(5, PlayerEnum.PlayerTwo);
            log.Moves.Add(5);
            state.AddMove(6, PlayerEnum.PlayerOne);
            log.Moves.Add(6);
            state.AddMove(0, PlayerEnum.PlayerTwo);
            log.Moves.Add(0);
            state.AddMove(1, PlayerEnum.PlayerOne);
            log.Moves.Add(1);
            state.AddMove(2, PlayerEnum.PlayerTwo);
            log.Moves.Add(2);
            state.AddMove(3, PlayerEnum.PlayerOne);
            log.Moves.Add(3);
            state.AddMove(4, PlayerEnum.PlayerTwo);
            log.Moves.Add(4);

            state.AddMove(0, PlayerEnum.PlayerOne);
            log.Moves.Add(0);
            state.AddMove(0, PlayerEnum.PlayerTwo);
            log.Moves.Add(0);
            state.AddMove(0, PlayerEnum.PlayerOne);
            log.Moves.Add(0);
            state.AddMove(0, PlayerEnum.PlayerTwo);
            log.Moves.Add(0);

            var whoGoesNext = PlayerEnum.PlayerOne;

            playerOne.GenerateDatabase(state, whoGoesNext);

            while (log.Winner != PlayerEnum.PlayerOne && log.Winner != PlayerEnum.PlayerTwo)
            {
                int nextMove;

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
                    results.AddGameResult(log);
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

            ShowResults(results);



            //var start = new Stopwatch();
            //start.Start();
            //strat.MakeMove(state, PlayerEnum.PlayerOne);
            //var end = start.ElapsedMilliseconds;

            //MessageBox.Show("Done! " + end.ToString());


            
        }

        private void ShowResults(SimulationResult results)
        {
            PlayerOneWinsTextBox.Text = results.PlayerOneWins.ToString();
            PlayerTwoWinsTextBox.Text = results.PlayerTwoWins.ToString();
            StalematesTextBox.Text = results.Stalemates.ToString();

            GameResultsListBox.Items.Clear();

            // Display only the first 1000 in the list
            foreach (var g in results.GameLogs.Take(1000))
            {
                GameResultsListBox.Items.Add(g);
            }
        }

        private void ShowGameEnding()
        {
            ClearPanels();

            var nextRow = new int[7];

            for (int i = 0; i < 7; i++)
            {
                nextRow[i] = 0;
            }

            var nextColor = Color.Red;

            foreach (var m in _currentGame.Moves)
            {
                _boardPanels[m][nextRow[m]].BackColor = nextColor;

                if (nextColor == Color.Red)
                {
                    nextColor = Color.Blue;
                }
                else
                {
                    nextColor = Color.Red;
                }

                nextRow[m]++;
            }
        }

        private void ClearPanels()
        {
            foreach (var col in _boardPanels)
            {
                foreach (var pos in col)
                {
                    pos.BackColor = Color.White;
                }
            }
        }

        private void GameResultsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentGame = (GameLog)GameResultsListBox.SelectedItem;
            DisplayGameLog();
        }

        private void DisplayGameLog()
        {
            ShowGameEnding();

            if (_currentGame.WhoGoesFirst == PlayerEnum.PlayerOne)
            {
                PlayerOneColorTextBox.Text = "Red";
                PlayerTwoColorTextBox.Text = "Blue";
                WhoWentFirstTextBox.Text = "Player One";
            }
            else
            {
                PlayerOneColorTextBox.Text = "Blue";
                PlayerTwoColorTextBox.Text = "Red";
                WhoWentFirstTextBox.Text = "Player Two";
            }

            if (_currentGame.Winner == PlayerEnum.PlayerOne)
            {
                WinnerTextBox.Text = "Player One";
            }

            if (_currentGame.Winner == PlayerEnum.PlayerTwo)
            {
                WinnerTextBox.Text = "Player Two";
            }

            if (_currentGame.Winner == PlayerEnum.Stalemate)
            {
                WinnerTextBox.Text = "Stalemate";
            }
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            ClearPanels();
            _nextColor = Color.Red;
            _currentMove = 0;
        }

        private void NextMoveButton_Click(object sender, EventArgs e)
        {
            if (_currentMove < _currentGame.Moves.Count)
            {
                var x = _currentGame.Moves[_currentMove];
                var y = FindNextPositionInColumn(x);

                _boardPanels[x][y].BackColor = _nextColor;

                SwapNextColor();

                _currentMove++;
            }
        }

        private void SwapNextColor()
        {
            if (_nextColor == Color.Red)
            {
                _nextColor = Color.Blue;
            }
            else
            {
                _nextColor = Color.Red;
            }
        }

        private int FindNextPositionInColumn(int column)
        {
            var pos = 0;

            while (_boardPanels[column][pos].BackColor != Color.White)
            {
                pos++;
            }

            return pos;
        }

        private void PreviousMoveButton_Click(object sender, EventArgs e)
        {
            if (_currentMove > 0)
            {
                _currentMove--;

                var x = _currentGame.Moves[_currentMove];
                var y = -1;

                if (_boardPanels[x][5].BackColor != Color.White)
                {
                    y = 5;
                }
                else
                {
                    y = FindNextPositionInColumn(x) - 1;
                }

                _boardPanels[x][y].BackColor = Color.White;

                SwapNextColor();
            }
        }

        private void ShowEndingButton_Click(object sender, EventArgs e)
        {
            ShowGameEnding();
        }

        private void CalculateStatsButton_Click(object sender, EventArgs e)
        {
            //var stats = new StatsCalculator();

            //var stateCount = stats.CountValidGameStatesStart();

            //MessageBox.Show(stateCount.ToString());
        }

        private void DebugNextMoveButton_Click(object sender, EventArgs e)
        {
            var gameState = CreateGameStateFromGameLog();

            if (_nextColor == Color.Red)
            {
                _playerOne.MakeMove(gameState, PlayerEnum.PlayerOne);
            }
            else
            {
                _playerTwo.MakeMove(gameState, PlayerEnum.PlayerTwo);
            }
        }

        private GameState CreateGameStateFromGameLog()
        {
            var result = new GameState(PlayerEnum.Empty);

            var whoGoesNext = PlayerEnum.PlayerOne;

            for (int m = 0; m < _currentMove; m++)
            {
                result.AddMove(_currentGame.Moves[m], whoGoesNext);
                whoGoesNext = GetOpponent(whoGoesNext);
            }

            return result;
        }

        private PlayerEnum GetOpponent(PlayerEnum whoGoesNext)
        {
            if (whoGoesNext == PlayerEnum.PlayerOne)
            {
                return PlayerEnum.PlayerTwo;
            }
            else
            {
                return PlayerEnum.PlayerOne;
            }
        }
    }
}
