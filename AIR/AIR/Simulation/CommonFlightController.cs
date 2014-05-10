using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// Constructor of Common Controller
        /// </summary>
        public CommonFlightController()
        {

        }
    }
}
