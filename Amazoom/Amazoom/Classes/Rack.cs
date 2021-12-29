using Amazoom.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
    public class Rack
    {
        // name of the rack
        public string name;
        // Start position 
        public Coordinate startPosition;
        //public bool horizontal { get; set; } // 1- horizontal, 0 vertical
        public int length;

        // variables for 3d and item placement
        public int max_weight_shelf;
        public int max_shelf;

        public bool aisle_side = true;
        public List<Item> items { get; set; } = new List<Item>();
        public Warehouse warehouse;

        public Item[,] itemGrid {get; set;}

        public Rack(string _name, Coordinate _startPosition, int _length, Warehouse _warehouese, int _max_shelf = 6, int _max_weight = 100)
        {
            itemGrid = new Item[_length,_max_shelf];
            name = _name;
            startPosition = _startPosition;
            length = _length;
            warehouse = _warehouese;

            max_shelf = _max_shelf;
            max_weight_shelf = _max_weight;
        }

        // initialize the locations in the warehouse
        public void AddItem(Item item, int quantity)
        {
            // number of items variables
            int numItems = quantity;
            int numItemsLeft = numItems;
            // aisle side
            bool aisle_side = true;

            Coordinate pos = FindPositionAvailable();

            if( pos != null)
            {
                if (item.weight * numItems < max_weight_shelf)
                {
                    item.itemCoord_list.Add(new itemCoordinate(pos.x, pos.y, pos.z, numItems, aisle_side));
                    itemGrid[pos.y - warehouse.rowAtBotton, pos.z] = item;
                    items.Add(item);
                }
                else
                {
                    int maxQty = max_weight_shelf/item.weight;
                    if(maxQty == 0)
                    {
                        Console.WriteLine(" Item is to heavy for the shelf");
                        return;
                    }
                    item.itemCoord_list.Add(new itemCoordinate(pos.x, pos.y, pos.z, maxQty, aisle_side));
                    itemGrid[pos.y - warehouse.rowAtBotton, pos.z] = item;
                    AddItem(item, numItems-maxQty);
                }
            }
            else
            {
                warehouse.warningMessage.Text = "No spots available";
                Console.WriteLine("No spots available");
            }
        }
        
        public void AddInitializedItem(Item item, int yRackPos, int zRackPos, int quantity, bool aisle_side)
        {
            int xPos = startPosition.x;
            int yPos = yRackPos + warehouse.rowAtBotton;
            int zPos = zRackPos;
            int numItems = quantity;

            item.itemCoord_list.Add(new itemCoordinate(xPos, yPos, zPos, numItems, aisle_side));
            itemGrid[yPos - warehouse.rowAtBotton, zPos] = item;
            items.Add(item);
        }

        Coordinate FindPositionAvailable(){
            
            for(int y = 0; y < length; y++)
            {
                for(int z = 0; z < max_shelf; z++)
                {
                    if(itemGrid[y,z] == null)
                    {
                        return new Coordinate(startPosition.x, y+ warehouse.rowAtBotton, z);
                    }
                }
            }
            return null;
        }

    }
}
