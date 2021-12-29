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
using Amazoom.Classes;

namespace Amazoom
{
    public partial class WarehousesViewer : Form
    {
        public List<WarehouseViewer> warehouses = new List<WarehouseViewer>();
        List<Order> orders
        {
            get
            {
                List<Order> odrs = new List<Order>();
                foreach (WarehouseViewer wv in warehouses)
                {
                    foreach (Job job in wv.ordersWaitingForDelivery.Items)
                    {
                        var order = new Order(job.id + startupIds, job.item.name, job.numberItemsTranported, job.clientName);
                        order.status = "Waiting for Delivery";
                        odrs.Add(order);
                    }
                    foreach (Job job in wv.ordersOutForDelivery.Items)
                    {
                        var order = new Order(job.id + startupIds, job.item.name, job.numberItemsTranported, job.clientName);
                        order.status = "Out for Delivery";
                        odrs.Add(order);
                    }
                }
                return odrs;
            }

        }

        List<Order> StoredOrders = new List<Order>();

        public WarehousesViewer()
        {
            InitializeComponent();
            InitializeWarehouses();
        }
        void InitializeWarehouses()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string current_directory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            current_directory = current_directory + "/WarehouseLayout.csv";
            using (var reader = new StreamReader(@current_directory))
            {
                List<string> listName = new List<string>();
                List<int> listX = new List<int>();
                List<int> listY = new List<int>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    listName.Add(values[0]);
                    listX.Add(Int32.Parse(values[1]));
                    listY.Add(Int32.Parse(values[2]));
                }

                int index = 0;
                int x, y;
                foreach (string name in listName)
                {
                    x = listX[index];
                    y = listY[index];
                    var warh = new WarehouseViewer(listX[index], listY[index], name);
                    warehouses.Add(warh);
                    listOfWarehouse.Items.Add(warh);
                    index++;
                }
            }
            InitializeOrders();
            listOfWarehouse.DisplayMember = "nameOfWarehouse";
        }

        private void listOfWarehouse_SelectedIndexChanged(object sender, EventArgs e)
        {
            WarehouseViewer selectedWarehouse = listOfWarehouse.SelectedItem as WarehouseViewer;
            if (!selectedWarehouse.Visible)
            {
                selectedWarehouse.Visible = true;
            }
        }


        public string PlaceOrder(string itemName, int qty, string clientName)
        {
            var warehouseStockLevels = new List<(string, string, int)> { };

            if (getItemStockLevel(itemName) < qty)
            {
                return "Order Failed Due to Insufficient Stock";
            }

            int temp_qty = qty;
            if (qty <= 0)
            {
                return "Order Failed. Negative Quantity Specified";
            }
            foreach (WarehouseViewer warehouseViewer in warehouses)
            {
                int warehouseStock = warehouseViewer.getItemStockLevel(itemName);
                if (warehouseStock >= temp_qty)
                {
                    if (warehouseViewer.PlaceOrder(itemName, temp_qty, clientName) == "Found item")
                    {
                        temp_qty = 0;
                        return "Order Successfully Placed";
                    }
                }
                else if (temp_qty > warehouseStock && warehouseStock > 0)
                {
                    temp_qty = temp_qty - warehouseStock;
                    warehouseViewer.PlaceOrder(itemName, temp_qty, clientName);
                }
            }
            if (temp_qty > 0)
            {
                return "Order Failed Due to Insufficient Stock";
            }
            else
            {
                return "Order Successfully Placed";
            }
        }

        public string RestockItem(string itemName, int qty)
        {
            var warehouseStockLevels = new List<(string, string, int)> { };

            if (getItemStockLevel(itemName) < qty)
            {
                return "Order Failed Due to Insufficient Stock";
            }

            int temp_qty = qty;
            if (qty <= 0)
            {
                return "Order Failed. Negative Quantity Specified";
            }
            foreach (WarehouseViewer warehouseViewer in warehouses)
            {
                //RestockItem
                int warehouseStock = warehouseViewer.getItemStockLevel(itemName);
                if (warehouseStock != -1)
                {
                    warehouseViewer.RestockItem(itemName, warehouseStock);
                    return ("item restocked");
                }
            }
            return ("item not found");
        }

        public string UpdateList()
        {
            string warehouseList = "";
            List<Item> itemsList = new List<Item>();
            int itmFound = 0;
            foreach (WarehouseViewer warehouseViewer in warehouses)
            {
                foreach (Item WHitem in warehouseViewer.GetItems())
                {
                    itmFound = 0;
                    foreach (Item tempItems in itemsList)
                    {
                        if (tempItems.name.ToLower() == WHitem.name.ToLower())
                        {
                            tempItems.qtyFrontEnd += WHitem.number_available_items;
                            itmFound = 1;
                        }
                    }
                    if (itmFound == 0)
                    {
                        Item newItem = new Item(WHitem.id, WHitem.name, WHitem.weight, WHitem.aisle_side);
                        newItem.qtyFrontEnd = WHitem.number_available_items;
                        itemsList.Add(newItem);
                        itmFound = 0;
                    }
                }
            }

            foreach (Item item_in_warehouse in itemsList)
            {
                warehouseList = warehouseList + "%" + item_in_warehouse.name + "-" + item_in_warehouse.qtyFrontEnd;
            }
            return warehouseList;
        }

        public int getItemStockLevel(string itemName)
        {
            int totalStock = 0;
            itemName = itemName.ToLower();
            var warehouseStockLevels = new List<(string, string, int)> { };
            foreach (WarehouseViewer warehouseViewer in warehouses)
            {
                int warehouseStock = warehouseViewer.getItemStockLevel(itemName);
                var tuple = (warehouseViewer.nameOfWarehouse, itemName, warehouseStock);
                warehouseStockLevels.Add(tuple);
                totalStock = totalStock + warehouseStock;
            }
            return (totalStock);
        }

        // TODO Implement status of the order
        private void searchButton_Click(object sender, EventArgs e)
        {
            List<Order> ordersSearch = new List<Order>();
            ordersSearch.AddRange(StoredOrders);
            ordersSearch.AddRange(orders);

            Order order = ordersSearch.Find(f => f.id == Convert.ToInt32(orderIdnumericUpDown.Value));
            if (order != null)
            {
                warningMessage.Text = "";
                clientName.Text = "Client: " + order.clientName;
                orderStatus.Text = "Status: " + order.status;
                orderItem.Text = "Item: " + order.itemName;
                orderQuantity.Text = "Quantity: " + order.qty.ToString();
            }
            else
            {
                warningMessage.Text = "Could not find Order. \nPlease Try again";
                clientName.Text = "";
                orderStatus.Text = "";
                orderItem.Text = "";
                orderQuantity.Text = "";
            }

        }

        private void MainPageTimerEvent(object sender, EventArgs e)
        {
            UpdateCsv();
        }

        private void UpdateCsv()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string main_directory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            var csv = new StringBuilder();
            var current_directory = main_directory + "/w1.csv";
            var newLine = string.Format("{0},{1},{2},{3},{4},{5}", "ID", "Name", "Quantity", "Weight", "Locations", "Warehouse");
            csv.AppendLine(newLine);
            foreach (WarehouseViewer war in warehouses)
            {
                foreach (Item item in war.GetItems())
                {
                    foreach (itemCoordinate pos in item.itemCoord_list)
                    {
                        string newLine2 = "";
                        int rack_xPos = (pos.x - war.warehouse.colsOnSides) / 2;
                        if (pos.aisle_side)
                        {
                            newLine2 = string.Format("{0},{1},{2},{3},{4},{5}", item.id.ToString(), item.name, pos.num_items.ToString(), item.weight.ToString(), rack_xPos.ToString() + "." + (pos.y - +war.warehouse.rowAtBotton).ToString() + "." + pos.z.ToString() + ".R", war.nameOfWarehouse);
                        }
                        else
                        {
                            newLine2 = string.Format("{0},{1},{2},{3},{4},{5}", item.id.ToString(), item.name, pos.num_items.ToString(), item.weight.ToString(), rack_xPos.ToString() + "." + (pos.y - +war.warehouse.rowAtBotton).ToString() + "." + pos.z.ToString() + ".L", war.nameOfWarehouse);
                        }
                        csv.AppendLine(newLine2);
                    }
                }
            }
            File.WriteAllText(current_directory, csv.ToString());

            var csv2 = new StringBuilder();
            var current_directory2 = main_directory + "/orders.csv";
            var newLineOrders = string.Format("{0},{1},{2},{3},{4}", "ID", "ItemName", "Quantity", "Client", "Status");
            csv2.AppendLine(newLineOrders);
            foreach (Order order in orders)
            {
                csv2.AppendLine(string.Format("{0},{1},{2},{3},{4}", order.id.ToString(), order.itemName, order.qty, order.clientName, order.status));
            }
            File.WriteAllText(current_directory2, csv2.ToString());
        }
        int startupIds = 0;
        public void InitializeOrders()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string current_directory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            current_directory = current_directory + "/orders.csv";
            using (var reader = new StreamReader(@current_directory))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (values[0] != "ID")
                    {
                        StoredOrders.Add(new Order(Convert.ToInt32(values[0]), values[1], Convert.ToInt32(values[2]), values[3]));
                    }
                    startupIds++;
                }
            }
        }
    }
}
