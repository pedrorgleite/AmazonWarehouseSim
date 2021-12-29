using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
    public class itemCoordinate
    {
        // x,y,z coordinate cariables
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }

        // additional item coordinate variables
        public int num_items { get; set; }
        public bool aisle_side { get; set; }

        public itemCoordinate(int x, int y, int z, int num_items, bool aisle_side)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.num_items = num_items;
            this.aisle_side = aisle_side;
        }
    }
}