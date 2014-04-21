using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIR.Maths
{
    /*
    public class KalmanFilter
    {
        /// <summary>
        /// State Matrix X
        /// </summary>
        public Matrix State { get; private set; }
        /// <summary>
        /// Covariance Matrix P
        /// </summary>
        public Matrix Covariance { get; private set; }
        /// <summary>
        /// Measurement Matrix H
        /// </summary>
        public Matrix Measure;
        /// <summary>
        /// Transformation Matrix
        /// </summary>
        public Matrix Transform;
        /// <summary>
        /// Process Noise Matrix
        /// </summary>
        public Matrix ProcessNoise;
        /// <summary>
        /// Measurement Noise Matrix
        /// </summary>
        public Matrix MeasurementNoise;

        Matrix K;
        Matrix y;
        Matrix s;

        public KalmanFilter(Matrix State, Matrix Covariance)
        {
            this.State = State;
            this.Covariance = Covariance;
        }

        public void update(Matrix z)
        {
            try
            {
                y = z - (Measure * State);
                s = (Measure * Covariance) * Measure.transpose() + MeasurementNoise;
                K = (Covariance * Measure) * s.inverse();
                State = State + (K * y);
                Covariance = Covariance - ((K * Measure) * Covariance);
            }
            catch (Exception)
            { }
            predict();
        }

        private void predict()
        {
            try
            {
                State = Transform * State;
                Covariance = ProcessNoise + ((Transform * Covariance) * Transform.transpose());
            }
            catch (Exception)
            { }
        }

        public void setMeasurementNoiseMatrix(Matrix r)
        {
            this.MeasurementNoise = r;
        }
        
    }
    */
}
