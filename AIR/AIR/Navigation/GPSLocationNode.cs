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
        { 
        
        }

    }
}
