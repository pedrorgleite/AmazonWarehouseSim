using Amazoom.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Amazoom.Classes
{
    public class Warehouse
    {
        //================================================================================//
        //                              Variable declarations                             //
        //================================================================================//
        public ListBox itemListBox { get; set; }
        public ListBox ordersListBox { get; set; }
        public ListBox ordersTobeDeliveredListBox { get; set; }
        public ListBox itemList { get; set; }
        public Label warningMessage { get; set; }


        public WarehouseViewer wrviewer { get; set; }
        // The size of the warehouse in nodes
        public Coordinate warehouseSize;

        public string Name; 

        // Position of the loading station
        public Coordinate loadingStation { get; set; } 

        // List of robots in the warehouse
        public List<Robot> robots { get; set; } = new List<Robot>();

        // List of shelfs in the warehouse. set automaticaly with the warehousesize
        public List<Rack> racks { get; set; }

        // List of items in the warehouse. set automaticaly with the warehousesize
        public List<Item> items
        {
            get
            {
                List<Item> _items = new List<Item>();
                foreach (Rack rack in racks)
                {
                    _items.AddRange(rack.items);
                }
                return _items;
            }
        }

        public List<Job> Orders 
        {
            get
            {
                return GetOrders(GetAllJobs());
            }
        }

        // Max items based on the number of racks
        public int maxItems
        {
            get
            {
                int rackLength = warehouseSize.y - rowAtBotton - rowAtTop;
                return racks.Count() * rackLength;
            }
        }

        // Drawer methode
        public WarehouseDrawer warehouseDrawer;

        // Grid node of the warehoues
        public NodeWarehouse nodeWarehouse;

        //public NodeWarehouse nodeWarehouse { get; set; }
        public int cellSize { get; set; }

        public int numberOfOrders = 0;
        //================================================================================//
        //                              Class Methods                                     //
        //================================================================================//

        public Warehouse(PictureBox pb, Coordinate _warehouseSize)
        {
            warehouseSize = _warehouseSize;
            InitilizeWarehouse();
            warehouseDrawer = new WarehouseDrawer(pb, this);
            nodeWarehouse = new NodeWarehouse(this);
        }

        // Delivery Item using a robot that is not busy or queue on the jobs of the robots
        private int nextRobotToEnque = 0;
        public void DeliveryItem(Item item, int quantity, string clientName)
        {
            bool allRobotsAreBusy = true;

            item = FindItem(item.name); //Populate the item with the information from the warehouse computer

            //Makes sure no job is too heavy for a single robot
            //THis is assuming every robot has the same weight capacity
            if (item.weight * quantity > robots[0].weightLimit)
            {
                try
                {
                    if (item.weight > robots[0].weightLimit)
                    {
                        throw new ArgumentException("Item is too heavy for a robot");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Environment.Exit(1);
                }
                //Spilts the quantity into two jobs if its too big for one robot
                //the math functions ensures the correct integer number of items gets delivered
                int newQuantitiy1 = (int)Math.Ceiling(quantity / 2.0);
                int newQuantitiy2 = (int)Math.Floor(quantity / 2.0);
                DeliveryItem(item, newQuantitiy1, clientName);
                DeliveryItem(item, newQuantitiy2, clientName);

            }

            foreach (Robot rob in robots)
            {
                if (!rob.isBusy)
                {
                    rob.GetItem(item, quantity, clientName, numberOfOrders);
                    allRobotsAreBusy = false;
                    break;
                }
            }          
            
            // If all robots are busy enqueue paths into jobs
            if (allRobotsAreBusy)
            {
                Robot robot = robots[nextRobotToEnque % robots.Count()];
                nextRobotToEnque++;
                robot.GetItem(item, quantity, clientName, numberOfOrders);
            }
            numberOfOrders++;
        }

        public void RestockItem(Item item, int quantity)
        {
            bool allRobotsAreBusy = true;

            foreach (Robot rob in robots)
            {
                if (!rob.isBusy)
                {
                    rob.RestockItem(item, quantity);
                    allRobotsAreBusy = false;
                    break;
                }
            }

            // If all robots are busy enqueue paths into jobs
            if (allRobotsAreBusy)
            {
                Robot robot = robots[nextRobotToEnque % robots.Count()];
                nextRobotToEnque++;
                robot.RestockItem(item, quantity);
            }
        }

        //
        //  Helper functions
        //

        // Create the racks of the warehouse based on the warehouse size
        public int rowAtBotton = 3;
        public int rowAtTop = 3;
        public int colsOnSides = 2;

        private void InitilizeWarehouse()
        {
            racks = new List<Rack>();
            int rackLength = warehouseSize.y - rowAtBotton - rowAtTop;

            // Since each the racks area occuppy 2 cols one for the rack and one for the hall 
            // the number of racks is  (cols - empty cols on the sides -1)/2 = number of racks
            for (int i = 0; i < (warehouseSize.x - colsOnSides - 1) / 2; i++)
            {
                racks.Add(new Rack(IntToCol(i + 1), new Coordinate(i*2 + colsOnSides, rowAtBotton), rackLength, this));
            }
            loadingStation = new Coordinate(warehouseSize.x - 1, warehouseSize.y - 1);
        }


        // Draw this warehouse
        public void Draw(PaintEventArgs e)
        {
            warehouseDrawer.Draw(e);
        }

        // helper functions
        private static string IntToCol(int value)
        {
            string result = string.Empty;
            while (--value >= 0)
            {
                result = (char)('A' + value % 26) + result;
                value /= 26;
            }
            return result;
        }

        public void AddInitializedItem(Item item, Rack rack, int yRackPos, int zRackPos, int quantity, bool aisle_side)
        {
            itemListBox.Items.Add(item);
            itemListBox.DisplayMember = "name";
            item.rack = rack;
            rack.AddInitializedItem(item, yRackPos, zRackPos, quantity, aisle_side);
        }

        public void AddItem(Item item, int quantity)
        {
            itemListBox.Items.Add(item);
            itemListBox.DisplayMember = "name";
            foreach (Rack rack in racks)
            {
                if (rack.items.Count() < rack.length)
                {
                    rack.AddItem(item, quantity);
                    item.rack = rack;
                    break;
                }
            }
            //items.Add(item);
        }
        public Item FindItem(string itemName)
        {
            foreach (Item item in items)
            {
                if(item.name.ToLower() == itemName.ToLower())
                {
                    return item; 
                }
            }
            return null;
        }
        public void AddRobot(Robot robot)
        {
            robots.Add(robot);
        }

        public List<Job> GetAllJobs(){
            List<Job> warehouseJobs = new List<Job>();
            foreach(Robot robot in robots){
                warehouseJobs.AddRange(robot.jobs.ToList());
            }
            return warehouseJobs;
        }

        public List<Job> GetOrders(List<Job> allJobs){
            List<Job> warehouseOrders = new List<Job>();
            foreach(Job job in allJobs){
                if(job.Type == 0){
                    warehouseOrders.Add(job);
                }
            }
            return warehouseOrders;
        }

    }
}