using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.Engine.TableGame
{
    public class CellChangeEventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TableCell OldCell { get; set; }
        public TableCell NewCell { get; set; }
    }
}
