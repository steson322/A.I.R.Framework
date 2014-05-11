using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIR.Maths;

namespace AIR.Simulation
{
    /// <summary>
    /// Controller with most case senario in flight gear
    /// </summary>
    public class CommonFlightController : FlightController
    {
        [ToReceive]
        [Node(@"/orientation/pitch-deg")]
        public double Pitch;

        [ToReceive]
        [Node(@"/orientation/roll-deg")]
        public double Roll;

        [ToReceive]
        [Node(@"/orientation/heading-deg")]
        public double Heading;

        [ToReceive]
        [Node(@"/position/latitude-deg")]
        public double Latitude;

        [ToReceive]
        [Node(@"/position/longitude-deg")]
        public double Longitude;

        [ToReceive]
        [Node(@"/position/altitude-ft")]
        public double Altitude;

        [ToReceive]
        [Node(@"/velocities/groundspeed-kt")]
        public double GroundSpeed;

        [ToSend]
        [Node(@"/controls/engines/engine/throttle")]
        public double Throttle;

        [ToSend]
        [Node(@"/controls/gear/brake-parking")]
        public double Brake;

        [ToSend]
        [Node(@"/controls/gear/gear-down")]
        public bool LandingGear;

        [ToSend]
        [Node(@"/controls/flight/aileron")]
        public double Aileron;

        [ToSend]
        [Node(@"/controls/flight/elevator")]
        public double Elevator;

        [ToSend]
        [Node(@"/controls/flight/rudder")]
        public double Rudder;

        #region Controllers

        /// <summary>
        /// Interval of fast control
        /// </summary>
        public int FastInterval;

        /// <summary>
        /// Interval of slow control
        /// </summary>
        public int SlowInterval;

        /// <summary>
        /// Pitch PID
        /// </summary>
        public PID PitchPID;

        /// <summary>
        /// Roll PID
        /// </summary>
        public PID RollPID;

        /// <summary>
        /// Altitude PID
        /// </summary>
        public PID AltitudePID;

        /// <summary>
        /// Heading PID
        /// </summary>
        public PID HeadingPID;

        /// <summary>
        /// Expected Altitude
        /// </summary>
        public double CruiseAltitude;

        /// <summary>
        /// Expected Heading
        /// </summary>
        public double CruiseHeading;

        /// <summary>
        /// Expected Speed
        /// </summary>
        public double CruiseSpeed;

        #endregion Controllers

        /// <summary>
        /// Constructor of Common Controller
        /// </summary>
        public CommonFlightController()
        {
            FastInterval = 2 * Settings.ControlInterval;
            SlowInterval = 10 * FastInterval;
            double fastFreq = 1000 / FastInterval;
            double slowFreq = 1000 / SlowInterval;
            PitchPID = new PID(fastFreq, 1, 0, 0, 0.5);
            RollPID = new PID(fastFreq, 1.4, 0.2, 0.01, 0.5);
            AltitudePID = new PID(slowFreq, 1, 0, 0, Math.PI / 3.0);
            HeadingPID = new PID(slowFreq, 1, 0, 0, Math.PI / 3.0);
            Behavior = DoWork;
        }

        void DoWork()
        {
            double ExpectedPitch = 0;
            double ExpectedRoll = 0;

            //fast dynamic
            System.Timers.Timer FastDynamic = new System.Timers.Timer(FastInterval);
            FastDynamic.Elapsed += new System.Timers.ElapsedEventHandler((obj, args) =>
            {
                double PitchRadian = Pitch * Math.PI / 180.0;
                double RollRadian = Roll * Math.PI / 180.0;
                Elevator = PitchPID.Feed(ExpectedPitch, -PitchRadian);
                Aileron = RollPID.Feed(ExpectedRoll, RollRadian);
            });
            
            //slow dynamic
            System.Timers.Timer SlowDynamic = new System.Timers.Timer(SlowInterval);
            SlowDynamic.Elapsed += new System.Timers.ElapsedEventHandler((obj, args) =>
            {
                ExpectedPitch = PitchPID.Feed(CruiseAltitude, Altitude);
                ExpectedRoll = RollPID.Feed(CruiseHeading, Heading);
            });

            FastDynamic.Start();
            //SlowDynamic.Start();
        }
    }
}
