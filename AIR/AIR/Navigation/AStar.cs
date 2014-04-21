using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIR.Navigation
{
    /// <summary>
    /// A Star Path finder
    /// </summary>
    public class AStar : PathFinder
    {
        /// <summary>
        /// A Star Search mode
        /// </summary>
        public enum AStarMode
        { 
            QuickCompute,
            BestResult
        }

        /// <summary>
        /// Search mode of A Start
        /// </summary>
        public AStarMode Mode = AStarMode.BestResult;

        /// <summary>
        /// Get the path using AStar Path algorithm
        /// </summary>
        /// <param name="StartNode">Node to start</param>
        /// <param name="EndNode">Node to end</param>
        /// <returns></returns>
        public override Path GetPath(GPSLocationNode StartNode, GPSLocationNode EndNode)
        {
            //make sure nodes exist
            if (!Nodes.Contains(StartNode))
                return null;
            if (!Nodes.Contains(EndNode))
                return null;
            //for best result
            if (Mode == AStarMode.BestResult)
                return GetBestResult(StartNode, EndNode);
            else if (Mode == AStarMode.QuickCompute)
                return GetQuickResult(StartNode, EndNode);
            //avoid some mode not implemented
            throw new NotImplementedException("AStar " + Mode.ToString() + " was not implemented");
        }

        /// <summary>
        /// Compute route with least travel cost but not best compute time
        /// </summary>
        /// <param name="StartNode">Node to start</param>
        /// <param name="EndNode">Node to end</param>
        /// <returns></returns>
        Path GetBestResult(GPSLocationNode StartNode, GPSLocationNode EndNode)
        {
            //make a free list of cannidate path
            List<Path> Cannidate = new List<Path>();
            //record distance from one node to end node
            Dictionary<GPSLocationNode, double> NodeDistances = new Dictionary<GPSLocationNode, double>();
            //prepare a memory for distance factor
            double distance = StartNode.GroundDistanceTo(EndNode);
            //create an initial path
            Path initial = new Path();
            initial.AddNode(StartNode, distance);
            //define ndoe distance
            NodeDistances[StartNode] = distance;
            //add initial path
            Cannidate.Add(initial);
            //start iteration
            while (true)
            { 
                //avoid empty cannidate
                if (Cannidate.Count == 0)
                    return null;
                //Sort Cannidate
                Cannidate.Sort(Path.LowestCostFirst);
                //check if first one is best
                if (Cannidate[0].Reached)
                    break;
                //dequeue first
                Path currentPath = Cannidate[0];
                Cannidate.RemoveAt(0);
                //expand path if have its connection
                if (Connections.ContainsKey(currentPath.LastNode))
                {
                    foreach (var expandableNode in Connections[currentPath.LastNode])
                    {
                        //disregard if the node is already visited
                        if (currentPath.Route.Contains(expandableNode))
                            continue;
                        //calculate end distance if not exist
                        if (!NodeDistances.ContainsKey(expandableNode))
                        {
                            distance = expandableNode.GroundDistanceTo(EndNode);
                            NodeDistances[expandableNode] = distance;
                        }
                        //clone current path and add expanded node
                        Path expandedPath = currentPath.Clone();
                        expandedPath.AddNode(expandableNode, NodeDistances[expandableNode]);
                        //add new path
                        Cannidate.Add(expandedPath);
                    }
                }
            }
            //return best cannidate
            return Cannidate[0];
        }

        /// <summary>
        /// Compute route with less compute time algorithm
        /// </summary>
        /// <param name="StartNode">Node to start</param>
        /// <param name="EndNode">Node to end</param>
        /// <returns></returns>
        Path GetQuickResult(GPSLocationNode StartNode, GPSLocationNode EndNode)
        {
            //make a free list of cannidate path
            List<Path> Cannidate = new List<Path>();
            //record distance from one node to end node
            Dictionary<GPSLocationNode, double> NodeDistances = new Dictionary<GPSLocationNode, double>();
            //record closed node
            List<GPSLocationNode> ClosedNodes = new List<GPSLocationNode>();
            //prepare a memory for distance factor
            double distance = StartNode.GroundDistanceTo(EndNode);
            //create an initial path
            Path initial = new Path();
            initial.AddNode(StartNode, distance);
            //define ndoe distance
            NodeDistances[StartNode] = distance;
            //add initial path
            Cannidate.Add(initial);
            //start iteration
            while (true)
            {
                //avoid empty cannidate
                if (Cannidate.Count == 0)
                    return null;
                //check if last one is best
                if (Cannidate[Cannidate.Count - 1].Reached)
                    break;
                //read last
                Path currentPath = Cannidate[Cannidate.Count - 1];
                //prepare next path
                GPSLocationNode bestNextNode = null;
                double bestNextDistance = Double.MaxValue;
                //expand path if have its connection while node not closed
                if (Connections.ContainsKey(currentPath.LastNode) && !ClosedNodes.Contains(currentPath.LastNode))
                {
                    //look for best
                    foreach (var expandableNode in Connections[currentPath.LastNode])
                    {
                        //disregard if the node is already closed
                        if (ClosedNodes.Contains(expandableNode))
                            continue;
                        //disregard if the node is already visited
                        if(currentPath.Route.Contains(expandableNode))
                            continue;
                        //calculate end distance if not exist
                        if (!NodeDistances.ContainsKey(expandableNode))
                        {
                            distance = expandableNode.GroundDistanceTo(EndNode);
                            NodeDistances[expandableNode] = distance;
                        }
                        //update best if found
                        if (NodeDistances[expandableNode] < bestNextDistance)
                        {
                            bestNextDistance = NodeDistances[expandableNode];
                            bestNextNode = expandableNode;
                        }
                    }
                }
                //close node if no expandable node found
                if (bestNextNode == null)
                {
                    ClosedNodes.Add(currentPath.LastNode);
                }
                else
                {
                    //clone current path and add expanded node
                    Path expandedPath = currentPath.Clone();
                    expandedPath.AddNode(bestNextNode, bestNextDistance);
                    //add new path
                    Cannidate.Add(expandedPath);
                }
            }
            //return best cannidate
            return Cannidate[Cannidate.Count - 1];
        }
    }
}
