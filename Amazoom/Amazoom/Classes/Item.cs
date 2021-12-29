using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
    public class Item
    {
        // Name of the Product
        public string name;
        // Weight of each product
        public int weight { get; set; } = 0;
        // weigth limit of the shelf
        public int weightLimit { get; set; } = 100;
        // Limit of products based on the weigth and weigth limit
        public int limitOfProducts { get; set; } = 100;
        // Total number of items in stock
        public int number_available_items        
        { 
            get
            {
                int numb = 0;
                foreach(itemCoordinate item in itemCoord_list){
                    if(item.num_items>0)
                    {
                        numb += item.num_items;
                    }
                }
                return numb;
            }
        }

        public int number_available_items_current_pos {
            get
            {
                return currentItemPos.num_items;
            }
        }

        public int qtyFrontEnd {get; set;} = 0;

        // Position of the Item
        public Coordinate position 
        { 
            get
            {
                return new Coordinate(currentItemPos.x,currentItemPos.y,currentItemPos.z);
            }
        }
        
        public itemCoordinate currentItemPos 
        { 
            get
            {
                return itemCoord_list.OrderByDescending(item => item.num_items).First();
            }
        }

        //public itemCoordinate itemCoor { get; set; }
        public List<itemCoordinate> itemCoord_list = new List<itemCoordinate>();

        // Is the item at the right or left side of the rack right == true left == false
        public bool aisle_side;

        public Rack rack {get; set;}

        public int id;

        public Item(int id, string name, int weight, bool _aisle_side = true)
        {
            aisle_side = _aisle_side;
            this.id = id;
            this.name = name;
            this.weight = weight;
        }

        public override string ToString()
        {
            return this.name;
        } 
        public void SubtractItemQuantity(int quantity)
        {                
            foreach(itemCoordinate item in itemCoord_list)
            {
                if(item.num_items>0)
                {
                    if(quantity<item.num_items)
                    {
                        item.num_items -= quantity;
                        return;
                    }
                    else
                    {
                        item.num_items = 0;
                        SubtractItemQuantity(quantity - item.num_items);
                    }
                }
            }
            Console.WriteLine("Subtracting more item quantity than what it has");
        }


    }
}
