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
        /// In sample x of datapoint
        /// </summary>
        double[,] X;

        /// <summary>
        /// In sample y of datapoint
        /// </summary> 
        double[] Y;

        /// <summary>
        /// Weight of each degree of dimesion
        /// </summary>
        public double[] Weights { get; set; }

        /// <summary>
        /// Dimension of the perceptron
        /// </summary>
        public int Dimension { get; set; }

        /// <summary>
        /// Constructor of Perceptron learner
        /// </summary>
        /// <param name="Dimesion">Dimension of learner</param>
        public PerceptronLearner(int Dimension)
        {
            //get dimension
            this.Dimension = Dimension;

            //weight of learning
            Weights = new double[Dimension + 1];

            //reset W
            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = 0;
        }

        /// <summary>
        /// Train the learner with in samples
        /// </summary>
        /// <param name="InSamples">Input training samples</param>
        /// <param name="IterationLimit">Iteration limit of learner</param>
        public void Train(List<DataPoint<double>> InSamples, int IterationLimit)
        {
            //initialize inSamples x
            X = new double[InSamples.Count, Dimension + 1];
            //copy insample x
            for (int i = 0; i < InSamples.Count; i++)
                for (int j = 0; j < Dimension + 1; j++)
                    X[i, j] = InSamples[i].Coordinate[j];

            //initialize inSamples y
            Y = new double[InSamples.Count];
            //copy insample y
            for (int i = 0; i < InSamples.Count; i++)
                Y[i] = InSamples[i].Value;

            //correct the data with linear regression
            LinearRegression();
            //apply PLA
            PLA(IterationLimit);
        }

        /// <summary>
        /// Use linear regression to calculate a ideal initial gain
        /// </summary>
        void LinearRegression()
        {
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
            int RightMax = testInSample();
            double[] weight = (double[])Weights.Clone();
            int converge = 0;
            for (int itr = 0; itr < limit; itr++)
            {
                int point = itr % X.GetLength(0);
                //determine if the point is correct
                double calc = 0;
                for (int i = 0; i < Weights.Length; i++)
                    calc += weight[i] * X[point, i];
                //in case of a in match
                if ((calc > 0 && Y[point] < 0) || (calc < 0 && Y[point] > 0))
                {
                    //generate new weight
                    for (int i = 0; i < Weights.Length; i++)
                        weight[i] += Y[point] * X[point, i];
                    converge = itr + 1;
                    //pocket alg
                    int Right = testInSample(weight);
                    if (Right > RightMax)
                    {
                        //update best apporach
                        RightMax = Right;
                        //update the weight value of best
                        Weights = (double[])weight.Clone();
                    }
                }
            }
            return converge;
        }

        public int testOutSample(List<DataPoint<double>> OutSamples)
        {
            //initialize outSamples x
            double[,] outX = new double[OutSamples.Count, Dimension + 1];
            //copy insample x
            for (int i = 0; i < OutSamples.Count; i++)
                for (int j = 0; j < Dimension + 1; j++)
                    outX[i, j] = OutSamples[i].Coordinate[j];

            //initialize outSamples y
            double[] outY = new double[OutSamples.Count];
            //copy insample y
            for (int i = 0; i < OutSamples.Count; i++)
                outY[i] = OutSamples[i].Value;

            int right = 0;
            //determine
            for (int itr = 0; itr < outX.GetLength(0); itr++)
            {
                double calc = 0;
                for (int i = 0; i < Weights.Length; i++)
                    calc += Weights[i] * outX[itr, i];
                if ((calc > 0 && outY[itr] > 0) || (calc < 0 && outY[itr] < 0))
                    right++;
            }

            int correctCount = 0;
            foreach (DataPoint<double> dp in OutSamples)
            {
                double sum = 0;
                for (int i = 0; i < Weights.Length; i++)
                    sum += Weights[i] * dp.Coordinate[i];
                if ((sum > 0 && dp.Value > 0) || (sum < 0 && dp.Value < 0))
                    correctCount++;
            }
            return right;
        }

        /// <summary>
        /// Test in sample with a specified weight
        /// </summary>
        /// <param name="weight"></param>
        /// <returns></returns>
        int testInSample(double[] weight)
        {//test the loaded weight
            int right = 0;
            //determine
            for (int itr = 0; itr < X.GetLength(0); itr++)
            {
                double calc = 0;
                for (int i = 0; i < Weights.Length; i++)
                    calc += weight[i] * X[itr, i];
                if ((calc > 0 && Y[itr] > 0) || (calc < 0 && Y[itr] < 0))
                    right++;
            }
            return right;
        }

        /// <summary>
        /// Test in sample with current weight
        /// </summary>
        /// <returns></returns>
        public int testInSample()
        {//test the currect weight
            return testInSample(Weights);
        }
    }
}
