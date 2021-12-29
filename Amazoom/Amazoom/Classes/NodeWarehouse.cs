using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
    public class NodeWarehouse
    {

        public Coordinate warehouseSize { get; set; }
        public Node[,] gridNode { get; set; }

        public Stack<Node> path { get; set; }
        public List<Rack> Racks { get; set; }
        private Warehouse warehouse;

        public NodeWarehouse(Warehouse _warehouse)
        {
            warehouse = _warehouse;
            warehouseSize = warehouse.warehouseSize;
            Racks = warehouse.racks;
            CreateGrid();
        }

        private void CreateGrid()
        {
            gridNode = new Node[warehouseSize.x, warehouseSize.y];

            for (int x = 0; x < warehouseSize.x; x++)
            {
                for (int y = 0; y < warehouseSize.y; y++)
                {
                    gridNode[x, y] = new Node(true, x, y);
                }
            }
            // set the racks as not walkable
            foreach (Rack rack in Racks)
            {
                for (int i = 0; i < rack.length; i++)
                {
                    gridNode[rack.startPosition.x, rack.startPosition.y + i].walkable = false;
                }
            }
         
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.x + x;
                    int checkY = node.y + y;

                    if (checkX >= 0 && checkX < warehouseSize.x && checkY >= 0 && checkY < warehouseSize.y)
                    {
                        neighbours.Add(gridNode[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }
    }
}
