using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHLowlevelLayer
{
    public class PolynomialTrajectoryPlanningStrategy : ITrajectoryPlanningStrategy
    {
        double Ts;
        double eps;
        double maxMoveTime;

        public PolynomialTrajectoryPlanningStrategy(double timeStep, double MaxTime)
        {
            Ts = timeStep;
            eps = Ts / 2;
            maxMoveTime = MaxTime;
            int size = (int)Math.Floor(maxMoveTime / Ts);
            maxMoveTime = size * Ts;
        }

        public override double[][,] TrajectoryPlanning(PointParams initialConditions, PointParams finalConditions)
        {
            if (finalConditions == null)
                return null;

            double time = (finalConditions.T - initialConditions.T).TotalSeconds;
            if ((double.IsInfinity(time)) || (time < 0))
                return null;
            if (time < Ts)
                time = Ts;

            try
            {
                double[,] initialConditionsArray;
                double[,] finalConditionsArray;
                int counter = 0;
                double finalTime, startTime = 0.0;
                int size = (int)Math.Floor(time / Ts);
                time = size * Ts;
                finalTime = time;
                double[][,] trajectory = new double[2][,];
                trajectory[0] = new double[2, size];
                trajectory[1] = new double[2, size];

                // check if motion is too long (longer than MaxMoveTime)
                if (time > maxMoveTime)
                {
                    startTime = time - maxMoveTime;
                    finalTime = maxMoveTime;
                    // zero velocity
                    initialConditions.EditParamByIndex(2, new Point(0, 0));
                    initialConditionsArray = initialConditions.GetParamsAsArray();

                    // motion is too long, setting an initial resting period
                    for (double t = Ts; t <= startTime + eps; t = t + Ts)
                    {
                        // Velocity 
                        trajectory[1][0, counter] = 0;
                        trajectory[1][1, counter] = 0;

                        // path
                        trajectory[0][0, counter] = initialConditionsArray[0, 0];
                        trajectory[0][1, counter] = initialConditionsArray[1, 0];

                        t = Math.Round(t * 1000) / 1000;
                        counter++;
                    } 
                }
                else
                {
                    initialConditionsArray = initialConditions.GetParamsAsArray();
                }

                finalConditionsArray = finalConditions.GetParamsAsArray();


                // check if a degenerated trajectory
                var equal =
                    initialConditionsArray.Rank == finalConditionsArray.Rank &&
                    Enumerable.Range(0, initialConditionsArray.Rank).All(dimension => initialConditionsArray.GetLength(dimension) == finalConditionsArray.GetLength(dimension)) &&
                    initialConditionsArray.Cast<double>().SequenceEqual(finalConditionsArray.Cast<double>());

                Matrix X1, Y1;
                if (equal)
                {
                    X1 = Matrix.Parse("0\r\n0\r\n0\r\n0");
                    Y1 = Matrix.Parse("0\r\n0\r\n0\r\n0");
                }
                else
                {
                    X1 = CalcSpline3(new double[] { initialConditionsArray[0, 0], initialConditionsArray[0, 1] },
                                                new double[] { finalConditionsArray[0, 0], finalConditionsArray[0, 1] },
                                                time-startTime);

                    Y1 = CalcSpline3(new double[] { initialConditionsArray[1, 0], initialConditionsArray[1, 1] },
                                            new double[] { finalConditionsArray[1, 0], finalConditionsArray[1, 1] },
                                            time-startTime);
                }

                // calculate motion for start/resting position
                
                for (double t = Ts; t <= finalTime + eps; t = t + Ts)
                {
                    // Velocity 
                    trajectory[1][0, counter] = Math.Round((X1.mat[0, 0] * 3 * t * t + X1.mat[1, 0] * 2 * t + X1.mat[2, 0])* 1000)/1000;
                    trajectory[1][1, counter] = Math.Round((Y1.mat[0, 0] * 3 * t * t + Y1.mat[1, 0] * 2 * t + Y1.mat[2, 0])* 1000)/1000;

                    // path
                    trajectory[0][0, counter] = X1.mat[0, 0] * t * t * t + X1.mat[1, 0] * t * t + X1.mat[2, 0] * t + X1.mat[3, 0];
                    trajectory[0][1, counter] = Y1.mat[0, 0] * t * t * t + Y1.mat[1, 0] * t * t + Y1.mat[2, 0] * t + Y1.mat[3, 0];

                    t = Math.Round(t * 1000) / 1000;
                    counter++;
                }

                return trajectory;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private Matrix CalcSpline3(double[] initialConditions, double[] finalConditions, double time)
        {
            Matrix A = Matrix.Parse("0 0 0 1\r\n" +
                                    "0 0 1 0\r\n" +
                                    Math.Pow(time, 3).ToString() + " " + Math.Pow(time, 2) + " " + time.ToString() + " 1\r\n" +
                                    (3 * Math.Pow(time, 2)).ToString() + " " + (2 * time).ToString() + " 1 0");

            Matrix B = Matrix.Parse(initialConditions[0].ToString() + "\r\n" + initialConditions[1].ToString() + "\r\n" +
                                    finalConditions[0].ToString() + "\r\n" + finalConditions[1].ToString());

            Matrix coeff = A.Invert() * B;
            return coeff;
        }
    }
}
