using Daem0n.Sudoku.Core;
using System;

namespace Daem0n.Sudoku.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            //var map = new int[9, 9]
            //{
            //    { 0,8,0,0,0,0,6,0,0},
            //    { 0,0,0,4,0,0,0,0,9},
            //    { 0,7,0,0,0,0,8,0,5},
            //    { 4,0,0,0,0,0,0,0,0},
            //    { 0,3,0,0,6,0,0,9,0},
            //    { 0,0,0,7,2,0,1,0,0},
            //    { 0,9,3,2,0,0,0,6,4},
            //    { 8,1,0,3,0,0,0,0,0},
            //    { 0,0,0,0,0,5,0,0,0}
            //};
            var map = new string[]
            {
                "800074000",
                "050600003",
                "000200000",
                "900500040",
                "000140060",
                "000006800",
                "000902056",
                "062000900",
                "070001200"
            };
            var numMap = new int[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int k = 0; k < 9; k++)
                {
                    numMap[i, k] = Convert.ToInt32(map[i][k].ToString());
                }
            }
            var sudoku = new SudokuHelper(numMap);
            sudoku.Run();
            for (int i = 0; i < 9; i++)
            {
                for (int k = 0; k < 9; k++)
                {
                    Console.Write($"{sudoku.Map[i, k]}\t");
                }
                Console.WriteLine();
            }
        }
    }
}
