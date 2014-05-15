//Written By Steven Song 2014
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIR.Learning
{
    /// <summary>
    /// 2 Dimesional Data Point
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataPoint2D<T> : DataPoint<T>
    {
        /// <summary>
        /// X coordinate
        /// </summary>
        public double X
        {
            get
            {
                return Coordinate[0];
            }
            set
            {
                Coordinate[0] = value;
            }
        }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public double Y
        {
            get
            {
                return Coordinate[1];
            }
            set
            {
                Coordinate[1] = value;
            }
        }

        /// <summary>
        /// Constructor of 2 Dimension Datapoint
        /// </summary>
        public DataPoint2D()
            : base(2)
        { }
    }

    /// <summary>
    /// 3 Dimesional Data Point
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataPoint3D<T> : DataPoint<T>
    {
        /// <summary>
        /// X coordinate
        /// </summary>
        public double X
        {
            get
            {
                return Coordinate[0];
            }
            set
            {
                Coordinate[0] = value;
            }
        }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public double Y
        {
            get
            {
                return Coordinate[1];
            }
            set
            {
                Coordinate[1] = value;
            }
        }

        /// <summary>
        /// Z coordinate
        /// </summary>
        public double Z
        {
            get
            {
                return Coordinate[2];
            }
            set
            {
                Coordinate[2] = value;
            }
        }

        /// <summary>
        /// Constructor of 3 Dimension Datapoint
        /// </summary>
        public DataPoint3D()
            : base(3)
        { }
    }

    /// <summary>
    /// A data point
    /// </summary>
    public class DataPoint<T>
    {
        /// <summary>
        /// Get Dimension of the DataPoint
        /// </summary>
        public int Dimension { get; private set; }

        /// <summary>
        /// Get vector coordinate of the DataPoint
        /// Looks like {1 , x1, x2, x3}
        /// </summary>
        public double[] Coordinate { get; private set; }

        /// <summary>
        /// Get value of the DataPoint
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Constructor of Data Point
        /// </summary>
        /// <param name="Dimension"></param>
        public DataPoint(int Dimension)
        {
            this.Dimension = Dimension;
            this.Coordinate = new double[Dimension];
            this.Coordinate[0] = 1;
        }

        /// <summary>
        /// Set the coordinate and value of data point
        /// </summary>
        /// <param name="Coordinate">Coordinate of DataPoint</param>
        /// <param name="Value">Value of DataPoint</param>
        /// <returns></returns>
        public bool Set(double[] Coordinate, T Value)
        {
            if (Coordinate.Length != Dimension)
                return false;
            //copy coordinate
            for (int i = 0; i < Dimension; i++)
                this.Coordinate[i] = Coordinate[i];
            //copy value
            this.Value = Value;
            return true;
        }
    }
}
