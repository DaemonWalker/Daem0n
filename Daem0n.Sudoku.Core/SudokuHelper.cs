using System;
using System.Collections.Generic;
using System.Linq;

namespace Daem0n.Sudoku.Core
{
    public class SudokuHelper
    {
        public int[,] Map { get; private set; }
        public SudokuHelper(int[,] map)
        {
            this.Map = map;
        }
        public SudokuHelper Run()
        {
            var stack = new Stack<int[,]>();
            stack.Push(this.Map);
            while (stack.Count > 0)
            {
                var tempMap = stack.Pop();
                if (PushStack(stack, tempMap))
                {
                    return this;
                }
            }
            throw new Exception("No Solution");
        }

        private int[,] CopyMap(int[,] map)
        {
            var tempMap = new int[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int k = 0; k < 9; k++)
                {
                    tempMap[i, k] = map[i, k];
                }
            }
            return tempMap;
        }
        private bool PushStack(Stack<int[,]> stack, int[,] map)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int k = 0; k < 9; k++)
                {
                    if (map[i, k] == 0)
                    {
                        var nums = GetLegalNum(map, i, k);
                        if (nums.Count() != 0)
                        {
                            foreach (var num in nums)
                            {
                                var newMap = CopyMap(map);
                                newMap[i, k] = num;
                                stack.Push(newMap);
                            }
                        }
                        return false;
                    }
                }
            }
            this.Map = map;
            return true;
        }
        private IEnumerable<int> GetLegalNum(int[,] map, int x, int y)
        {
            var pos = Enumerable.Range(0, 9);
            var showNum = pos.Select(_ => map[x, _]).ToList();
            showNum.AddRange(pos.Select(_ => map[_, y]));
            for (int i = x / 3 * 3; i < x / 3 * 3 + 3; i++)
            {
                for (int k = y / 3 * 3; k < y / 3 * 3 + 3; k++)
                {
                    showNum.Add(map[i, k]);
                }
            }
            var cases = Enumerable.Range(1, 9);
            return cases.Where(_ => showNum.Contains(_) == false);
        }
    }
}
