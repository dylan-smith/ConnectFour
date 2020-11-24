using System.Collections.Generic;
using System.Drawing;

namespace ConnectFour.Interfaces
{
    public static class WinningLines
    {
        private static List<WinningLine> lines;

        private static Dictionary<Point, List<WinningLine>> linesByPoint;

        public static void Initialize()
        {
            InitializeLines();
            InitializeLinesByPoint();
        }
        
        public static IEnumerable<WinningLine> GetAllWinningLines()
        {
            return lines;
        }

        public static IEnumerable<WinningLine> GetLinesByPoint(int x, int y)
        {
            return linesByPoint[new Point(x, y)];
        }

        private static void InitializeLinesByPoint()
        {
            linesByPoint = new Dictionary<Point, List<WinningLine>>();

            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    var pointLines = new List<WinningLine>();

                    foreach (var l in lines)
                    {
                        foreach (var p in l)
                        {
                            if (p.X == x && p.Y == y)
                            {
                                pointLines.Add(l);
                                break;
                            }
                        }
                    }

                    linesByPoint.Add(new Point(x, y), pointLines);
                }
            }
        }

        private static void InitializeLines()
        {
            lines = new List<WinningLine>(69);

            for (int line = 0; line < 69; line++)
            {
                lines.Add(new WinningLine());
            }

            // Horizontal Lines
            lines[0].Add(new Point(0, 0));
            lines[0].Add(new Point(1, 0));
            lines[0].Add(new Point(2, 0));
            lines[0].Add(new Point(3, 0));

            lines[1].Add(new Point(1, 0));
            lines[1].Add(new Point(2, 0));
            lines[1].Add(new Point(3, 0));
            lines[1].Add(new Point(4, 0));

            lines[2].Add(new Point(2, 0));
            lines[2].Add(new Point(3, 0));
            lines[2].Add(new Point(4, 0));
            lines[2].Add(new Point(5, 0));

            lines[3].Add(new Point(3, 0));
            lines[3].Add(new Point(4, 0));
            lines[3].Add(new Point(5, 0));
            lines[3].Add(new Point(6, 0));

            lines[4].Add(new Point(0, 1));
            lines[4].Add(new Point(1, 1));
            lines[4].Add(new Point(2, 1));
            lines[4].Add(new Point(3, 1));

            lines[5].Add(new Point(1, 1));
            lines[5].Add(new Point(2, 1));
            lines[5].Add(new Point(3, 1));
            lines[5].Add(new Point(4, 1));

            lines[6].Add(new Point(2, 1));
            lines[6].Add(new Point(3, 1));
            lines[6].Add(new Point(4, 1));
            lines[6].Add(new Point(5, 1));

            lines[7].Add(new Point(3, 1));
            lines[7].Add(new Point(4, 1));
            lines[7].Add(new Point(5, 1));
            lines[7].Add(new Point(6, 1));

            lines[8].Add(new Point(0, 2));
            lines[8].Add(new Point(1, 2));
            lines[8].Add(new Point(2, 2));
            lines[8].Add(new Point(3, 2));

            lines[9].Add(new Point(1, 2));
            lines[9].Add(new Point(2, 2));
            lines[9].Add(new Point(3, 2));
            lines[9].Add(new Point(4, 2));

            lines[10].Add(new Point(2, 2));
            lines[10].Add(new Point(3, 2));
            lines[10].Add(new Point(4, 2));
            lines[10].Add(new Point(5, 2));

            lines[11].Add(new Point(3, 2));
            lines[11].Add(new Point(4, 2));
            lines[11].Add(new Point(5, 2));
            lines[11].Add(new Point(6, 2));

            lines[12].Add(new Point(0, 3));
            lines[12].Add(new Point(1, 3));
            lines[12].Add(new Point(2, 3));
            lines[12].Add(new Point(3, 3));

            lines[13].Add(new Point(1, 3));
            lines[13].Add(new Point(2, 3));
            lines[13].Add(new Point(3, 3));
            lines[13].Add(new Point(4, 3));

            lines[14].Add(new Point(2, 3));
            lines[14].Add(new Point(3, 3));
            lines[14].Add(new Point(4, 3));
            lines[14].Add(new Point(5, 3));

            lines[15].Add(new Point(3, 3));
            lines[15].Add(new Point(4, 3));
            lines[15].Add(new Point(5, 3));
            lines[15].Add(new Point(6, 3));

            lines[16].Add(new Point(0, 4));
            lines[16].Add(new Point(1, 4));
            lines[16].Add(new Point(2, 4));
            lines[16].Add(new Point(3, 4));

            lines[17].Add(new Point(1, 4));
            lines[17].Add(new Point(2, 4));
            lines[17].Add(new Point(3, 4));
            lines[17].Add(new Point(4, 4));

            lines[18].Add(new Point(2, 4));
            lines[18].Add(new Point(3, 4));
            lines[18].Add(new Point(4, 4));
            lines[18].Add(new Point(5, 4));

            lines[19].Add(new Point(3, 4));
            lines[19].Add(new Point(4, 4));
            lines[19].Add(new Point(5, 4));
            lines[19].Add(new Point(6, 4));

            lines[20].Add(new Point(0, 5));
            lines[20].Add(new Point(1, 5));
            lines[20].Add(new Point(2, 5));
            lines[20].Add(new Point(3, 5));

            lines[21].Add(new Point(1, 5));
            lines[21].Add(new Point(2, 5));
            lines[21].Add(new Point(3, 5));
            lines[21].Add(new Point(4, 5));

            lines[22].Add(new Point(2, 5));
            lines[22].Add(new Point(3, 5));
            lines[22].Add(new Point(4, 5));
            lines[22].Add(new Point(5, 5));

            lines[23].Add(new Point(3, 5));
            lines[23].Add(new Point(4, 5));
            lines[23].Add(new Point(5, 5));
            lines[23].Add(new Point(6, 5));

            // Vertical lines            
            lines[24].Add(new Point(0, 0));
            lines[24].Add(new Point(0, 1));
            lines[24].Add(new Point(0, 2));
            lines[24].Add(new Point(0, 3));

            lines[25].Add(new Point(0, 1));
            lines[25].Add(new Point(0, 2));
            lines[25].Add(new Point(0, 3));
            lines[25].Add(new Point(0, 4));

            lines[26].Add(new Point(0, 2));
            lines[26].Add(new Point(0, 3));
            lines[26].Add(new Point(0, 4));
            lines[26].Add(new Point(0, 5));

            lines[27].Add(new Point(1, 0));
            lines[27].Add(new Point(1, 1));
            lines[27].Add(new Point(1, 2));
            lines[27].Add(new Point(1, 3));

            lines[28].Add(new Point(1, 1));
            lines[28].Add(new Point(1, 2));
            lines[28].Add(new Point(1, 3));
            lines[28].Add(new Point(1, 4));

            lines[29].Add(new Point(1, 2));
            lines[29].Add(new Point(1, 3));
            lines[29].Add(new Point(1, 4));
            lines[29].Add(new Point(1, 5));

            lines[30].Add(new Point(2, 0));
            lines[30].Add(new Point(2, 1));
            lines[30].Add(new Point(2, 2));
            lines[30].Add(new Point(2, 3));

            lines[31].Add(new Point(2, 1));
            lines[31].Add(new Point(2, 2));
            lines[31].Add(new Point(2, 3));
            lines[31].Add(new Point(2, 4));

            lines[32].Add(new Point(2, 2));
            lines[32].Add(new Point(2, 3));
            lines[32].Add(new Point(2, 4));
            lines[32].Add(new Point(2, 5));

            lines[33].Add(new Point(3, 0));
            lines[33].Add(new Point(3, 1));
            lines[33].Add(new Point(3, 2));
            lines[33].Add(new Point(3, 3));

            lines[34].Add(new Point(3, 1));
            lines[34].Add(new Point(3, 2));
            lines[34].Add(new Point(3, 3));
            lines[34].Add(new Point(3, 4));

            lines[35].Add(new Point(3, 2));
            lines[35].Add(new Point(3, 3));
            lines[35].Add(new Point(3, 4));
            lines[35].Add(new Point(3, 5));

            lines[36].Add(new Point(4, 0));
            lines[36].Add(new Point(4, 1));
            lines[36].Add(new Point(4, 2));
            lines[36].Add(new Point(4, 3));

            lines[37].Add(new Point(4, 1));
            lines[37].Add(new Point(4, 2));
            lines[37].Add(new Point(4, 3));
            lines[37].Add(new Point(4, 4));

            lines[38].Add(new Point(4, 2));
            lines[38].Add(new Point(4, 3));
            lines[38].Add(new Point(4, 4));
            lines[38].Add(new Point(4, 5));

            lines[39].Add(new Point(5, 0));
            lines[39].Add(new Point(5, 1));
            lines[39].Add(new Point(5, 2));
            lines[39].Add(new Point(5, 3));

            lines[40].Add(new Point(5, 1));
            lines[40].Add(new Point(5, 2));
            lines[40].Add(new Point(5, 3));
            lines[40].Add(new Point(5, 4));

            lines[41].Add(new Point(5, 2));
            lines[41].Add(new Point(5, 3));
            lines[41].Add(new Point(5, 4));
            lines[41].Add(new Point(5, 5));

            lines[42].Add(new Point(6, 0));
            lines[42].Add(new Point(6, 1));
            lines[42].Add(new Point(6, 2));
            lines[42].Add(new Point(6, 3));

            lines[43].Add(new Point(6, 1));
            lines[43].Add(new Point(6, 2));
            lines[43].Add(new Point(6, 3));
            lines[43].Add(new Point(6, 4));

            lines[44].Add(new Point(6, 2));
            lines[44].Add(new Point(6, 3));
            lines[44].Add(new Point(6, 4));
            lines[44].Add(new Point(6, 5));

            // diagonal lines - left to right, bottom to top
            lines[45].Add(new Point(0, 0));
            lines[45].Add(new Point(1, 1));
            lines[45].Add(new Point(2, 2));
            lines[45].Add(new Point(3, 3));

            lines[46].Add(new Point(0, 1));
            lines[46].Add(new Point(1, 2));
            lines[46].Add(new Point(2, 3));
            lines[46].Add(new Point(3, 4));

            lines[47].Add(new Point(0, 2));
            lines[47].Add(new Point(1, 3));
            lines[47].Add(new Point(2, 4));
            lines[47].Add(new Point(3, 5));

            lines[48].Add(new Point(1, 0));
            lines[48].Add(new Point(2, 1));
            lines[48].Add(new Point(3, 2));
            lines[48].Add(new Point(4, 3));

            lines[49].Add(new Point(1, 1));
            lines[49].Add(new Point(2, 2));
            lines[49].Add(new Point(3, 3));
            lines[49].Add(new Point(4, 4));

            lines[50].Add(new Point(1, 2));
            lines[50].Add(new Point(2, 3));
            lines[50].Add(new Point(3, 4));
            lines[50].Add(new Point(4, 5));

            lines[51].Add(new Point(2, 0));
            lines[51].Add(new Point(3, 1));
            lines[51].Add(new Point(4, 2));
            lines[51].Add(new Point(5, 3));

            lines[52].Add(new Point(2, 1));
            lines[52].Add(new Point(3, 2));
            lines[52].Add(new Point(4, 3));
            lines[52].Add(new Point(5, 4));

            lines[53].Add(new Point(2, 2));
            lines[53].Add(new Point(3, 3));
            lines[53].Add(new Point(4, 4));
            lines[53].Add(new Point(5, 5));

            lines[54].Add(new Point(3, 0));
            lines[54].Add(new Point(4, 1));
            lines[54].Add(new Point(5, 2));
            lines[54].Add(new Point(6, 3));

            lines[55].Add(new Point(3, 1));
            lines[55].Add(new Point(4, 2));
            lines[55].Add(new Point(5, 3));
            lines[55].Add(new Point(6, 4));

            lines[56].Add(new Point(3, 2));
            lines[56].Add(new Point(4, 3));
            lines[56].Add(new Point(5, 4));
            lines[56].Add(new Point(6, 5));

            // diagonal lines - right to left, bottom to top
            lines[57].Add(new Point(6, 0));
            lines[57].Add(new Point(5, 1));
            lines[57].Add(new Point(4, 2));
            lines[57].Add(new Point(3, 3));

            lines[58].Add(new Point(6, 1));
            lines[58].Add(new Point(5, 2));
            lines[58].Add(new Point(4, 3));
            lines[58].Add(new Point(3, 4));

            lines[59].Add(new Point(6, 2));
            lines[59].Add(new Point(5, 3));
            lines[59].Add(new Point(4, 4));
            lines[59].Add(new Point(3, 5));

            lines[60].Add(new Point(5, 0));
            lines[60].Add(new Point(4, 1));
            lines[60].Add(new Point(3, 2));
            lines[60].Add(new Point(2, 3));

            lines[61].Add(new Point(5, 1));
            lines[61].Add(new Point(4, 2));
            lines[61].Add(new Point(3, 3));
            lines[61].Add(new Point(2, 4));

            lines[62].Add(new Point(5, 2));
            lines[62].Add(new Point(4, 3));
            lines[62].Add(new Point(3, 4));
            lines[62].Add(new Point(2, 5));

            lines[63].Add(new Point(4, 0));
            lines[63].Add(new Point(3, 1));
            lines[63].Add(new Point(2, 2));
            lines[63].Add(new Point(1, 3));

            lines[64].Add(new Point(4, 1));
            lines[64].Add(new Point(3, 2));
            lines[64].Add(new Point(2, 3));
            lines[64].Add(new Point(1, 4));

            lines[65].Add(new Point(4, 2));
            lines[65].Add(new Point(3, 3));
            lines[65].Add(new Point(2, 4));
            lines[65].Add(new Point(1, 5));

            lines[66].Add(new Point(3, 0));
            lines[66].Add(new Point(2, 1));
            lines[66].Add(new Point(1, 2));
            lines[66].Add(new Point(0, 3));

            lines[67].Add(new Point(3, 1));
            lines[67].Add(new Point(2, 2));
            lines[67].Add(new Point(1, 3));
            lines[67].Add(new Point(0, 4));
                                        
            lines[68].Add(new Point(3, 2));
            lines[68].Add(new Point(2, 3));
            lines[68].Add(new Point(1, 4));
            lines[68].Add(new Point(0, 5));
        }
    }
}
