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
        public enum DistanceMode
        {
            Direct,
            Linear
        }

        /// <summary>
        /// Distance mode of knn
        /// </summary>
        public DistanceMode Mode = DistanceMode.Direct;

        /// <summary>
        /// Data points for KNN
        /// </summary>
        public List<DataPoint3D<int>> DataPoints { get; private set; }

        /// <summary>
        /// Center of cluster
        /// </summary>
        public List<PointF> ClusterCenters { get; private set; }

        /// <summary>
        /// Constructor of KNN Cluster
        /// </summary>
        public KNNClusterLearner()
        {
            DataPoints = new List<DataPoint3D<int>>();
            ClusterCenters = new List<PointF>();
        }

        public void Iterate()
        {
            //define cluster
            Parallel.ForEach(DataPoints.ToArray(), points =>
            {
                //calculate closest cluster
                int center = 0;
                float min = float.MaxValue;
                for (int c = 0; c < ClusterCenters.Count; c++)
                {
                    float dist = 0;
                    if (Mode == DistanceMode.Direct)
                    {
                        dist = (float)Math.Abs(points.X - ClusterCenters[c].X)
                            + (float)Math.Abs(points.Y - ClusterCenters[c].Y);
                    }
                    else if (Mode == DistanceMode.Linear)
                    {
                        dist = (float)Math.Sqrt(
                            Math.Pow(points.X - ClusterCenters[c].X, 2)
                            + Math.Pow(points.Y - ClusterCenters[c].Y, 2));
                    }
                    if (dist < min)
                    {
                        min = dist;
                        center = c + 1;
                    }
                }
                //append final
                points.Value = center;
                //DataPoints[points.] = center;
            });

            if (ClusterCenters.Count > 0)
            {
                //calculate new center
                PointF[] Average = new PointF[ClusterCenters.Count];
                int[] count = new int[ClusterCenters.Count];
                foreach (var points in DataPoints.ToArray())
                {
                    Average[points.Value - 1].X += (float)points.X;
                    Average[points.Value - 1].Y += (float)points.Y;
                    count[points.Value - 1]++;
                }
                //calculate average
                for (int i = 0; i < Average.Length; i++)
                {
                    Average[i].X /= (float)count[i];
                    Average[i].Y /= (float)count[i];
                    ClusterCenters[i] = Average[i];
                }
            }
        }
    }
}
