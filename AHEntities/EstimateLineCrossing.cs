using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public static class EstimateLineCrossing
    {
        public static double goalLine = -1180;
        public static double blockLine = -900;
        public static double defenseAttackLine = -800.0;
        public static double attackLine = -750;
        private static double eps = 0.01;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="puckP">Puck position at starting estimation point</param>
        /// <param name="puckV">Puck velocity at starting estimation point</param>
        /// <param name="puckR">Puck radius</param>
        /// <param name="xLine">Crossing line</param>
        /// <param name="tableW">Table width - X size</param>
        /// <param name="tableH">Table height - Y size</param>
        /// <returns>(y,t,vx,vy):
        /// y - where on the line the cross occurs
        /// t - the time it takes the puck to get to the crossing line
        /// </returns>
        public static double[] Estimate(Point puckP, Point puckV, double puckR, double xLine, int tableW, int tableH)
        {
            // The puck is moving away from the agent
            if (puckV.X >= eps)
                return null;

            if (Math.Abs(puckV.X) < eps)
                return null;

            Point xTemp = new Point(puckP);
            double EffectiveTableY = tableH / 2 - (3 + puckR);
            double time = 0.0;
            double tempTime = 0.0;
            Point temp = new Point(0,0);
            Point tempV = new Point(puckV);

            while (Math.Abs(xTemp.X - xLine) > eps)
            {
                if (puckV.Y > 0)
                {
                    #region Vy>0
                    tempTime = (EffectiveTableY - xTemp.Y) / puckV.Y;
                    if (xTemp.X + tempTime * puckV.X - xLine > eps)
                    {
                        temp.X = xTemp.X + tempTime * puckV.X;
                        temp.Y = EffectiveTableY;
                        time = time + tempTime;
                    }
                    else
                    {
                        tempTime = (xLine - xTemp.X) / puckV.X;
                        temp.X = xLine;
                        temp.Y = xTemp.Y + tempTime * puckV.Y;
                        time = time + tempTime;
                    }
                    #endregion Vy>0
                }
                else if (puckV.Y < 0)
                {
                    #region Vy<0
                    tempTime = (-EffectiveTableY - xTemp.Y) / puckV.Y;
                    if (xTemp.X + tempTime * puckV.X - xLine > eps)
                    {
                        temp.X = xTemp.X + tempTime * puckV.X;
                        temp.Y = -EffectiveTableY;
                        time = time + tempTime;
                    }
                    else
                    {
                        tempTime = (xLine - xTemp.X) / puckV.X;
                        temp.X = xLine;
                        temp.Y = xTemp.Y + tempTime * puckV.Y;
                        time = time + tempTime;
                    }
                    #endregion Vy<0
                }
                else
                {
                    #region Vy=0
                    time = time + (xLine - xTemp.X) / puckV.X;
                    temp.X = xLine;
                    temp.Y = xTemp.Y;
                    #endregion Vy=0
                }
                tempV.Y = puckV.Y;
                
                puckV.Y = -puckV.Y;
                xTemp.X = temp.X;
                xTemp.Y = temp.Y;
            }

            return new double[4] { xTemp.Y, time, tempV.X, tempV.Y };
        }

        public static Point EstimateStuck(Point puckP, Point puckV, double puckR, int tableW, int tableH, double time, double malletR)
        {
            // not stuck!!
            if (Math.Abs(puckV.X) > 0.05)
                return null;
            // not enough time for action!!
            if (time < 0.01)
                return null;

            Point attackPoint = new Point();

            double virtualY = puckP.Y + time * puckV.Y;
            double actualY = virtualY % tableW;
            double actualX = puckV.X * time + puckP.X;
            int tableFlops = (int)(virtualY / tableW);
            puckV.Y = Math.Pow(-1, tableFlops);
            attackPoint = CalculateActionPoint(new Point(actualX, actualY), puckV, puckR, malletR);

            return attackPoint;
        }


        /// <summary>
        /// calculate the actual point in which the mallet should move to achive a correct contact with the puck
        /// </summary>
        /// <param name="CrossingPuckP">The point where the puck cross the desired line</param>
        /// <param name="CrossingPuckV">The velocity with which the puck cross the desired line</param>
        /// <param name="puckR">Puck radius</param>
        /// <param name="malletR">Mallet radius</param>
        /// <returns>A point in space in which the mallet should move to</returns>
        public static Point CalculateActionPoint(Point CrossingPuckP, Point CrossingPuckV, double puckR, double malletR)
        {
            double a = puckR + malletR;
            Point actionPoint = new Point();
            if (CrossingPuckV.Y < 0)
            {
                double alpha = Math.Atan(Math.Abs(CrossingPuckV.X / CrossingPuckV.Y));
                actionPoint.X = CrossingPuckP.X - a * Math.Sin(alpha);
                actionPoint.Y = CrossingPuckP.Y - a * Math.Cos(alpha) / 3;// +alpha * 18 / Math.PI;
            }
            else if (CrossingPuckV.Y > 0)
            {
                double alpha = Math.Atan(Math.Abs(CrossingPuckV.X / CrossingPuckV.Y));
                actionPoint.X = CrossingPuckP.X - a * Math.Sin(alpha);
                actionPoint.Y = CrossingPuckP.Y + a * Math.Cos(alpha) / 3;// -alpha * 18 / Math.PI;
            }
            else
            {
                actionPoint.X = CrossingPuckP.X - a;
                actionPoint.Y = CrossingPuckP.Y;
            }

            return actionPoint;
        }

        public static TrajectoryQueue EstimatePuckTrajectory(Point puckP, Point puckV, double puckR, int tableW, int tableH, double timeStep, double timeScale)
        {
            TrajectoryQueue traj = new TrajectoryQueue(2, puckP.GetAsArray());
            double Tmax = 1;
            double t = 0;
            double Ts = timeStep;

            while (t < Tmax)
            {
                #region X
                puckP.X = puckP.X + Ts * puckV.X;
                if (puckP.X > tableW / 2.0)
                {
                    puckP.X = puckP.X - puckP.X % (tableW / 2.0);
                    puckV.X = -puckV.X;
                }
                else if (puckP.X < -tableW / 2.0)
                {
                    puckP.X = puckP.X + (Math.Abs(puckP.X) % (tableW / 2.0));
                    puckV.X = -puckV.X;
                }
                #endregion X

                #region Y
                puckP.Y = puckP.Y + Ts * puckV.Y;
                if (puckP.Y > tableH / 2.0)
                {
                    puckP.Y = puckP.Y - (puckP.Y % (tableH / 2.0));
                    puckV.Y = -puckV.Y;
                }
                else if (puckP.Y < -tableH / 2.0)
                {
                    puckP.Y = puckP.Y + (Math.Abs(puckP.Y) % (tableH / 2.0));
                    puckV.Y = -puckV.Y;
                }
                #endregion y

                traj.AddBack(QueueType.Position, puckP.GetAsArray());
                traj.AddBack(QueueType.Velocity, puckV.GetAsArray());

                t = t + Ts;
            }
            return traj;
        }
    }
}
