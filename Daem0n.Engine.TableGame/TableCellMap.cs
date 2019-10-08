using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.Engine.TableGame
{
    public class TableCellMap
    {

        private TableCell[,] dataSource;
        public Action<CellChangeEventArgs> SingleCellChangeEvent { get; set; }
        public Action<MapChangeArgs> Reset { get; set; }
        public int Height => this.dataSource.GetLength(1);
        public int Width => this.dataSource.GetLength(0);

        public TableCell[,] DataSource
        {
            get => this.dataSource;
            set
            {
                this.dataSource = value;
                Reset?.Invoke(new MapChangeArgs() { TableCellMap = this });
            }
        }
        public TableCell this[int x, int y]
        {
            get => this.dataSource[x, y];
            set
            {
                var args = new CellChangeEventArgs()
                {
                    X = x,
                    Y = y,
                    OldCell = this.dataSource[x, y],
                    NewCell = value
                };
                this.SingleCellChangeEvent?.Invoke(args);
                this.dataSource[x, y] = value;
            }
        }
    }
}
