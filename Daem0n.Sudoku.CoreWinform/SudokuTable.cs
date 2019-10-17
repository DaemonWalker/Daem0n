#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Daem0n.Sudoku.CoreWinform
{
    public class SudokuTable : UserControl
    {
        private SudokuButton[,] sudokuButtons;
        private int[,] dataSource;
        public int BtnSize { get; set; } = 50;
        public int BtnMargin { get; set; } = 10;
        public SudokuTable()
        {
            sudokuButtons = new SudokuButton[9, 9];
            dataSource = new int[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int k = 0; k < 9; k++)
                {
                    var btn = new SudokuButton();
                    btn.X = i;
                    btn.Y = k;
                    sudokuButtons[i, k] = btn;
                    btn.Width = this.BtnSize;
                    btn.Height = this.BtnSize;
                    btn.Top = (i + 1) * this.BtnMargin + i * this.BtnSize;
                    btn.Left = (k + 1) * this.BtnMargin + k * this.BtnSize;
                    btn.KeyDown += (sender, args) =>
                    {
                        var btn = sender as SudokuButton;
                        var keyCode = (int)args.KeyCode - 48;
                        if (0 <= keyCode && keyCode <= 9)
                        {
                            btn.Number = keyCode;
                            this.dataSource[btn.X, btn.Y] = keyCode;
                            sudokuButtons[btn.X + (btn.Y + 1) / 9, (btn.Y + 1) % 9].Select();
                        }
                    };
                    this.Controls.Add(btn);
                }
            }

            this.GotFocus += (sender, args) =>
            {
                var table = sender as SudokuTable;
                table.sudokuButtons[0, 0].Select();
            };
        }

        public int[,] DataSource
        {
            get => this.dataSource;
            set
            {
                this.dataSource = value;
                for (int i = 0; i < 9; i++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        sudokuButtons[i, k].Number = value[i, k];
                    }
                }
            }
        }



    }
}
#nullable restore