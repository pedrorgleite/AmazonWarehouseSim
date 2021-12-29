using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
    public class Coordinate
    {
        // x,y,z coordinate cariables
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }

        // additional item coordinate variables
        public int num_items { get; set; }
        public int aisle_side { get; set; }

        public Coordinate(int x = 0, int y = 0, int z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
         
        /*public itemCoordinate(int x, int y, int z, int num_items, int aisle_side)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.num_items = num_items;
            this.aisle_side = aisle_side;
        }*/
    }
}