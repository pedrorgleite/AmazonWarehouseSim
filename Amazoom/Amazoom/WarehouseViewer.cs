using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Amazoom.Classes;

namespace Amazoom
{
    using System.Collections.Generic;
    using System.Threading;

    public partial class WarehouseViewer : Form
    {
        public Warehouse warehouse;
        private Coordinate warehouseSize;
        public string nameOfWarehouse { get; }
        
            
        public bool flag { get; set; } = false;


        public WarehouseViewer(int xSize, int ySize, string _name)
        {
            warehouseSize = new Coordinate(xSize, ySize);
            nameOfWarehouse = _name;
            InitializeComponent();
            // Todo: Make a windows forms to get these values from the user
            InitializeWarehouse(xSize, ySize, _name);
        }

        public List<Item> GetItems()
        {
            return warehouse.items;
        }
        
        public string PlaceOrder(string itemName, int qty, string clientName)
        {
            foreach(Item item_in_warehouse in warehouse.items) {
                if (item_in_warehouse.name.ToLower() == itemName.ToLower())
                {
                    warehouse.DeliveryItem(item_in_warehouse, qty, clientName);
                    item_in_warehouse.SubtractItemQuantity(qty);
                    Console.WriteLine(nameOfWarehouse + " is delivering " + qty.ToString() + " " + itemName);
                    return "Found item";
                }
            }
            return "Failed to find item";
        }

        public void RestockItem(string itemName, int qty)
        {
            foreach (Item item_in_warehouse in warehouse.items)
            {
                if (item_in_warehouse.name.ToLower() == itemName)
                {
                    warehouse.RestockItem(item_in_warehouse, qty);
                    Console.WriteLine(itemName);
                    Console.WriteLine(qty);
                }
            }
            return;
        }

        public int getItemStockLevel(string itemName)
        {
            foreach (Item item_in_warehouse in warehouse.items)
            {
                if (item_in_warehouse.name.ToLower() == itemName.ToLower())
                {
                    return item_in_warehouse.number_available_items;
                }
            }
            return -1 ;
        }

        private void InitializeWarehouse(int xSize, int ySize, string _name)
        {
            warehouse = new Warehouse
                (
                    pictureBox,
                    new Coordinate(xSize, ySize)
                );
            Text = _name;
            warehouse.wrviewer = this;
            warehouse.itemListBox = ItemList;
            warehouse.ordersTobeDeliveredListBox = ordersWaitingForDelivery;
            warehouse.itemList = ItemList;
            warehouse.warningMessage = warehouseWarning;
            InitializeItems();
            InitializeRobots();
        }

        private void InitializeRobots()
        {
            warehouse.AddRobot(new Robot("Robbie", new Coordinate(0, 0), 1, warehouse, 65, 50));
            warehouse.AddRobot(new Robot("Robbie2", new Coordinate(1, 0), 1, warehouse, 25, 50));
            warehouse.AddRobot(new Robot("Robbie3", new Coordinate(2, 0), 1, warehouse, 65, 50));
            warehouse.AddRobot(new Robot("Robbie4", new Coordinate(3, 0), 1, warehouse, 36, 50));
        }

        public void InitializeItems()
        {
            bool aisle_side;
            string workingDirectory = Environment.CurrentDirectory;
            string current_directory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            current_directory = current_directory + "/w1.csv";
            using (var reader = new StreamReader(@current_directory))
            {
                List<string> listName = new List<string>();
                List<int> listX = new List<int>();
                List<int> listY = new List<int>();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (values[0] != "ID" && values[5] == nameOfWarehouse)
                    {
                        int id = Convert.ToInt32(values[0]);
                        string name = values[1];
                        int quantity = Convert.ToInt32(values[2]);
                        int weight = Convert.ToInt32(values[3]);
                        var locations = values[4].Split('.');
                        Rack rack = warehouse.racks[Convert.ToInt32(locations[0])];
                        if (locations[3] == "R")
                        {
                            aisle_side = true;
                        }
                        else
                        {
                            aisle_side = false;
                        }
                        warehouse.AddInitializedItem(new Item(id, name, weight, aisle_side), rack, Convert.ToInt32(locations[1]), Convert.ToInt32(locations[2]),quantity,aisle_side);
                    }
                }
            }
        }

        private void OnDraw(object sender, PaintEventArgs e)
        {
            warehouse.Draw(e);
        }

        private void UpdateRobots(Warehouse warehouse)
        {
            // reset all nodes
            foreach(Node n in warehouse.nodeWarehouse.gridNode){
                n.robotAtNode = null;
            }
            foreach (Robot robot in warehouse.robots)
            {
                // set node for collision avoidance
                warehouse.nodeWarehouse.gridNode[robot.position.x, robot.position.y].robotAtNode = robot;
            }
            foreach (Robot robot in warehouse.robots)
            {
                robot.Updade();
            }
        }

        public int ticks = 0;
        private void MainSimTimerEvent(object sender, EventArgs e)
        {
            UpdateRobots(warehouse);
            Refresh();
            ticks++;
        }



        private void ItemList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Item selectedItem = ItemList.SelectedItem as Item;
            itemPosition.Text = "";
            if (selectedItem != null)
            {
                if (selectedItem.number_available_items < 3)
                {
                    numberOfItems.Text = "Number of Items: " + selectedItem.number_available_items.ToString() + "  LOW STOCK";
                }
                else
                {
                    numberOfItems.Text = "Number of Items: " + selectedItem.number_available_items.ToString();
                }

                itemName.Text = "Name: " + selectedItem.name + ", Weight: " + selectedItem.weight.ToString();

                foreach (itemCoordinate pos in selectedItem.itemCoord_list)
                {
                    if (selectedItem.aisle_side)
                    {
                        itemPosition.Text += "Position: " + "(" + pos.x.ToString() + "," + pos.y.ToString() + "," + pos.z.ToString() + ") - " + "Rack:" + selectedItem.rack.name + "," + "Right ";
                    }
                    else
                    {
                        itemPosition.Text += "Position: " + "(" + pos.x.ToString() + "," + pos.y.ToString() + "," + pos.z.ToString() + ") - " + "Rack:" + selectedItem.rack.name + "," + "Left ";
                    }
                }
            }
            warehouse.warningMessage.Text = "";

        }

        private void restockButton_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(numberRestockItem.Value) > 0){
                Item selectedItem = ItemList.SelectedItem as Item;
                warehouse.RestockItem(selectedItem, Convert.ToInt32(numberRestockItem.Value));

                numberOfItems.Text = "Number of Items: " + selectedItem.number_available_items.ToString();
                Refresh();
            }
        }


        private void refresh_Click(object sender, EventArgs e)
        {
            Item selectedItem = ItemList.SelectedItem as Item; 
            //RefreshOrderList();
            if (selectedItem != null)
            {
                Refresh();
                numberOfItems.Text = "Number of Items: " + selectedItem.number_available_items.ToString();

            }
        }

        private void testOrder_Click(object sender, EventArgs e)
        {
            warehouse.DeliveryItem(warehouse.items[ticks%40], 1, "Testing");
        }

        private void deliveryButton_Click(object sender, EventArgs e)
        {
            int maxTruckWightInt = Convert.ToInt32(maxTruckWeight.Value);
            int loadingWeight = 0;
            List<Job> removeFromWaitList = new List<Job>();
            foreach(Job job in warehouse.ordersTobeDeliveredListBox.Items)
            {
                if(loadingWeight >= maxTruckWightInt)
                {
                    break;
                }
                else
                {
                    loadingWeight += (job.item.weight * job.numberItemsTranported);
                    ordersOutForDelivery.Items.Add(job);
                    ordersOutForDelivery.DisplayMember = "name";
                    removeFromWaitList.Add(job);
                }
            }
            foreach(Job job in removeFromWaitList)
            {
                warehouse.ordersTobeDeliveredListBox.Items.Remove(job);
            }

        }

        private void addRobot_Click(object sender, EventArgs e)
        {
            if(warehouse.robots.Count() < warehouse.warehouseSize.x)
            {
                string robotName = "Robbie" + warehouse.robots.Count().ToString();
                int xPos = 0;
                int yPos = 0;
                foreach (Robot robot in warehouse.robots)
                {
                    if (robot.chargingStationPosition.x == xPos && robot.chargingStationPosition.y == yPos)
                    {
                        xPos++;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                warehouse.AddRobot(new Robot(robotName, new Coordinate(xPos, yPos), 1, warehouse, 100, 50));
            }
        }

        private void removeRobot_Click(object sender, EventArgs e)
        {
            warehouse.robots.RemoveAt(warehouse.robots.Count - 1);
        }

        private void formClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }


        private void addItemButton_Click(object sender, EventArgs e)
        {
            if(ItemsName.Text != "Item's Name" && warehouse.items.FindIndex(f => f.name == ItemsName.Text) < 0)
            {
                string rightOrLeftPos = rightOrLeftBox.SelectedItem as string;
                if (Convert.ToInt32(qtyUpAndDown.Value)>0 && Convert.ToInt32(weightUpAndDown.Value) > 0 && rightOrLeftPos!= null)
                {
                    bool aisleSideRightBool = (rightOrLeftPos == "Right");
                    warehouse.AddItem(new Item(warehouse.items.Count()+1, ItemsName.Text, Convert.ToInt32(weightUpAndDown.Value), aisleSideRightBool), Convert.ToInt32(qtyUpAndDown.Value)); 
                }
                var itms = GetItems();
            }
        }

        private void removeItem_Click(object sender, EventArgs e)
        {
            Item selectedItem = ItemList.SelectedItem as Item;
            foreach( Rack rack in warehouse.racks)
            {
                foreach(Item item in rack.items)
                {
                    if(item == selectedItem)
                    {
                        rack.items.Remove(selectedItem);
                        ItemList.Items.Remove(selectedItem);
                        return;
                    }
                }
            }
        }


        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void numberRestockItem_ValueChanged(object sender, EventArgs e)
        {

        }

        private void WarehouseViewer_Load(object sender, EventArgs e)
        {

        }

        private void formClosedManually(object sender, FormClosedEventArgs e)
        {
        }

        private void ItemsName_TextChanged(object sender, EventArgs e)
        {
        }


    }
}