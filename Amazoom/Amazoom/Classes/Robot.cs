using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Amazoom.Classes
{

    public class Robot
    {

        //================================================================================//
        //                              Variable declarations                             //
        //================================================================================//

        // name of the robot: mandatory
        public string name;

        // charging station position: mandatory
        public Coordinate chargingStationPosition;

        // Priority of the robot over other robots
        public int priority;

        // Position of the robot on the nodewarehouse
        public Node position { get; set; }

        public bool wait { get; set; } = false;

        public bool wait1 { get; set; } = false;
        // check if the robot is doing any job if not set isBusy to false
        public bool isBusy
        {
            get
            {
                return jobs.Any();
            }
        }

        public bool waitingForJobs = false;

        // Current jobs of the robot
        public Queue<Job> jobs { get; set; } = new Queue<Job>();

        // current job of the robot
        public Job job
        {
            get
            {
                return jobs.Peek();
            }
        }

        // next node
        public Node nextNode
        {
            get
            {
                return job.PeekNextNode();
            }
        }


        // Warehouse that the robot is in
        public Warehouse warehouse;

        // Method used to find the robot path
        public PathFinder method;

        // TODO Implement battery and weight
        // Battery level
        public float batteryLevel { get; set; }

        private float batteryDrainPerGrid { get; }
        private float batteryChargePerTick { get; }
        // Waight limit
        public int weightLimit { get; set; }



        //================================================================================//
        //                              Class functions                                   //
        //================================================================================//

        public Robot(string _name, Coordinate _chargingStationPosition, int _priority, Warehouse _warehouse, float _batteryLevel, int _weightLimit)
        {
            name = _name;
            chargingStationPosition = _chargingStationPosition;
            priority = _priority;
            warehouse = _warehouse;
            position = new Node(false, chargingStationPosition.x, chargingStationPosition.y);
            method = new PathFinder(warehouse.nodeWarehouse);
            batteryLevel = _batteryLevel;
            weightLimit = _weightLimit;
            batteryDrainPerGrid = 1;
            batteryChargePerTick = 2;
        }

        public void GetItem(Item item, int quantity, string clientName, int jobId)
        {
            if(quantity <= item.number_available_items){
                               
                if(quantity < item.number_available_items_current_pos)
                {
                    var jobTobeEnqueued = GetDeliveryJob(item, quantity, clientName, jobId);
                    
                    if(jobTobeEnqueued == null)
                    {
                        return;
                    }

                    jobs.Enqueue(jobTobeEnqueued);
                }
                else
                {
                    int quantityTemp = quantity;

                    while(quantityTemp > item.number_available_items_current_pos)
                    {
                        if(GetDeliveryJob(item, item.number_available_items_current_pos, clientName, jobId) == null)
                        {
                            return;
                        }
                        jobs.Enqueue(GetDeliveryJob(item, item.number_available_items_current_pos, clientName, jobId));
                        quantityTemp -= item.number_available_items_current_pos;
                    }
                    if (quantityTemp > 0)
                    {
                        if(GetDeliveryJob(item, item.number_available_items_current_pos, clientName, jobId) == null)
                        {
                            return;
                        }
                        jobs.Enqueue(GetDeliveryJob(item, quantityTemp, clientName, jobId));
                    }
                }
            }
            else
            {
                Console.Write("Not enough items in stock");
            }

        }

        public void RestockItem(Item item, int quantity)
        {
            if(quantity*item.weight < item.rack.max_weight_shelf - item.number_available_items_current_pos*item.weight)
            {
                var jobTobeEnqueue = GetRestockJob(item, quantity);
                jobs.Enqueue(jobTobeEnqueue);

            }
            else
            {
                warehouse.warningMessage.Text = "To many items rack cannot handle";
                Console.WriteLine("To many items rack cannot handle");
            }
        }


        public void Updade()
        {
            //Adds charge to the battery if robot is on the charging pad 
            if (position.x == chargingStationPosition.x && position.y == chargingStationPosition.y)
            {
                batteryLevel = batteryLevel + batteryChargePerTick;
            }
            if (position.x == warehouse.loadingStation.x && position.y == warehouse.loadingStation.y)
            {
                if (isBusy && !job.isWaitingForDelivery)
                {
                    if (job.Type == 0)
                    {
                        warehouse.ordersTobeDeliveredListBox.Items.Add(job);
                        warehouse.ordersTobeDeliveredListBox.DisplayMember = "name";
                        job.isWaitingForDelivery = true;
                    }
                }

            }
            if (isBusy)
            {
                if (job.Any())
                {
                    Move();
                }
                else
                {
                    jobs.Dequeue();
                }
            }
        }

        public void Move()
        {
            //Stops robot from moving if robot is out of battery
            if (batteryLevel <= 0)
            {
                Console.Write(name);
                Console.WriteLine(" is out of battery!");
                return;
            }

            // Check if there is any robot in the next node to avoid collision
            if (nextNode.robotAtNode == null || nextNode.robotAtNode == this)
            {
                // If wait -> do nothing for 1 tick
                if (!wait1)
                {
                    // Seach if there is any robot with the same destination as this robot to make the other wait
                    foreach (Robot robot in warehouse.robots)
                    {
                        if (robot == this || !robot.isBusy)
                        {
                            continue;
                        }
                        if (robot.job.Any())
                        {
                            if (robot.nextNode.x == nextNode.x && robot.nextNode.y == nextNode.y)
                            {
                                robot.wait1 = true;
                            }
                        }
                    }
                    // Update position
                    if (job.Any())
                    {
                        //Check if robot is on the charging station
                        if (position.x == chargingStationPosition.x && position.y == chargingStationPosition.y)
                        {
                            //Check if the robot has enough battery (plus 10% safety margin) to complete the job
                            if (job.Length * 1.1 * batteryDrainPerGrid >= batteryLevel)
                            {
                                wait1 = true; //If insuficent battery, wait a tick to continue charging
                            }
                            else
                            {
                                //If sufficient battery, move off the charge station
                                position.x = nextNode.x;
                                position.y = nextNode.y;
                                batteryLevel = batteryLevel - batteryDrainPerGrid;
                                job.DequeueNextNode();
                            }
                        }
                        else
                        {
                            //Move the robot and drain the battery
                            position.x = nextNode.x;
                            position.y = nextNode.y;
                            batteryLevel = batteryLevel - batteryDrainPerGrid;
                            job.DequeueNextNode();
                        }
                    }
                }
                else
                {
                    wait1 = false;
                }
            }
            else
            {
                Robot robotAtNextNode = nextNode.robotAtNode;
                if (robotAtNextNode.isBusy)
                {
                    if (robotAtNextNode.job.Any())
                    {

                        if ((robotAtNextNode.job.PeekNextNode().x == position.x && robotAtNextNode.job.PeekNextNode().y == position.y)
                            && (robotAtNextNode.position.x == job.PeekNextNode().x && job.PeekNextNode().y == robotAtNextNode.position.y))
                        {
                            UpdatePath();
                        }
                    }
                }
                else
                {
                    UpdatePath();
                }
            }
        }

        private void UpdatePath()
        {
            Node node = nextNode;
            node.walkable = false;
            jobs.Peek().UpdatePath(method.FindPath(this, new Coordinate(position.x, position.y), new Coordinate(job.currentPath.targetPos.x, job.currentPath.targetPos.y)).path);
            node.walkable = true;
        }


        // 
        // Helper fuction
        //
        Job GetDeliveryJob(Item item, int quantity, string clientName, int jobId)
        {
            Coordinate targetPos = GetTargetPos(item);
            Queue<RobotPath> paths = new Queue<RobotPath>();
            int jobLength = 0;

            //If the control software tries to assign a job too heavy, reject it
            if (item.weight * quantity > weightLimit)
            {
                return null;
            }

            // charging station to item
            paths.Enqueue(method.FindPath(this, chargingStationPosition, targetPos));
            // charging station to docking station
            paths.Enqueue(method.FindPath(this, targetPos, warehouse.loadingStation));
            // charging station to charging station
            paths.Enqueue(method.FindPath(this, warehouse.loadingStation, chargingStationPosition));

            //Add up the length of each section for battery calculation later
            foreach (RobotPath path in paths)
            {
                jobLength = jobLength + path.path_length;
            }

            Job jobTobeEnqueue = new Job(paths, item);
            jobTobeEnqueue.numberItemsTranported = quantity;
            jobTobeEnqueue.Type = 0;
            jobTobeEnqueue.Length = jobLength;
            jobTobeEnqueue.clientName = clientName;
            jobTobeEnqueue.id = jobId;
            return jobTobeEnqueue;
        }

        Job GetRestockJob(Item item, int quantity)
        {
            Coordinate targetPos = GetTargetPos(item);
            Queue<RobotPath> paths = new Queue<RobotPath>();
            int jobLength = 0;
            // charging station to item
            paths.Enqueue(method.FindPath(this, chargingStationPosition, warehouse.loadingStation));
            // charging station to docking station
            paths.Enqueue(method.FindPath(this, warehouse.loadingStation, targetPos));
            // charging station to charging station
            paths.Enqueue(method.FindPath(this, targetPos, chargingStationPosition));

            foreach (RobotPath path in paths)
            {
                jobLength = jobLength + path.path_length;
            }

            Job jobTobeEnqueue = new Job(paths, item);
            jobTobeEnqueue.numberItemsTranported = quantity;
            jobTobeEnqueue.Type = 1;
            jobTobeEnqueue.Length = jobLength;
            return jobTobeEnqueue;
        }

        Coordinate GetTargetPos(Item item)
        {

            Coordinate targetPos = new Coordinate();
            if (item.aisle_side)
            {
                targetPos = new Coordinate(item.position.x + 1, item.position.y);
            }
            else
            {
                targetPos = new Coordinate(item.position.x - 1, item.position.y);
            }

            return targetPos;
            // copy above for a vertical warehouse


        }


        /// Detects if the robots will collide

        public bool ColisionAvoidance(Node nextNode)
        {
            if (nextNode.isThereRobot)
            {
                return true;
            }
            return false;
        }


    }
}