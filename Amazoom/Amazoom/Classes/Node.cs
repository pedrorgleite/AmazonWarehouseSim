using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
	public class Node
	{

		public bool walkable { get; set; } = true;
		// checks if there is any robot in the node
		public bool isThereRobot { get; set; } = false;

		public Robot robotAtNode { get; set; }
		public Coordinate warehousePosition;
		public int x;
		public int y;

		public int gCost;
		public int hCost;
		public Node parent;

		public Node(bool _walkable, int _X, int _Y)
		{
			walkable = _walkable;
			x = _X;
			y = _Y;
		}

		public int fCost
		{
			get
			{
				return gCost + hCost;
			}
		}
	}
}
