using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
    public class PathFinder
    {
		// Warehouse to evaluated the path finding algorithm
		public Warehouse warehouse;

		// This is a node grid for path finding 
		public NodeWarehouse nodeWarehouse { get; set; }

		public PathFinder( NodeWarehouse _nodeWarehouse)
        {
			nodeWarehouse = _nodeWarehouse;
		}

		public RobotPath FindPath(Robot robot, Coordinate startPosition, Coordinate targetPosition)
		{
			Node startNode = nodeWarehouse.gridNode[startPosition.x, startPosition.y];
			Node targetNode = nodeWarehouse.gridNode[targetPosition.x, targetPosition.y];

			RobotPath path = new RobotPath();
			List<Node> openSet = new List<Node>();
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				Node node = openSet[0];
				for (int i = 1; i < openSet.Count; i++)
				{
					if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost)
					{
						if (openSet[i].hCost < node.hCost)
							node = openSet[i];
					}
				}

				openSet.Remove(node);
				closedSet.Add(node);

				if (node == targetNode)
				{
					path = RetracePath(robot, startNode, targetNode);

				}

				foreach (Node neighbour in nodeWarehouse.GetNeighbours(node))
				{
					if (!neighbour.walkable || closedSet.Contains(neighbour))
					{
						continue;
					}

					int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
					if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetNode);
						neighbour.parent = node;

						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
					}
				}
			}
			return path;
		}

		public RobotPath RetracePath(Robot robot, Node startNode, Node endNode)
		{
			List<Node> finalPath = new List<Node>();
			RobotPath finalRobotPath = new RobotPath();
			Node currentNode = endNode;

			// wait for a bit before go to the other location
			for (int i = 0; i < 5; i++)
			{
				if (endNode.x == robot.chargingStationPosition.x && endNode.y == robot.chargingStationPosition.y)
				{
					break;
				}
				finalPath.Add(currentNode);
			}

			while (currentNode != startNode)
			{
				finalPath.Add(currentNode);
				currentNode = currentNode.parent;
			}
			finalPath.Reverse();

			foreach(Node n in finalPath)
            {
				finalRobotPath.Enqueue(n);
            }

			return finalRobotPath;

		}

		int GetDistance(Node nodeA, Node nodeB)
		{
			int dstX = Math.Abs(nodeA.x - nodeB.x);
			int dstY = Math.Abs(nodeA.y - nodeB.y);

			if (dstX > dstY)
				return 14 * dstY + 10 * (dstX - dstY);
			return 14 * dstX + 10 * (dstY - dstX);
		}

	}
}
