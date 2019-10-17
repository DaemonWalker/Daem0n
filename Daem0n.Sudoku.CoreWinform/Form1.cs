using Daem0n.Sudoku.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Daem0n.Sudoku.CoreWinform
{
    public partial class Form1 : Form
    {
        private SudokuHelper sudokuHelper;
        public SudokuTable sudokuTable;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sudokuTable = new SudokuTable();
            sudokuTable.Dock = DockStyle.Fill;

            var panel1 = new Panel();
            panel1.Dock = DockStyle.Left;
            panel1.Width = sudokuTable.BtnSize * 9 + sudokuTable.BtnMargin * 10;

            var panel2 = new Panel();
            panel2.Dock = DockStyle.Fill;

            var btn = new Button();
            btn.Text = "获取解答";
            btn.Width = 80;
            btn.Height = 30;
            btn.Top = 30;
            btn.Width = 30;
            btn.Click += Btn_Click;

            panel1.Controls.Add(sudokuTable);
            panel2.Controls.Add(btn);
            this.Controls.Add(panel2);
            this.Controls.Add(panel1);

            sudokuTable.Select();
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            sudokuHelper = new SudokuHelper(sudokuTable.DataSource);
            sudokuTable.DataSource = sudokuHelper.Run().Map;
        }
    }
}
