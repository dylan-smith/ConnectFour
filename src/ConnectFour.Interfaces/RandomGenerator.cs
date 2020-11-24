using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFour.Interfaces
{
    public static class RandomGenerator
    {
        private static Random _generator = new System.Random();

        public static Random Generator()
        {
            return _generator;
        }
    }
}
