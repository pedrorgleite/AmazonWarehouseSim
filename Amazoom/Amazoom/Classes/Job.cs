using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazoom.Classes
{
    public class Job
    {
        //=======================================================//
        //               Variable declaration                    //
        //=======================================================//
        Queue<RobotPath> job;
        public Item item;
        public RobotPath currentPath
        {
            get
            {
                return job.Peek();
            }
        }

        public int Length { get; set; }

        Node currentTarget
        {
            get
            {
                return currentPath.targetPos;
            }
        }

        public int numberItemsTranported = 1;

        public string name
        {
            get
            {
                return item.name + ", Quantity:"+ Math.Abs(numberItemsTranported).ToString()+ ", Client:" + clientName; //Use the abs value because it gets changed to negative somewhere along the line
            }
        }

        public string clientName { get; set; } = "Not set";

        public bool isWaitingForDelivery { get; set; } = false;
        /// <summary>
        /// Type of job
        /// 0: Delivery
        /// 1: Restock
        /// </summary>
        public int Type;

        public int id;

        //=======================================================//
        //                        Methods                        //
        //=======================================================//

        public Job(Queue<RobotPath> _job, Item _item)
        {
            job = _job;
            item = _item;
        }

        public Node PeekNextNode()
        {
            return currentPath.Peek();
        }

        public void UpdatePath(Queue<Node> path)
        {
            job.Peek().path = path;
        }


        public Node DequeueNextNode()
        {
            return currentPath.Dequeue();
        }

        public RobotPath Peek()
        {
            return job.Peek();
        }

        public void Enqueue(RobotPath path)
        {
            job.Enqueue(path);
        }

        public RobotPath Dequeue()
        {
            return job.Dequeue();
        }

        public bool Any()
        {
            if (job.Any())
            {
                if (currentPath.Any())
                {
                    return true;
                }
                else
                {
                    job.Dequeue();
                    if (job.Any())
                    {
                        return true;
                    }
                    else
                    {
                        if (Type == 0)
                        {
                            //item.number_available_items -= Math.Abs(numberItemsTranported); //Use the abs value because it gets changed to negative somewhere along the line
                        }
                        else
                        {
                            item.currentItemPos.num_items += Math.Abs(numberItemsTranported); //Use the abs value because it gets changed to negative somewhere along the line
                        }
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }
}