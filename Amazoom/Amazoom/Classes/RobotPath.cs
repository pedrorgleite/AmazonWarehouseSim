using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
    public class RobotPath
    {
        public Node targetPos;
        public Queue<Node> path { get; set; }

        public int path_length { get; set; }


        public RobotPath()
        {
            path = new Queue<Node>();
            path_length = 0;
        }

        public Node Peek(){
            return path.Peek();
        }
        public void Enqueue(Node node)
        {
            path.Enqueue(node);
            targetPos = node;
            path_length++;
        }

        public Node Dequeue()
        {
            path_length--;
            return path.Dequeue();

        }

        public bool Any()
        {
            return path.Any();
        }
    }
}
