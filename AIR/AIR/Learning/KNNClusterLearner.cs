//Written By Steven Song 2014
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AIR.Learning
{
    public class KNNClusterLearner
    {
        /// <summary>
        /// The way to calculate distance in KNN
        /// </summary>
        public enum DistanceMode
        {
            /// <summary>
            /// Direct vector distance
            /// </summary>
            Direct,
            /// <summary>
            /// Linear Geometry distance
            /// </summary>
            Linear
        }

        /// <summary>
        /// Dimension of KNN Learner
        /// </summary>
        public int Dimension { get; private set; }

        /// <summary>
        /// Distance mode of knn
        /// </summary>
        public DistanceMode Mode = DistanceMode.Direct;

        #region Datapoints

        /// <summary>
        /// Data points for KNN
        /// </summary>
        Dictionary<DataPoint<double>, int> DataPoints { get; set; }

        /// <summary>
        /// Add one datapoint
        /// </summary>
        /// <param name="dp"></param>
        public void AddDatapoint(DataPoint<double> dp)
        {
            if (dp.Dimension != this.Dimension)
                throw new IndexOutOfRangeException();
            DataPoints[dp] = 0;
        }

        /// <summary>
        /// Add a list of data points
        /// </summary>
        /// <param name="dps"></param>
        public void AddDatapoints(List<DataPoint<double>> dps)
        {
            foreach (var dp in dps)
                AddDatapoint(dp);
        }

        /// <summary>
        /// Get the cluster center id of dp
        /// </summary>
        /// <param name="dp"></param>
        public int GetDatapointClusterCenter(DataPoint<double> dp)
        {
            if (DataPoints.ContainsKey(dp))
                return DataPoints[dp];
            throw new IndexOutOfRangeException();
        }

        #endregion Datapoints

        /// <summary>
        /// Center of cluster
        /// </summary>
        public List<double[]> ClusterCenters { get; private set; }

        /// <summary>
        /// Constructor of KNN Cluster
        /// </summary>
        public KNNClusterLearner(int Dimension)
        {
            this.Dimension = Dimension;
            DataPoints = new Dictionary<DataPoint<double>, int>();
            ClusterCenters = new List<double[]>();
        }

        /// <summary>
        /// Get distance from one datapoint to center
        /// </summary>
        /// <param name="datapoint"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        double GetDistance(DataPoint<double> datapoint, double[] center)
        {
            double dist = 0;
            if (Mode == DistanceMode.Direct)
            {
                for (int i = 0; i < Dimension; i++)
                    dist += Math.Abs(datapoint.Coordinate[i] - center[i]);
            }
            else if (Mode == DistanceMode.Linear)
            {
                double sum = 0;
                for (int i = 0; i < Dimension; i++)
                    sum += Math.Pow(datapoint.Coordinate[i] - center[i], 2.0);
                dist = Math.Sqrt(sum);
            }
            return dist;
        }

        /// <summary>
        /// Iterate KNN once
        /// </summary>
        public void Iterate()
        {
            //define cluster
            Parallel.ForEach(DataPoints.Keys.ToArray(), points =>
            {
                //calculate closest cluster
                int center = 0;
                double min = double.MaxValue;
                for (int c = 0; c < ClusterCenters.Count; c++)
                {
                    //get center distance
                    double dist = GetDistance(points, ClusterCenters[c]);
                    if (dist < min)
                    {
                        min = dist;
                        center = c + 1;
                    }
                }
                //append final
                DataPoints[points] = center;
            });

            if (ClusterCenters.Count > 0)
            {
                //calculate new center
                double[,] Average = new double[ClusterCenters.Count, Dimension];
                int[] count = new int[ClusterCenters.Count];
                foreach (var points in DataPoints.Keys.ToArray())
                {
                    for (int i = 0; i < Dimension; i++)
                        Average[DataPoints[points] - 1, i] += (float)points.Coordinate[i];
                    count[DataPoints[points] - 1]++;
                }
                //calculate average
                for (int a = 0; a < Average.GetLength(0); a++)
                {
                    for (int i = 0; i < Dimension; i++)
                        Average[a,i] /= (double)count[a];
                    for (int i = 0; i < Dimension; i++)
                        ClusterCenters[a][i] = Average[a, i];
                }
            }
        }

        public double Estimate(double[] coordinate, int IterationCount)
        {
            if(coordinate.Length != Dimension)
                throw new IndexOutOfRangeException();
            //create a new datapoint
            DataPoint<double> ndp = new DataPoint<double>(Dimension);
            for (int i = 0; i < Dimension; i++)
                ndp.Coordinate[i] = coordinate[i];
            //add new datapoint
            DataPoints[ndp] = 0;
            //iterate with new datapoint
            for (int i = 0; i < IterationCount; i++)
                Iterate();
            //get center id
            int Center = DataPoints[ndp];
            double value = 0;
            double count = 0;
            foreach(var kv in DataPoints)
                if (kv.Value == Center && kv.Key != ndp)
                {
                    value += kv.Key.Value;
                    count++;
                }
            value /= (double)count;
            //remove new datapoint
            DataPoints.Remove(ndp);
            return value;
        }
    }
}
