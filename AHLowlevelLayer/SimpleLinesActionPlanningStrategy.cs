using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHLowlevelLayer
{
    public class SimpleLinesActionPlanningStrategy : IActionPlanningStrategy
    {
        double e = 0.9;
        double mu = 0.5;
        double rp = 32;
        double rm = 47.5;
        WorldModel worldModel;
        Logger.Logger mLogger;
        int[] global;
        Random random = new Random();
        Dictionary<string, double> physicalState;
        private PointParams lastPlan;
        private AHEntities.Action lastAction;

        public SimpleLinesActionPlanningStrategy(WorldModel WM)
        {
            worldModel = WM;
            mLogger = Logger.Logger.Instance;
            random = new Random();
            lastPlan = null;
            lastAction = AHEntities.Action.LEAVE;
        }

        public override PointParams ActionPlanning(AHEntities.ActionDirective action, bool isNewPlaning)
        {
            AHEntities.Action A = action.Action;
            PointParams finalConditions = null;
            Random random = new Random();
            Point puckP, puckV;

            physicalState = worldModel.GetPhysicalState();
            puckP = new Point(physicalState["PuckX"], physicalState["PuckY"]);
            puckV = new Point(physicalState["PuckVx"], physicalState["PuckVy"]);
            global = worldModel.GetSize();
            Point crossing;
            double[] crossParams;

            switch (A)
            {
                case AHEntities.Action.BLOCK:
                    #region BLOCK
                    crossParams = AHEntities.EstimateLineCrossing.Estimate(puckP, puckV, rp,
                                                AHEntities.EstimateLineCrossing.blockLine, global[0], global[1]);
                    crossing = new Point(AHEntities.EstimateLineCrossing.blockLine, crossParams[0]);
                    Point defendPoint = AHEntities.EstimateLineCrossing.CalculateActionPoint(crossing,
                                                    new Point(crossParams[2], crossParams[3]), rp, rm);
                    mLogger.AddLogMessage("LowLevel Planning BLOCK: estimated collission: mallet: " + defendPoint.ToString() + 
                                          ", puck: " + crossing.ToString());
                    finalConditions = new PointParams();
                    finalConditions.AddParameters(defendPoint);
                    finalConditions.AddParameters(new Point(0,0));
                    finalConditions.T = DateTime.Now + TimeSpan.FromSeconds(crossParams[1]);
                    break;
                    #endregion BLOCK
                case AHEntities.Action.LEAVE:
                    #region LEAVE
                    finalConditions = null;
                    break;
                    #endregion LEAVE
                case AHEntities.Action.ATTACK_RIGHT:
                    #region ATTACK_RIGHT
                    //finalConditions = CalculateDirectedAttack(AHEntities.Action.ATTACK_RIGHT, puckP, puckV, physicalState["PuckR"],
                    //                  AHEntities.EstimateLineCrossing.attackLine, global[0], global[1]);
                    //break;
                    #endregion ATTACK_RIGHT
                case AHEntities.Action.ATTACK_LEFT:
                    #region ATTACK_LEFT
                    //finalConditions = CalculateDirectedAttack(AHEntities.Action.ATTACK_LEFT, puckP, puckV, physicalState["PuckR"],
                    //                  AHEntities.EstimateLineCrossing.attackLine, global[0], global[1]);
                    //break;
                    #endregion ATTACK_LEFT
                case AHEntities.Action.ATTACK_MIDDLE:
                    #region ATTACK_MIDDLE
                    crossParams = AHEntities.EstimateLineCrossing.Estimate(puckP, puckV, rp,
                                           AHEntities.EstimateLineCrossing.attackLine, global[0], global[1]);
                    mLogger.AddLogMessage("LowLevel: Puck Estimated at: (" + AHEntities.EstimateLineCrossing.attackLine.ToString() + "," + crossParams[0].ToString() + ") in " + crossParams[1].ToString() + " seconds");
                    Point puckPline = new Point(AHEntities.EstimateLineCrossing.attackLine, crossParams[0]);
                    double velocity = 1000;
                    finalConditions = CalculateNaiveDirectedAttack(A, puckPline, crossParams[1],
                                      AHEntities.EstimateLineCrossing.attackLine, rp, rm, global[0], global[1], velocity);
                    break;
                #endregion ATTACK_MIDDLE
                case AHEntities.Action.DEFENSE_ATTACK:
                    #region DEFENSE_ATTACK
                    if (Math.Abs(puckV.X) < 0.01)
                    {
                        break;
                    }
                    crossParams = AHEntities.EstimateLineCrossing.Estimate(puckP, puckV, rp,
                                           AHEntities.EstimateLineCrossing.defenseAttackLine, global[0], global[1]);
                    if (crossParams == null)
                    {
                        finalConditions = null;
                        break;
                    }
                    crossing = new Point(AHEntities.EstimateLineCrossing.defenseAttackLine, crossParams[0]);
                    Point attackPoint = AHEntities.EstimateLineCrossing.CalculateActionPoint(new Point(AHEntities.EstimateLineCrossing.defenseAttackLine, crossParams[0]),
                                                    new Point(crossParams[2], crossParams[3]), rp, rm);
                    mLogger.AddLogMessage("LowLevel Planning DEFENSE_ATTACK: estimated collission: mallet: " + attackPoint.ToString() +
                                          ", puck estimated at: " + crossing.ToString());

                    finalConditions = new PointParams();
                    finalConditions.AddParameters(attackPoint);
                    double yvel = (attackPoint.Y > 0) ? (yvel = -200) : (yvel = 200);
                    finalConditions.AddParameters(new Point(500, yvel));
                    finalConditions.T = DateTime.Now + TimeSpan.FromSeconds(crossParams[1]);
                    break;
                    #endregion DEFENSE_ATTACK 
                case AHEntities.Action.PREPARE:
                    #region PREPARE
                    finalConditions = new PointParams();
                    finalConditions.AddParameters(new Point(-1000, 0 ));
                    finalConditions.AddParameters(new Point(0,0));
                    finalConditions.T = action.TimeStamp + TimeSpan.FromSeconds(0.5);
                    break;
                    #endregion PREPARE
                case AHEntities.Action.STUCK_ATTACK:
                #region STUCK_ATTACK
                    double stuckTime;
                    stuckTime = (action.Duration - (DateTime.Now - action.TimeStamp)).TotalSeconds;
                    Point stuckAttackPoint = AHEntities.EstimateLineCrossing.EstimateStuck(puckP, puckV, rp, global[0], global[1], stuckTime, rm);
                    if (stuckAttackPoint == null)
                    {
                        finalConditions = null;
                        break;
                    }
                    mLogger.AddLogMessage("LowLevel Planning STUCK_ATTACK: estimated collission: mallet: " + stuckAttackPoint.ToString());
                    finalConditions = new PointParams();
                    finalConditions.AddParameters(stuckAttackPoint);
                    finalConditions.AddParameters(new Point(0, 0));
                    finalConditions.T = action.TimeStamp + TimeSpan.FromSeconds(stuckTime);
                    break;
                #endregion STUCK_ATTACK
                default:
                    #region default
                    finalConditions = null;
                    break;
                    #endregion default
            }

            //saving current planning (action and parameters)
            lastAction = A;
            if (finalConditions != null)
            {
                if (lastPlan == null)
                    lastPlan = new PointParams();
                lastPlan.Clear();
                lastPlan.AddParameters(finalConditions.GetParamsByIndex(1));
                lastPlan.AddParameters(finalConditions.GetParamsByIndex(2));
                lastPlan.T = finalConditions.T;
            }
            else
                lastPlan = null;

            return finalConditions;
        }
        
        private PointParams CalculateNaiveDirectedAttack(AHEntities.Action direction, Point puckPline, double time,
                                                double xline, double puckRadius, double malletRadius, int tableW, int tableH, double velocity)
        {
            PointParams finalConditions = new PointParams();
            Point collisionPoint, targetPoint, targetVelocity;

            collisionPoint = GetNaiveCollisionPoint(direction, puckPline, puckRadius-2, tableW, tableH);
            targetPoint = GetNaiveTargetPoint(puckPline, collisionPoint, malletRadius-2);
            targetVelocity = GetNaiveTargetVelocity(collisionPoint, targetPoint, velocity);

            finalConditions.AddParameters(targetPoint);
            finalConditions.AddParameters(targetVelocity);
            finalConditions.T = DateTime.Now + TimeSpan.FromSeconds(time);

            return finalConditions;
        }

        private Point GetNaiveCollisionPoint(AHEntities.Action direction, Point puckPline, double puckRadius, int tableW, int tableH)
        {
            Point attackVector = new Point();
            Point collisionPoint = null;
            Point wallCollision = null;
            double ratio = 0;
            switch (direction)
            {
                case AHEntities.Action.ATTACK_RIGHT:
                    ratio = Math.Abs(puckPline.Y - (-tableH / 2)) / (tableH / 2);
                    wallCollision = new Point(Math.Abs(tableW / 2 - puckPline.X) / (1 + ratio) + puckPline.X, -tableH/2);
                    attackVector = (wallCollision - puckPline).Normalize();
                    attackVector.Set(attackVector.X * puckRadius, attackVector.Y * puckRadius);
                    collisionPoint = puckPline - attackVector;
                    break;
                case AHEntities.Action.ATTACK_MIDDLE:
                    attackVector.X = tableW / 2 - puckPline.X;
                    attackVector.Y = -puckPline.Y;
                    attackVector = attackVector.Normalize();
                    attackVector.Set(attackVector.X * puckRadius, attackVector.Y * puckRadius);
                    collisionPoint = puckPline - attackVector;
                    break;
                case AHEntities.Action.ATTACK_LEFT:
                    ratio = Math.Abs(puckPline.Y - tableH / 2) / (tableH / 2);
                    wallCollision = new Point(Math.Abs(tableW / 2 - puckPline.X) / (1 + ratio) + puckPline.X, tableH / 2);
                    attackVector = (wallCollision - puckPline).Normalize();
                    attackVector.Set(attackVector.X * puckRadius, attackVector.Y * puckRadius);
                    collisionPoint = puckPline - attackVector;
                    break;
                default:
                    collisionPoint = null; 
                    break;
            }

            return collisionPoint;
        }

        private Point GetNaiveTargetPoint(Point puckPline, Point collisionPoint, double malletRadius)
        {
            Point targetPoint = null;
            Point attackVector = (puckPline - collisionPoint).Normalize();
            attackVector.Set(attackVector.X * rm, attackVector.Y * rm);
            targetPoint = collisionPoint - attackVector;
            return targetPoint;
        }

        private Point GetNaiveTargetVelocity(Point collisionPoint, Point targetPoint, double velocity)
        {
            Point velocityVector = (collisionPoint - targetPoint).Normalize();
            velocityVector.Set(velocityVector.X * velocity, velocityVector.Y * velocity);
            return velocityVector;
        }



        private PointParams CalculateDirectedAttack(AHEntities.Action direction, Point puckP, Point puckV, double puckR,
                                                double xline, int tableW, int tableH)
        {
            PointParams finalConditions = new PointParams();

            double[] crossParamsAttack = AHEntities.EstimateLineCrossing.Estimate(puckP, puckV, puckR, xline, tableW, tableH);

            Point XYi = GetCollissionPoint(puckV.X, puckV.Y, xline, crossParamsAttack[0], direction);
            // Detemine speeds at the goal.
            Point Pg = new Point(1180, 0);
            Point speedG = GetGoalSpeed(direction, xline, crossParamsAttack[0], Pg);
            double thetaG = -speedG.X / rp; // constrain on the angle speed at the goal
            double vG = speedG.Norm();

            double vPS;
            Point speedPS;
            double thetaPS, thetaPSdot;
            if (direction != AHEntities.Action.ATTACK_MIDDLE)
            {
                // calculating angles
                double alpha = CalculateAlpha(speedG, AHEntities.Action.ATTACK_RIGHT);
                double beta = CalculateBeta(direction, xline, crossParamsAttack[0], tableH, Pg, alpha);
                // calculating post strike prameters
                vPS = -vG / e * Math.Sin(alpha) / Math.Sin(beta);
                speedPS = new Point(vPS * Math.Cos(beta), vPS * Math.Sin(beta));
                thetaPS = Math.Atan2(speedPS.Y, speedPS.X);
                thetaPSdot = 2 * vG / (rp * e) * Math.Sin(alpha) / Math.Tan(beta) - 3 * vG / rp * Math.Cos(alpha);
            }
            else // direction == MIDDLE
            {
                // calculating post strike prameters
                vPS = 0;
                speedPS = new Point();
                thetaPS = 0;
                thetaPSdot = 0;
            }

            // calculating frames rotation angle
            double thetaP = Math.Atan2(puckV.Y, puckV.X);
            double thetaC = CalculateRotationAngle(thetaPS, thetaPSdot, thetaP, puckR, vPS, puckV.Norm());
            // transform puck and ps velocities to impact frame
            Point impactVps = TransformFrames(thetaC, speedPS, true);
            Point impactVp = TransformFrames(thetaC, puckV, true);
            // calculate control velocities
            Point impactVm = CalculateImpactVelocities(impactVp, impactVps, thetaP, thetaPS, rp);
            // transform to XY
            Point vM = TransformFrames(thetaC, impactVm, false);

            // create finalConditions.
            Point target = CalculateTargetPoint(XYi, new Point(AHEntities.EstimateLineCrossing.attackLine, crossParamsAttack[0]));
            finalConditions.AddParameters(target);
            finalConditions.AddParameters(vM);
            finalConditions.T = DateTime.Now + TimeSpan.FromSeconds(crossParamsAttack[1]);

            return finalConditions;
        }

        #region directed attack helper functions
        private Point GetCollissionPoint(double PuckVx, double PuckVy, double xLine, double y, AHEntities.Action attackDirection)
        {
            Point XYi = new Point();
            Random random = new Random();
            int a, b;
            

            switch (attackDirection)
            {
                case AHEntities.Action.ATTACK_LEFT:
                    if (PuckVy > 0)
                    {
                        a = random.Next(5);
                        b = (int)Math.Round(Math.Sqrt(rp * rp - a * a));
                        XYi.Y = y + a;
                        XYi.X = xLine - b;
                    }
                    else
                    {
                        a = random.Next(10);
                        b = (int)Math.Round(Math.Sqrt(rp * rp - a * a));
                        XYi.Y = y - a;
                        XYi.X = xLine - b;
                    }
                    break;
                case AHEntities.Action.ATTACK_MIDDLE:
                    if (PuckVy > 0)
                    {
                        a = random.Next(3);
                        b = (int)Math.Round(Math.Sqrt(rp * rp - a * a));
                        XYi.Y = y + a;
                        XYi.X = xLine - b;
                    }
                    else
                    {
                        a = random.Next(3);
                        b = (int)Math.Round(Math.Sqrt(rp * rp - a * a));
                        XYi.Y = y - a;
                        XYi.X = xLine - b;
                    }
                    break;
                case AHEntities.Action.ATTACK_RIGHT:
                    if (PuckVy > 0)
                    {
                        a = random.Next(10);
                        b = (int)Math.Round(Math.Sqrt(rp * rp - a * a));
                        XYi.Y = y + a;
                        XYi.X = xLine - b;
                    }
                    else
                    {
                        a = random.Next(5);
                        b = (int)Math.Round(Math.Sqrt(rp * rp - a * a));
                        XYi.Y = y - a;
                        XYi.X = xLine - b;
                    }
                    break;
            }
            return XYi;
        }

        private Point GetGoalSpeed(AHEntities.Action direction, double x, double y, Point Pg)
        {
            Point Vg;
            Random random = new Random();
            switch (direction)
            {
                case AHEntities.Action.ATTACK_LEFT:
                    Vg = new Point(random.Next(400,600), -random.Next(200,300));
                    break;
                case AHEntities.Action.ATTACK_RIGHT:
                    Vg = new Point(random.Next(400,600), random.Next(200,300));
                    break;
                case AHEntities.Action.ATTACK_MIDDLE:
                    int Vx = random.Next(500, 700);
                    double t = (1180.0 - x) / Vx;
                    int Vy = -Convert.ToInt32(t*y);
                    Vg = new Point(Vx, Vy);
                    break;
                default:
                    Vg = new Point();
                    break;
            }
            return Vg;
        }

        private double CalculateAlpha(Point SpeedGoal, AHEntities.Action direction)
        {
            if (direction == AHEntities.Action.ATTACK_LEFT)
            {
                return Math.Atan2(-SpeedGoal.Y, SpeedGoal.X);
            }
            else
            {
                return Math.Atan2(SpeedGoal.Y, SpeedGoal.X);
            }
        }

        private double CalculateBeta(AHEntities.Action direction, double x, double y, double tableY, Point Pg, double alpha)
        {
            double yp, a1;
            if (direction == AHEntities.Action.ATTACK_LEFT)
            {
                a1 = tableY / 2 - Pg.Y;
                yp = tableY / 2 - y;
            }
            else // ATTACK_RIGHT
            {
                a1 = Pg.Y - tableY/2;
                yp = tableY/2 - y;
            }
            double b1 = a1 / Math.Tan(alpha);
            double b2 = (Pg.X - x) - b1;
            double beta = Math.Atan2(yp, b2);
            return beta;
        }

        private double CalculateRotationAngle(double thetaPS, double thetaPSdot, double thetaP, double thetaPdot, double vPS, double Vp)
        {
            double thetaC;
            double X = vPS * Math.Cos(thetaPS) - Vp * Math.Cos(thetaP);
            double Y = vPS * Math.Sin(thetaPS) - Vp * Math.Sin(thetaP);
            double Z = rp / 2 * (thetaPSdot - thetaPdot);
            double normalizationFactor = Z / Math.Sqrt(X * X + Y * Y);
            double alpha = Math.Atan2(Y / normalizationFactor, X / normalizationFactor);
            if (Math.Abs(Z / normalizationFactor) <= 1)
                thetaC = alpha + Math.Acos(Z / normalizationFactor);
            else
                thetaC = Double.NaN;
            return thetaC;
        }

        private Point TransformFrames(double thetaC, Point initFrameV, bool toBN)
        {
            Point targetFrameV;
            try
            {
                if (toBN)
                {
                    Matrix At = Matrix.Parse(Math.Cos(thetaC).ToString() + " " + Math.Sin(thetaC).ToString() + "\r\n" +
                                             "-" + Math.Sin(thetaC).ToString() + " " + Math.Cos(thetaC).ToString());
                    Matrix XY = Matrix.Parse(initFrameV.X.ToString() + "\r\n" + initFrameV.Y.ToString());
                    Matrix BN = At * XY;
                    targetFrameV = new Point(BN[0, 0], BN[1, 0]);

                }
                else
                {
                    Matrix A = Matrix.Parse(Math.Cos(thetaC).ToString() + " -" + Math.Sin(thetaC).ToString() + "\r\n" +
                                             Math.Sin(thetaC).ToString() + " " + Math.Cos(thetaC).ToString());
                    Matrix BN = Matrix.Parse(initFrameV.X.ToString() + "\r\n" + initFrameV.Y.ToString());
                    Matrix XY = A * BN;
                    targetFrameV = new Point(XY[0, 0], XY[1, 0]);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return targetFrameV;
        }

        private Point CalculateImpactVelocities(Point Vp, Point Vps, double thetaP, double thetaPS, double puckRadius)
        {
            Point velocities = new Point();
            double a = Math.Abs( Vps.X + thetaPS * puckRadius );
            double b = Math.Abs(3 * mu * (1 + e) * Vps.Y);

            if (a < b)  // no sliding
            {
                velocities.X = 3 * Vps.X - 2 * Vp.X + puckRadius * thetaP;
                velocities.Y = 1 / (1 + e) * Vps.Y + e / (1 + e) * Vp.Y;
            }
            else        // sliding
            {
                velocities.Y = 1 / (1 + e) * Vps.Y + e / (1 + e) * Vp.Y;
                double s = (Vps.X - Vp.X) / (mu * (1 + e) * (Vp.Y - velocities.Y));
                if (s > 0)
                {
                    velocities.X = Vp.X + thetaP * puckRadius - 0.1;
                }
                else if (s < 0)
                {
                    velocities.X = Vp.X + thetaP * puckRadius + 0.1;
                }
                else
                {
                    velocities.X = Vp.X + thetaP * puckRadius;
                }
            }
            return velocities;
        }

        private Point CalculateTargetPoint(Point collision, Point puck)
        {
            Point mallet = new Point();
            double alpha;
            if (puck.Y < collision.Y)
            {
                alpha = Math.Atan2(puck.X - collision.X, collision.Y - puck.Y);
                mallet.Y = collision.Y + rm * Math.Cos(alpha);
                mallet.X = collision.X - rm * Math.Sin(alpha);
            }
            else if (puck.Y > collision.Y)
            {
                alpha = Math.Atan2(puck.X - collision.X, puck.Y - collision.Y);
                mallet.Y = collision.Y - rm * Math.Cos(alpha);
                mallet.X = collision.X - rm * Math.Sin(alpha);
            }
            else
            {
                mallet.X = collision.X - rm;
                mallet.Y = collision.Y;
            }
            return mallet;
        }

        #endregion directed attack helper functions
    }
}
