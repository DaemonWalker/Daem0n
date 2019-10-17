using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Daem0n.Sudoku.CoreWinform
{
    class SudokuButton : Button
    {
        public int X { get; set; }
        public int Y { get; set; }
        public SudokuButton(int num = 0)
        {
            this.Text = num.ToString();
            this.Click += (sender, args) =>
            {
                var btn = sender as SudokuButton;
                btn.Number = (btn.Number + 1) % 9;
            };
        }
        public int Number
        {
            get => Convert.ToInt32(this.Text);
            set => this.Text = value.ToString();
        }
    }
}
