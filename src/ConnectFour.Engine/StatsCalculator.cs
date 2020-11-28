//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using ConnectFour.Interfaces;

//namespace ConnectFour.Engine
//{
//    public class StatsCalculator
//    {
//        private static long _numStates = 0;

//        public long CountValidGameStatesStart()
//        {
//            WinningLines.Initialize();

//            var beginState = new GameState(PlayerEnum.GameNotDone);

//            CountValidGameStatesRecurse(beginState, 0);

//            return _numStates;
//        }

//        private void CountValidGameStatesRecurse(object args)
//        {
//            CountValidGameStatesRecurse(((ThreadArg)args).CurrentState, ((ThreadArg)args).NextPosition);
//        }

//        private void CountValidGameStatesRecurse(GameState currentState, int nextPosition)
//        {
//            if (nextPosition == 42)
//            {
//                Interlocked.Increment(ref _numStates);
//                return;
//            }

//            // TODO: maybe change this from using maths, to using a lookup
//            var x = nextPosition % 7;
//            var y = (int)Math.Floor((double)nextPosition / 7);

//            nextPosition++;

//            // This can happen if a cell lower down in the column was marked empty
//            if (currentState.GetPosition(x, y) != PlayerEnum.GameNotDone)
//            {
//                CountValidGameStatesRecurse(currentState, nextPosition);
//                return;
//            }

//            if (nextPosition <= 2)
//            {
//                var s1 = currentState.Copy();
//                var s2 = currentState.Copy();
//                var s3 = currentState.Copy();

//                s1.RemoveMove(x, y, PlayerEnum.PlayerOne);
//                s2.RemoveMove(x, y, PlayerEnum.PlayerTwo);

//                for (int i = y; i < 6; i++)
//                {
//                    s3.RemoveMove(x, i, PlayerEnum.Empty);
//                }

//                var thread1 = new Thread(new ParameterizedThreadStart(CountValidGameStatesRecurse));
//                var thread2 = new Thread(new ParameterizedThreadStart(CountValidGameStatesRecurse));
//                var thread3 = new Thread(new ParameterizedThreadStart(CountValidGameStatesRecurse));

//                var args1 = new ThreadArg() { CurrentState = s1, NextPosition = nextPosition };
//                var args2 = new ThreadArg() { CurrentState = s2, NextPosition = nextPosition };
//                var args3 = new ThreadArg() { CurrentState = s3, NextPosition = nextPosition };

//                if (IsValidState(s1, x, y, PlayerEnum.PlayerOne))
//                {
//                    thread1.Start(args1);
//                }

//                if (IsValidState(s2, x, y, PlayerEnum.PlayerTwo))
//                {
//                    thread2.Start(args2);
//                }

//                if (IsValidState(s3, x, y, PlayerEnum.Empty))
//                {
//                    thread3.Start(args3);
//                }

//                thread1.Join();
//                thread2.Join();
//                thread3.Join();
//            }
//            else
//            {
//                currentState.RemoveMove(x, y, PlayerEnum.PlayerOne);
//                if (IsValidState(currentState, x, y, PlayerEnum.PlayerOne))
//                {
//                    CountValidGameStatesRecurse(currentState, nextPosition);
//                }

//                currentState.RemoveMove(x, y, PlayerEnum.PlayerTwo);
//                if (IsValidState(currentState, x, y, PlayerEnum.PlayerTwo))
//                {
//                    CountValidGameStatesRecurse(currentState, nextPosition);
//                }

//                // Update this cell and all above it with Empty
//                for (int i = y; i < 6; i++)
//                {
//                    currentState.RemoveMove(x, i, PlayerEnum.Empty);
//                }

//                if (IsValidState(currentState, x, y, PlayerEnum.Empty))
//                {
//                    CountValidGameStatesRecurse(currentState, nextPosition);
//                }

//                for (int i = y; i < 6; i++)
//                {
//                    currentState.RemoveMove(x, i, PlayerEnum.GameNotDone);
//                }
//            }
//        }

//        private bool IsValidState(GameState currentState, int x, int y, PlayerEnum player)
//        {
//            if (ContainsUnbalancedPlayerChips(currentState))
//            {
//                return false;
//            }

//            if (player != PlayerEnum.Empty)
//            {
//                if (ContainsWinningLine(currentState, x, y))
//                {
//                    return false;
//                }
//            }

//            return true;
//        }

//        private bool ContainsUnbalancedPlayerChips(GameState currentState)
//        {
//            var oneChipCount = currentState.GetChipCount(PlayerEnum.PlayerOne);
//            var twoChipCount = currentState.GetChipCount(PlayerEnum.PlayerTwo);
//            var notDoneChipCount = currentState.GetChipCount(PlayerEnum.GameNotDone);

//            if (Math.Abs(oneChipCount - twoChipCount) > (notDoneChipCount + 1))
//            {
//                return true;
//            }

//            return false;
//        }

//        private bool ContainsWinningLine(GameState currentState, int x, int y)
//        {
//            foreach (var line in WinningLines.GetLinesByPoint(x, y))
//            {
//                if (line.CheckLine(currentState) != PlayerEnum.GameNotDone)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }
//    }
//}
