using Daem0n.Engine.TableGame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Daem0n.Game.Llk
{
    public partial class FrmGame : Form
    {
        private List<Image> images = new List<Image>();
        private TableCellMap gameMap;
        private Random rand = new Random();
        public FrmGame()
        {
            InitializeComponent();
            this.Controls.Add(this.gamePanel);
            this.Controls.Add(this.topPanel);
            this.gamePanel.Controls.Add(this.table);
            this.WindowState = FormWindowState.Maximized;
            this.table.CellClickEvent += (obj, args) =>
            {
                gameMap[args.X, args.Y] = new TableCell() { Image = images[rand.Next(10)] };
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadResource();
            var map = new TableCell[10, 15];
            for (var x = 0; x < 10; x++)
            {
                for (var y = 0; y < 15; y++)
                {
                    var cell = new TableCell() { Image = images[rand.Next(images.Count)] };
                    map[x, y] = cell;
                }
            }
            gameMap = new TableCellMap();
            table.SetMap(gameMap);
            gameMap.DataSource = map;
        }
        private void LoadResource()
        {
            var dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "images"));
            foreach (var file in dir.GetFiles())
            {
                images.Add(Image.FromFile(file.FullName));
            }
        }
    }
}
