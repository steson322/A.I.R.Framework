using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIR.Navigation
{
    public class GPSLocation
    {
        /// <summary>
        /// Guid of waypoint
        /// </summary>
        public Guid GUID;
        /// <summary>
        /// Lat of waypoint
        /// </summary>
        public double Lat;
        /// <summary>
        /// Long of waypoint
        /// </summary>
        public double Long;
        /// <summary>
        /// Altitute of waypoint
        /// </summary>
        public double Alt;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Lat"></param>
        /// <param name="Long"></param>
        /// <param name="Alt"></param>
        public GPSLocation(double Lat, double Long, double Alt)
        {
            this.Lat = Lat;
            this.Long = Long;
            this.Alt = Alt;
            this.GUID = Guid.NewGuid();
        }
        /// <summary>
        /// String format of a waypoint
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<LAT:" + Lat + ", LONG:" + Long + ", ALT:" + Alt + "> id:" ;
        }

        /// <summary>
        /// Calculate bearing angle from one waypoint to another
        /// </summary>
        /// <param name="LatStart">Lat of Start Point</param>
        /// <param name="LongStart">Long of Start Point</param>
        /// <param name="LatEnd">Lat of End Point</param>
        /// <param name="LongEnd">Long of End Point</param>
        /// <returns></returns>
        public static double GetBearing(double LatStart, double LongStart, double LatEnd, double LongEnd)
        {
            double dLat = (LatEnd - LatStart) * Math.PI / 180.0;
            double dLon = (LongEnd - LongStart) * Math.PI / 180.0;
            LatStart = LatStart * Math.PI / 180.0;
            LatEnd = LatEnd * Math.PI / 180.0;
            double y = Math.Sin(dLon) * Math.Cos(LatEnd);
            double x = Math.Cos(LatStart) * Math.Sin(LatEnd) -
                        Math.Sin(LatStart) * Math.Cos(LatEnd) * Math.Cos(dLon);
            double bearing = Math.Atan2(y, x);
            return bearing;
        }
        /// <summary>
        /// Calculate distance from one waypoint to another
        /// </summary>
        /// <param name="LatStart">Lat of Start Point</param>
        /// <param name="LongStart">Long of Start Point</param>
        /// <param name="LatEnd">Lat of End Point</param>
        /// <param name="LongEnd">Long of End Point</param>
        /// <returns></returns>
        public static double GetDistance(double LatStart, double LongStart, double LatEnd, double LongEnd)
        {
            double R = 6371000; // meter
            double dLat = (LatEnd - LatStart) * Math.PI / 180.0;
            double dLon = (LongEnd - LongStart) * Math.PI / 180.0;
            LatStart = LatStart * Math.PI / 180.0;
            LatEnd = LatEnd * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(LatStart) * Math.Cos(LatEnd);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c;
            return distance;
        }
    }
}
