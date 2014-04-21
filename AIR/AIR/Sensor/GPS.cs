using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIR.Sensor
{
    public class GPS : Sensor<GPSPackage>
    {
        #region Public Property
        /// <summary>
        /// Raw data of IMU
        /// </summary>
        public GPSPackage RawData { get; private set; }
        /// <summary>
        /// GPS time stamp
        /// </summary>
        public DateTime GPSTime { get; private set; }
        /// <summary>
        /// Lat of GPS
        /// </summary>
        public double Latitude { get; private set; }
        /// <summary>
        /// Long of GPS
        /// </summary>
        public double Longitude { get; private set; }
        /// <summary>
        /// Alt of GPS
        /// </summary>
        public double Altitude { get; private set; }
        /// <summary>
        /// Indicate good or bad data
        /// </summary>
        public bool GoodData { get; private set; }
        /// <summary>
        /// Satillite count
        /// </summary>
        public int SatelliteCount { get; private set; }
        /// <summary>
        /// Track angle of gps
        /// </summary>
        public double TrackAngle { get; private set; }
        /// <summary>
        /// Ground Speed of gps
        /// </summary>
        public double GroundSpeed { get; private set; }
        #endregion Public Property
        /// <summary>
        /// Constructor of a GPS object
        /// </summary>
        public GPS()
        {
            RawData = new GPSPackage();
        }
        /// <summary>
        /// Update a GPS data
        /// </summary>
        /// <param name="package"></param>
        public override void Update(GPSPackage package)
        {
            this.RawData = package;
            if (RawData.NMEA.IndexOf("$GPGGA") > -1)
            {
                try
                {
                    string[] fields = RawData.NMEA.Split(',');
                    
                    //parse time
                    double timeT = Convert.ToDouble(fields[1]);
                    int timeHr = (int)timeT / 10000;
                    int timeMin = ((int)timeT % 10000) / 100;
                    int timeSec = (int)timeT % 100;
                    int timeMSec = (int)((timeT - (int)timeT) * 1000);
                    //get time
                    GPSTime = new DateTime(1900, 1, 1, timeHr, timeMin, timeSec, timeMSec);
                    //get lat
                    double LatData = Convert.ToDouble(fields[2]);
                    int LatDeg = (int)LatData / 100;
                    double LatMin = LatData - LatDeg * 100.0;
                    double LatOri = fields[3].StartsWith("N") ? 1.0 : -1.0;
                    Latitude = LatOri * ((double)LatDeg + (double)LatMin / 60.0);
                    //get long
                    double LonData = Convert.ToDouble(fields[4]);
                    int LonDeg = (int)LonData / 100;
                    double LonMin = LonData - LonDeg * 100.0;
                    double LonOri = fields[5].StartsWith("E") ? 1.0 : -1.0;
                    Longitude = LonOri * ((double)LonDeg + (double)LonMin / 60.0);
                    //get validation
                    GoodData = Convert.ToInt32(fields[6]) > 0;
                    
                    //count Satellite
                    SatelliteCount = Convert.ToInt32(fields[7]);
                    //Altitude
                    Altitude = Convert.ToDouble(fields[9]);
                }
                catch (Exception)
                {
                    GoodData = false;
                }
            }
            if (RawData.NMEA.IndexOf("$GPRMC") > -1)
            {
                try
                {
                    string[] fields = RawData.NMEA.Split(',');

                    //parse time
                    int timeT = Convert.ToInt32(fields[1].Substring(0, 6));
                    int timeHr = timeT / 10000;
                    int timeMin = (timeT % 10000) / 100;
                    int timeSec = timeT % 100;
                    int timeMSec = Convert.ToInt32(fields[1].Substring(7, 3));
                    //parse day
                    int timeD = Convert.ToInt32(fields[9]);
                    int timeDay = timeD / 10000;
                    int timeMon = (timeD % 10000) / 100;
                    int timeYr = timeD % 100;
                    //get time
                    GPSTime = new DateTime(timeYr, timeMon, timeDay, timeHr, timeMin, timeSec, timeMSec);
                    //get validation
                    GoodData = fields[2].StartsWith("A");
                    //get lat
                    double LatData = Convert.ToDouble(fields[3]);
                    int LatDeg = (int)LatData / 100;
                    double LatMin = LatData - LatDeg * 100.0;
                    double LatOri = fields[4].StartsWith("N") ? 1.0 : -1.0;
                    Latitude = LatOri * ((double)LatDeg + (double)LatMin / 60.0);
                    //get long
                    double LonData = Convert.ToDouble(fields[5]);
                    int LonDeg = (int)LonData / 100;
                    double LonMin = LonData - LonData * 100.0;
                    double LonOri = fields[6].StartsWith("E") ? 1.0 : -1.0;
                    Longitude = LonOri * ((double)LonDeg + (double)LonMin / 60.0);

                    //get ground speed
                    GroundSpeed = Convert.ToDouble(fields[7]);
                    //get track angle
                    TrackAngle = Convert.ToDouble(fields[8]);
                }
                catch (Exception)
                {
                    GoodData = false;
                }
            }
        }
    }
    /// <summary>
    /// A package of GPS data
    /// </summary>
    public class GPSPackage : SensorPackage
    {
        /// <summary>
        /// NMEA string of GPS
        /// </summary>
        public string NMEA;
    }
}
