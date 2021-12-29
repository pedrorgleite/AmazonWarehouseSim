using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
    class Order
    {
        public int id;
        public string itemName { get; set; }
        public int qty { get; set; }
        public string clientName { get; set; }
        public string status;

        public Job job { get; set; }

        public Order(int _id, string _itemName, int _qty, string _clientName)
        {
            id = _id;
            itemName = _itemName;
            qty = _qty;
            clientName = _clientName;
        }
    }
}
