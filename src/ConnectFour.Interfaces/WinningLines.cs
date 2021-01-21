using System.Collections.Generic;
using System.Drawing;

namespace ConnectFour.Interfaces
{
    public static class WinningLines
    {
        private static WinningLine[] lines;
        private static WinningLine[][] linesByPoint;

        public static void Initialize()
        {
            InitializeLines();
            InitializeLinesByPoint();
        }

        public static WinningLine[] GetAllWinningLines()
        {
            return lines;
        }

        public static WinningLine[] GetLinesByPoint(int x, int y)
        {
            return linesByPoint[y * 7 + x];
        }

        private static void InitializeLinesByPoint()
        {
            var result = new List<WinningLine>[42];

            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    var pointLines = new List<WinningLine>();

                    foreach (var l in lines)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            if (l[i].X == x && l[i].Y == y)
                            {
                                pointLines.Add(l);
                                break;
                            }
                        }
                    }

                    result[y * 7 + x] = pointLines;
                }
            }

            linesByPoint = new WinningLine[42][];

            for (var i = 0; i < 42; i++)
            {
                linesByPoint[i] = new WinningLine[result[i].Count];

                for (var j = 0; j < result[i].Count; j++)
                {
                    linesByPoint[i][j] = result[i][j];
                }
            }
        }

        private static void InitializeLines()
        {
            lines = new WinningLine[69];

            for (int i = 0; i < 69; i++)
            {
                lines[i] = new WinningLine();
            }

            // Horizontal Lines
            lines[0][0] = new Point(0, 0);
            lines[0][1] = new Point(1, 0);
            lines[0][2] = new Point(2, 0);
            lines[0][3] = new Point(3, 0);

            lines[1][0] = new Point(1, 0);
            lines[1][1] = new Point(2, 0);
            lines[1][2] = new Point(3, 0);
            lines[1][3] = new Point(4, 0);

            lines[2][0] = new Point(2, 0);
            lines[2][1] = new Point(3, 0);
            lines[2][2] = new Point(4, 0);
            lines[2][3] = new Point(5, 0);

            lines[3][0] = new Point(3, 0);
            lines[3][1] = new Point(4, 0);
            lines[3][2] = new Point(5, 0);
            lines[3][3] = new Point(6, 0);

            lines[4][0] = new Point(0, 1);
            lines[4][1] = new Point(1, 1);
            lines[4][2] = new Point(2, 1);
            lines[4][3] = new Point(3, 1);

            lines[5][0] = new Point(1, 1);
            lines[5][1] = new Point(2, 1);
            lines[5][2] = new Point(3, 1);
            lines[5][3] = new Point(4, 1);

            lines[6][0] = new Point(2, 1);
            lines[6][1] = new Point(3, 1);
            lines[6][2] = new Point(4, 1);
            lines[6][3] = new Point(5, 1);

            lines[7][0] = new Point(3, 1);
            lines[7][1] = new Point(4, 1);
            lines[7][2] = new Point(5, 1);
            lines[7][3] = new Point(6, 1);

            lines[8][0] = new Point(0, 2);
            lines[8][1] = new Point(1, 2);
            lines[8][2] = new Point(2, 2);
            lines[8][3] = new Point(3, 2);

            lines[9][0] = new Point(1, 2);
            lines[9][1] = new Point(2, 2);
            lines[9][2] = new Point(3, 2);
            lines[9][3] = new Point(4, 2);

            lines[10][0] = new Point(2, 2);
            lines[10][1] = new Point(3, 2);
            lines[10][2] = new Point(4, 2);
            lines[10][3] = new Point(5, 2);

            lines[11][0] = new Point(3, 2);
            lines[11][1] = new Point(4, 2);
            lines[11][2] = new Point(5, 2);
            lines[11][3] = new Point(6, 2);

            lines[12][0] = new Point(0, 3);
            lines[12][1] = new Point(1, 3);
            lines[12][2] = new Point(2, 3);
            lines[12][3] = new Point(3, 3);

            lines[13][0] = new Point(1, 3);
            lines[13][1] = new Point(2, 3);
            lines[13][2] = new Point(3, 3);
            lines[13][3] = new Point(4, 3);

            lines[14][0] = new Point(2, 3);
            lines[14][1] = new Point(3, 3);
            lines[14][2] = new Point(4, 3);
            lines[14][3] = new Point(5, 3);

            lines[15][0] = new Point(3, 3);
            lines[15][1] = new Point(4, 3);
            lines[15][2] = new Point(5, 3);
            lines[15][3] = new Point(6, 3);

            lines[16][0] = new Point(0, 4);
            lines[16][1] = new Point(1, 4);
            lines[16][2] = new Point(2, 4);
            lines[16][3] = new Point(3, 4);

            lines[17][0] = new Point(1, 4);
            lines[17][1] = new Point(2, 4);
            lines[17][2] = new Point(3, 4);
            lines[17][3] = new Point(4, 4);

            lines[18][0] = new Point(2, 4);
            lines[18][1] = new Point(3, 4);
            lines[18][2] = new Point(4, 4);
            lines[18][3] = new Point(5, 4);

            lines[19][0] = new Point(3, 4);
            lines[19][1] = new Point(4, 4);
            lines[19][2] = new Point(5, 4);
            lines[19][3] = new Point(6, 4);

            lines[20][0] = new Point(0, 5);
            lines[20][1] = new Point(1, 5);
            lines[20][2] = new Point(2, 5);
            lines[20][3] = new Point(3, 5);

            lines[21][0] = new Point(1, 5);
            lines[21][1] = new Point(2, 5);
            lines[21][2] = new Point(3, 5);
            lines[21][3] = new Point(4, 5);

            lines[22][0] = new Point(2, 5);
            lines[22][1] = new Point(3, 5);
            lines[22][2] = new Point(4, 5);
            lines[22][3] = new Point(5, 5);

            lines[23][0] = new Point(3, 5);
            lines[23][1] = new Point(4, 5);
            lines[23][2] = new Point(5, 5);
            lines[23][3] = new Point(6, 5);

            // Vertical lines            
            lines[24][0] = new Point(0, 0);
            lines[24][1] = new Point(0, 1);
            lines[24][2] = new Point(0, 2);
            lines[24][3] = new Point(0, 3);

            lines[25][0] = new Point(0, 1);
            lines[25][1] = new Point(0, 2);
            lines[25][2] = new Point(0, 3);
            lines[25][3] = new Point(0, 4);

            lines[26][0] = new Point(0, 2);
            lines[26][1] = new Point(0, 3);
            lines[26][2] = new Point(0, 4);
            lines[26][3] = new Point(0, 5);

            lines[27][0] = new Point(1, 0);
            lines[27][1] = new Point(1, 1);
            lines[27][2] = new Point(1, 2);
            lines[27][3] = new Point(1, 3);

            lines[28][0] = new Point(1, 1);
            lines[28][1] = new Point(1, 2);
            lines[28][2] = new Point(1, 3);
            lines[28][3] = new Point(1, 4);

            lines[29][0] = new Point(1, 2);
            lines[29][1] = new Point(1, 3);
            lines[29][2] = new Point(1, 4);
            lines[29][3] = new Point(1, 5);

            lines[30][0] = new Point(2, 0);
            lines[30][1] = new Point(2, 1);
            lines[30][2] = new Point(2, 2);
            lines[30][3] = new Point(2, 3);

            lines[31][0] = new Point(2, 1);
            lines[31][1] = new Point(2, 2);
            lines[31][2] = new Point(2, 3);
            lines[31][3] = new Point(2, 4);

            lines[32][0] = new Point(2, 2);
            lines[32][1] = new Point(2, 3);
            lines[32][2] = new Point(2, 4);
            lines[32][3] = new Point(2, 5);

            lines[33][0] = new Point(3, 0);
            lines[33][1] = new Point(3, 1);
            lines[33][2] = new Point(3, 2);
            lines[33][3] = new Point(3, 3);

            lines[34][0] = new Point(3, 1);
            lines[34][1] = new Point(3, 2);
            lines[34][2] = new Point(3, 3);
            lines[34][3] = new Point(3, 4);

            lines[35][0] = new Point(3, 2);
            lines[35][1] = new Point(3, 3);
            lines[35][2] = new Point(3, 4);
            lines[35][3] = new Point(3, 5);

            lines[36][0] = new Point(4, 0);
            lines[36][1] = new Point(4, 1);
            lines[36][2] = new Point(4, 2);
            lines[36][3] = new Point(4, 3);

            lines[37][0] = new Point(4, 1);
            lines[37][1] = new Point(4, 2);
            lines[37][2] = new Point(4, 3);
            lines[37][3] = new Point(4, 4);

            lines[38][0] = new Point(4, 2);
            lines[38][1] = new Point(4, 3);
            lines[38][2] = new Point(4, 4);
            lines[38][3] = new Point(4, 5);

            lines[39][0] = new Point(5, 0);
            lines[39][1] = new Point(5, 1);
            lines[39][2] = new Point(5, 2);
            lines[39][3] = new Point(5, 3);

            lines[40][0] = new Point(5, 1);
            lines[40][1] = new Point(5, 2);
            lines[40][2] = new Point(5, 3);
            lines[40][3] = new Point(5, 4);

            lines[41][0] = new Point(5, 2);
            lines[41][1] = new Point(5, 3);
            lines[41][2] = new Point(5, 4);
            lines[41][3] = new Point(5, 5);

            lines[42][0] = new Point(6, 0);
            lines[42][1] = new Point(6, 1);
            lines[42][2] = new Point(6, 2);
            lines[42][3] = new Point(6, 3);

            lines[43][0] = new Point(6, 1);
            lines[43][1] = new Point(6, 2);
            lines[43][2] = new Point(6, 3);
            lines[43][3] = new Point(6, 4);

            lines[44][0] = new Point(6, 2);
            lines[44][1] = new Point(6, 3);
            lines[44][2] = new Point(6, 4);
            lines[44][3] = new Point(6, 5);

            // diagonal lines - left to right, bottom to top
            lines[45][0] = new Point(0, 0);
            lines[45][1] = new Point(1, 1);
            lines[45][2] = new Point(2, 2);
            lines[45][3] = new Point(3, 3);

            lines[46][0] = new Point(0, 1);
            lines[46][1] = new Point(1, 2);
            lines[46][2] = new Point(2, 3);
            lines[46][3] = new Point(3, 4);

            lines[47][0] = new Point(0, 2);
            lines[47][1] = new Point(1, 3);
            lines[47][2] = new Point(2, 4);
            lines[47][3] = new Point(3, 5);

            lines[48][0] = new Point(1, 0);
            lines[48][1] = new Point(2, 1);
            lines[48][2] = new Point(3, 2);
            lines[48][3] = new Point(4, 3);

            lines[49][0] = new Point(1, 1);
            lines[49][1] = new Point(2, 2);
            lines[49][2] = new Point(3, 3);
            lines[49][3] = new Point(4, 4);

            lines[50][0] = new Point(1, 2);
            lines[50][1] = new Point(2, 3);
            lines[50][2] = new Point(3, 4);
            lines[50][3] = new Point(4, 5);

            lines[51][0] = new Point(2, 0);
            lines[51][1] = new Point(3, 1);
            lines[51][2] = new Point(4, 2);
            lines[51][3] = new Point(5, 3);

            lines[52][0] = new Point(2, 1);
            lines[52][1] = new Point(3, 2);
            lines[52][2] = new Point(4, 3);
            lines[52][3] = new Point(5, 4);

            lines[53][0] = new Point(2, 2);
            lines[53][1] = new Point(3, 3);
            lines[53][2] = new Point(4, 4);
            lines[53][3] = new Point(5, 5);

            lines[54][0] = new Point(3, 0);
            lines[54][1] = new Point(4, 1);
            lines[54][2] = new Point(5, 2);
            lines[54][3] = new Point(6, 3);

            lines[55][0] = new Point(3, 1);
            lines[55][1] = new Point(4, 2);
            lines[55][2] = new Point(5, 3);
            lines[55][3] = new Point(6, 4);

            lines[56][0] = new Point(3, 2);
            lines[56][1] = new Point(4, 3);
            lines[56][2] = new Point(5, 4);
            lines[56][3] = new Point(6, 5);

            // diagonal lines - right to left, bottom to top
            lines[57][0] = new Point(6, 0);
            lines[57][1] = new Point(5, 1);
            lines[57][2] = new Point(4, 2);
            lines[57][3] = new Point(3, 3);

            lines[58][0] = new Point(6, 1);
            lines[58][1] = new Point(5, 2);
            lines[58][2] = new Point(4, 3);
            lines[58][3] = new Point(3, 4);

            lines[59][0] = new Point(6, 2);
            lines[59][1] = new Point(5, 3);
            lines[59][2] = new Point(4, 4);
            lines[59][3] = new Point(3, 5);

            lines[60][0] = new Point(5, 0);
            lines[60][1] = new Point(4, 1);
            lines[60][2] = new Point(3, 2);
            lines[60][3] = new Point(2, 3);

            lines[61][0] = new Point(5, 1);
            lines[61][1] = new Point(4, 2);
            lines[61][2] = new Point(3, 3);
            lines[61][3] = new Point(2, 4);

            lines[62][0] = new Point(5, 2);
            lines[62][1] = new Point(4, 3);
            lines[62][2] = new Point(3, 4);
            lines[62][3] = new Point(2, 5);

            lines[63][0] = new Point(4, 0);
            lines[63][1] = new Point(3, 1);
            lines[63][2] = new Point(2, 2);
            lines[63][3] = new Point(1, 3);

            lines[64][0] = new Point(4, 1);
            lines[64][1] = new Point(3, 2);
            lines[64][2] = new Point(2, 3);
            lines[64][3] = new Point(1, 4);

            lines[65][0] = new Point(4, 2);
            lines[65][1] = new Point(3, 3);
            lines[65][2] = new Point(2, 4);
            lines[65][3] = new Point(1, 5);

            lines[66][0] = new Point(3, 0);
            lines[66][1] = new Point(2, 1);
            lines[66][2] = new Point(1, 2);
            lines[66][3] = new Point(0, 3);

            lines[67][0] = new Point(3, 1);
            lines[67][1] = new Point(2, 2);
            lines[67][2] = new Point(1, 3);
            lines[67][3] = new Point(0, 4);

            lines[68][0] = new Point(3, 2);
            lines[68][1] = new Point(2, 3);
            lines[68][2] = new Point(1, 4);
            lines[68][3] = new Point(0, 5);
        }
    }
}
