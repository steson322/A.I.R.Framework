//Written By Steven Song 2014
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIR.Maths;

namespace AIR.Learning
{
    /// <summary>
    /// a Perceptron Learner
    /// </summary>
    public partial class PerceptronLearner
    {
        /// <summary>
        /// In Samples
        /// </summary>
        List<DataPoint<double>> InSamples { get; set; }

        /// <summary>
        /// Weight of each degree of dimesion
        /// </summary>
        public double[] Weights { get; set; }

        /// <summary>
        /// Dimension of the perceptron
        /// </summary>
        public int Dimension { get; private set; }

        /// <summary>
        /// Hypothesis of leaning model
        /// (x, y) = > x, y*y etc
        /// </summary>
        public Func<double[], double[]> Hypothesis = LinearHypothesis;

        /// <summary>
        /// Constructor of Perceptron learner
        /// </summary>
        /// <param name="Dimesion">Dimension of learner hypothesis</param>
        public PerceptronLearner(int Dimension)
        {
            //get dimension
            this.Dimension = Dimension;

            //weight of learning
            Weights = new double[Dimension + 1];

            //reset W
            for (int i = 0; i < Dimension + 1; i++)
                Weights[i] = 0;
        }

        /// <summary>
        /// Train the learner with in samples
        /// </summary>
        /// <param name="InSamples">Input training samples</param>
        /// <param name="IterationLimit">Iteration limit of learner</param>
        public void Train(List<DataPoint<double>> InSamples, int IterationLimit)
        {
            //copy in sample
            this.InSamples = InSamples;
            //correct the data with linear regression
            LinearRegression();
            //apply PLA
            PLA(IterationLimit);
        }

        /// <summary>
        /// A Hypothesis that
        /// </summary>
        /// <param name="Weights"></param>
        /// <param name="Coordinate"></param>
        /// <returns></returns>
        public static double[] LinearHypothesis( double[] Coordinate)
        {
            return Coordinate;
            /*
            double calc = Weights[0];
            for (int i = 1; i < Weights.Length; i++)
                calc += Weights[i] * Coordinate[i - 1];
            return calc;*/
        }

        /// <summary>
        /// Estimate value based on learning at a certain location
        /// </summary>
        /// <param name="Coordinate"></param>
        /// <returns></returns>
        public double EstimateValue(double[] Coordinate)
        {
            double[] AdjustedCoordinates = Hypothesis(Coordinate);
            return Compute(Weights, Coordinate);
        }

        /// <summary>
        /// Compute current coordinate
        /// </summary>
        /// <param name="weights"></param>
        /// <param name="Coordinate"></param>
        /// <returns></returns>
        double Compute(double[] weights, double[] Coordinate)
        {
            double[] AdjustedCoordinate = Hypothesis(Coordinate);
            double calc = weights[0];
            for (int i = 0; i < Dimension; i++)
                calc += Weights[i + 1] * AdjustedCoordinate[i];
            return calc;
        }

        /// <summary>
        /// Use linear regression to calculate a ideal initial gain
        /// </summary>
        void LinearRegression()
        {
            //initialize inSamples x
            double[,] X = new double[InSamples.Count, Dimension + 1];
            //copy insample x
            for (int i = 0; i < InSamples.Count; i++)
            {
                double[] AdjustedCoordinate = Hypothesis(InSamples[i].Coordinate);
                X[i, 0] = 1;
                for (int j = 0; j < Dimension; j++)
                    X[i, j + 1] = AdjustedCoordinate[j];
            }

            //initialize inSamples y
            double[] Y = new double[InSamples.Count];
            //copy insample y
            for (int i = 0; i < InSamples.Count; i++)
                Y[i] = InSamples[i].Value;

            Matrix mX = new Matrix(X);
            Matrix mY = new Matrix(Y);
            mY = mY.Transpose;
            Matrix mW = mX.PseudoInverse * mY;
            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = mW.Get(i, 0);
        }

        /// <summary>
        /// Iteration using Perceptron learning algorithm
        /// </summary>
        /// <param name="limit">limit of iteration</param>
        /// <returns></returns>
        int PLA(int limit)
        {
            double ErrorMin = MeasureError();
            double[] weight = (double[])Weights.Clone();
            int converge = 0;
            for (int itr = 0; itr < limit; itr++)
            {
                int point = itr % InSamples.Count;
                double calc = Compute(weight, InSamples[point].Coordinate);
                if(calc * InSamples[point].Value <= 0)
                {
                    //generate new weight
                    weight[0] += InSamples[point].Value;
                    double[] AdjustedCoordinate = Hypothesis(InSamples[point].Coordinate);
                    for (int i = 1; i < Weights.Length; i++)
                        weight[i] += InSamples[point].Value * AdjustedCoordinate[i - 1];
                    converge = itr + 1;
                    //pocket alg
                    double Error = MeasureError(weight);
                    if (Error < ErrorMin)
                    {
                        //update best apporach
                        ErrorMin = Error;
                        //update the weight value of best
                        Weights = (double[])weight.Clone();
                    }
                }
            }
            return converge;
        }

        public int testOutSample(List<DataPoint<double>> OutSamples)
        {
            int right = 0;
            //determine
            foreach(DataPoint<double> dp in OutSamples)
            {
                double calc = EstimateValue(dp.Coordinate);
                if ((calc > 0 && dp.Value > 0) || (calc < 0 && dp.Value < 0))
                    right++;
            }
            return right;
        }

        /// <summary>
        /// Test in sample with a specified weight
        /// </summary>
        /// <param name="weight"></param>
        /// <returns></returns>
        double MeasureError(double[] weight)
        {//test the loaded weight
            double distance = 0;
            //determine
            for (int itr = 0; itr < InSamples.Count; itr++)
            {
                double calc = Compute(weight, InSamples[itr].Coordinate);
                distance += Math.Abs(InSamples[itr].Value - calc);
            }
            return distance;
        }

        /// <summary>
        /// Test in sample with current weight
        /// </summary>
        /// <returns></returns>
        public double MeasureError()
        {//test the currect weight
            return MeasureError(Weights);
        }
    }
}
