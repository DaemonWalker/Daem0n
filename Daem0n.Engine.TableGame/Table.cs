using System;
using System.Windows.Forms;

namespace Daem0n.Engine.TableGame
{
    public class Table : Control
    {
        private TableCellMap map;
        private Control[,] buttons;
        public int CellWidth { get; set; } = 80;
        public int CellHeight { get; set; } = 80;
        public int CellXSpacing { get; set; } = 5;
        public int CellYSpacing { get; set; } = 5;
        public int XBorder { get; set; } = 30;
        public int YBorder { get; set; } = 30;
        public event Action<object, TableCellClickEventArgs> CellClickEvent;
        public void SetMap(TableCellMap map)
        {
            map.SingleCellChangeEvent = _ =>
            {
                SetBtn(_.X, _.Y, _.NewCell);
            };
            map.Reset = _ =>
            {
                if (this.buttons != null)
                {
                    foreach (var btn in this.buttons)
                    {
                        btn.Dispose();
                    }
                    this.buttons = null;
                }
                this.buttons = new Control[_.TableCellMap.Width, _.TableCellMap.Height];
                for (var x = 0; x < _.TableCellMap.Width; x++)
                {
                    for (var y = 0; y < _.TableCellMap.Height; y++)
                    {
                        SetBtn(x, y, _.TableCellMap[x, y]);
                    }
                }
            };
            this.map = map;
        }

        public void SetCell(int x, int y, TableCell cell)
        {

        }
        private void SetBtn(int x, int y, TableCell cell)
        {
            this.buttons[x, y]?.Dispose();
            var btn = new CheckBox();
            //btn.Image = cell.Image;
            btn.TextImageRelation = TextImageRelation.TextAboveImage;
            btn.BackgroundImage = cell.Image;
            btn.Width = this.CellWidth;
            btn.Height = this.CellHeight;
            this.buttons[x, y] = btn;
            btn.Top = XBorder + x * (CellHeight + CellXSpacing);
            btn.Left = YBorder + y * (CellWidth + CellYSpacing);
            btn.Appearance = Appearance.Button;
            btn.Click += (obj, args) =>
            {
                CellClickEvent?.Invoke(obj, new TableCellClickEventArgs() { X = x, Y = y });
            };
            this.Controls.Add(btn);
        }
    }
}
