﻿using System;
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
        /// Elevator PID
        /// </summary>
        public PID ElevatorPID;

        /// <summary>
        /// Aileron PID
        /// </summary>
        public PID AileronPID;

        /// <summary>
        /// Rudder PID
        /// </summary>
        public PID RudderPID;

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
            FastInterval = Settings.ControlInterval;
            SlowInterval = 5 * FastInterval;
            ElevatorPID = new PID(FastInterval, 2.6, 1.4, 0.5, 0.5);
            AileronPID = new PID(FastInterval, 0.7, 0.8, 0.8, 0.5);
            AltitudePID = new PID(SlowInterval, 0.0005, 0.00001, 0.001, Math.PI / 4.0);
            HeadingPID = new PID(SlowInterval, 1, 0.1, 0, Math.PI / 4.0);
            Behavior = DoWork;
        }


        public double ExpectedPitch = 0;
        public double ExpectedRoll = 0;

        void DoWork()
        {

            //fast dynamic
            System.Timers.Timer FastDynamic = new System.Timers.Timer(FastInterval);
            FastDynamic.Elapsed += new System.Timers.ElapsedEventHandler((obj, args) =>
            {
                double PitchRadian = Pitch * Math.PI / 180.0;
                double RollRadian = Roll * Math.PI / 180.0;
                double PitchError = ExpectedPitch - PitchRadian;
                PitchError = (PitchError + Math.PI) % (Math.PI * 2.0) - Math.PI;
                double RollError = ExpectedRoll - RollRadian;
                RollError = (RollError + Math.PI) % (Math.PI * 2.0) - Math.PI;
                Aileron = AileronPID.Feed(RollError/* * Math.Cos(PitchRadian)*/);
                Elevator = -ElevatorPID.Feed(PitchError * Math.Cos(RollRadian));
            });
            
            //slow dynamic
            System.Timers.Timer SlowDynamic = new System.Timers.Timer(SlowInterval);
            SlowDynamic.Elapsed += new System.Timers.ElapsedEventHandler((obj, args) =>
            {
                ExpectedPitch = AltitudePID.Feed(CruiseAltitude, Altitude);

                double HeadingRadian = Heading * Math.PI / 180.0;
                HeadingRadian = (HeadingRadian + Math.PI) % (Math.PI * 2.0) - Math.PI;
                double HeadingError = CruiseHeading - HeadingRadian;
                HeadingError = (HeadingError + Math.PI) % (Math.PI * 2.0) - Math.PI;
                ExpectedRoll = HeadingPID.Feed(HeadingError);
            });

            FastDynamic.Start();
            SlowDynamic.Start();
        }
    }
}
