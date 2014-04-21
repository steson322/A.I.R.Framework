using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIR.Navigation
{
    /// <summary>
    /// A path finder
    /// </summary>
    public abstract class PathFinder
    {
        /// <summary>
        /// Nodes for inside map
        /// </summary>
        public List<GPSLocationNode> Nodes;

        /// <summary>
        /// If a node can go to another node
        /// </summary>
        protected Dictionary<GPSLocationNode, List<GPSLocationNode>> Connections;

        /// <summary>
        /// Get a Path of node path
        /// </summary>
        /// <param name="StartNode">Node to start</param>
        /// <param name="EndNode">Node to end</param>
        /// <returns></returns>
        public abstract Path GetPath(GPSLocationNode StartNode, GPSLocationNode EndNode);

        /// <summary>
        /// Construcor of Path Finder
        /// </summary>
        public PathFinder()
        {
            Nodes = new List<GPSLocationNode>();
            Connections = new Dictionary<GPSLocationNode, List<GPSLocationNode>>();
        }

        /// <summary>
        /// Add a connection from one node to another
        /// </summary>
        /// <param name="FromNode">The node connect from</param>
        /// <param name="ToNode">The node connect to</param>
        public void AddConnection(GPSLocationNode FromNode, GPSLocationNode ToNode)
        {
            //add to node list
            if (!Nodes.Contains(FromNode))
                Nodes.Add(FromNode);
            if (!Nodes.Contains(ToNode))
                Nodes.Add(ToNode);
            //initialize the target list if not exist
            if (!Connections.ContainsKey(FromNode))
                Connections[FromNode] = new List<GPSLocationNode>();
            //Add the new target node if not yet define
            if(!Connections[FromNode].Contains(ToNode))
                Connections[FromNode].Add(ToNode);
        }

        /// <summary>
        /// Path of route
        /// </summary>
        public class Path
        {
            /// <summary>
            /// Route of node
            /// </summary>
            public List<GPSLocationNode> Route = new List<GPSLocationNode>();

            /// <summary>
            /// Total ground distance of path without final node distance to target
            /// </summary>
            public double Cost { get; private set; }

            /// <summary>
            /// Total ground distance of path with final node distance to target
            /// </summary>
            double TotalDistance;

            /// <summary>
            /// Get if Path has reached target
            /// </summary>
            public bool Reached
            {
                get
                {
                    return Cost == TotalDistance;
                }
            }

            /// <summary>
            /// Get the last node
            /// </summary>
            public GPSLocationNode LastNode
            {
                get
                {
                    return Route[Route.Count - 1];
                }
            }

            /// <summary>
            /// Add a new node at the end of route
            /// </summary>
            /// <param name="Node">The new node add to route</param>
            /// <param name="Distance">The distance from node to end</param>
            public void AddNode(GPSLocationNode Node, double Distance)
            {
                //add cost
                Cost += Node.GroundDistanceTo(this.LastNode);
                this.Route.Add(Node);
                TotalDistance = Cost + Distance;
            }

            /// <summary>
            /// Get a Clone of Path
            /// </summary>
            /// <returns></returns>
            public Path Clone()
            {
                Path path = new Path();
                path.TotalDistance = this.TotalDistance;
                path.Cost = this.Cost;
                path.Route = new List<GPSLocationNode>(this.Route.ToArray());
                return path;
            }

            /// <summary>
            /// Compare two path for lowest cost
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static int LowestCostFirst(Path A, Path B)
            {
                if (A.TotalDistance > B.TotalDistance)
                    return 1;
                else if (A.TotalDistance < B.TotalDistance)
                    return -1;
                else
                    return 0;
            }
        }
    }
}
