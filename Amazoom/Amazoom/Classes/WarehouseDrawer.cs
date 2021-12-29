using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Amazoom.Classes
{

    public class WarehouseDrawer
    {
        // Get path of robot picture
        private const string RobbieFileName = "Robbie.png";
        private const string DeadRobbieFileName = "Robbie_dead.png";
        private string RobbiePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, @"Amazoom\Resources\", RobbieFileName);
        private string DeadRobbiePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, @"Amazoom\Resources\", DeadRobbieFileName);

        // Initilize variables
        private PictureBox pictureBox;
        private Warehouse warehouse;
        private int offset = 2; // offset between rectangles
        public int NumberOfxCells;
        public int NumberOfyCells;
        private int cellSize;

        public WarehouseDrawer(PictureBox pb, Warehouse Warehouse)
        {
            // Picture box that will draw
            pictureBox = pb;
            // Warehouse being draw
            warehouse = Warehouse;

            // Warehouse dimension
            NumberOfxCells = Warehouse.warehouseSize.x;
            NumberOfyCells = Warehouse.warehouseSize.y;

            // Get the value of the cell size for the picture box
            cellSize = pictureBox.Width / NumberOfxCells;
            Warehouse.cellSize = cellSize;
        }

        // Draw elements of warehouse
        public void Draw(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Resize picture box so it is perfectly flush
            pictureBox.Height = cellSize * NumberOfyCells;
            pictureBox.Width = cellSize * NumberOfxCells;

            DrawWarehouse(g);
            DrawRobots(g);
            DrawRacks(g);

        }

        // Draw the grid in LightBlue
        public void DrawWarehouse(Graphics g)
        {
            for (var x = 0; x < NumberOfxCells; x++)
            {
                for (var y = 0; y < NumberOfyCells; y++)
                {
                    g.FillRectangle(Brushes.LightBlue, GetRectangleAt(x, y));
                }
            }
            g.FillRectangle(Brushes.Yellow, GetRectangleAt(warehouse.loadingStation.x, warehouse.loadingStation.y));
        }

        // draw robots with the image in resourses. Raises a exception if the robot is out of bounds
        public void DrawRobots(Graphics g)
        {
            foreach (Robot r in warehouse.robots)
            {
                try
                {
                    if (r.position.x > NumberOfxCells || r.position.x < 0 || r.position.y > NumberOfyCells || r.position.y < 0)
                    {
                        throw new ArgumentException("Out of bounds");
                    }
                    else
                    {
                        Image image = new Bitmap(RobbiePath);
                        if (r.batteryLevel <= 0){
                            image = new Bitmap(DeadRobbiePath);
                        }
                        g.FillRectangle(Brushes.Green, GetRectangleAt(r.chargingStationPosition.x, r.chargingStationPosition.y));
                        g.DrawImage(image, GetRectangleAt(r.position.x, r.position.y));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Environment.Exit(1);
                }
            }
        }

        // Draw racks in Blue
        public void DrawRacks(Graphics g)
        {
            foreach (Rack r in warehouse.racks)
            {
                for (int i = 0; i < r.length; i++)
                {

                    g.FillRectangle(Brushes.Blue, GetRectangleAt(r.startPosition.x, r.startPosition.y + i));

                }
                DrawItems(g, r);
            }
        }

        // Draw items in black circles
        public void DrawItems(Graphics g, Rack rack)
        {
            // Draw
            foreach (Item item in rack.items)
            {
                foreach(itemCoordinate pos in item.itemCoord_list)
                {
                    g.FillEllipse(Brushes.Black, GetRectangleAt(pos.x, pos.y));
                }
            }
        }

        private Rectangle GetRectangleAt(int x, int y)
        {
            return new Rectangle(x * cellSize, y * cellSize, cellSize - offset, cellSize - offset);
        }

    }
}
