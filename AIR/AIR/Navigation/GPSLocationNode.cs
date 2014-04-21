using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIR.Navigation
{
    public class GPSLocationNode : GPSLocation
    {
        /// <summary>
        /// Constructor of GPS Location Node
        /// </summary>
        /// <param name="Lat"></param>
        /// <param name="Long"></param>
        /// <param name="Alt"></param>
        public GPSLocationNode(double Lat, double Long, double Alt)
            : base(Lat, Long, Alt)
        { }

        /// <summary>
        /// Ground distance from one this node to target node
        /// </summary>
        /// <param name="Node">Node to calculate to</param>
        /// <returns></returns>
        public double GroundDistanceTo(GPSLocationNode Node)
        {
            return GetDistance(this.Lat, this.Long, Node.Lat, Node.Long);
        }

        /// <summary>
        /// True distance from one this node to target node
        /// </summary>
        /// <param name="Node">Node to calculate to</param>
        /// <returns></returns>
        public double TrueDistanceTo(GPSLocationNode Node)
        {
            double groundDist = GetDistance(this.Lat, this.Long, Node.Lat, Node.Long);
            double AltDist = this.Alt - Node.Alt;
            return Math.Sqrt(groundDist * groundDist + AltDist * AltDist);
        }
    }
}
