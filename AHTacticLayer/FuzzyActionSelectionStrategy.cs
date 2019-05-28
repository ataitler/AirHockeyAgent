using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHTacticLayer
{
    public class FuzzyActionSelectionStrategy : IActionSelectionStrategy
    {
        WorldModel WM;
        Hashtable global;

        public FuzzyActionSelectionStrategy(WorldModel worldModel)
        {
            WM = worldModel;
            global = worldModel.GetConstants();
        }

        public override AHEntities.ActionDirective SelectAction(SenseEventType planReason)
        {
            Dictionary<string, double> physicalState = WM.GetPhysicalState();
            double[] crossParamsGoal, crossParamsAttack, crossParamsDefense;
            bool isThreat = false;
            double time2goal = 0;
            double time2attack = 0;
            double time2defense = 0;
            AHEntities.ActionDirective action = new ActionDirective();
            action.TimeStamp = DateTime.Now;

            #region stuck events handling
            if (planReason == SenseEventType.StuckPlayer)
            {
                action.Action = AHEntities.Action.PREPARE;
                action.Duration = TimeSpan.FromSeconds(1);
                return action;
            }
            if (planReason == SenseEventType.StuckAgent)
            {
                action.Action = AHEntities.Action.STUCK_ATTACK;
                action.Duration = TimeSpan.FromSeconds(0.3);
                return action;
            }

            #endregion stuck events handling

            #region Crossing calculations
            Point puckP = new Point(physicalState["PuckX"], physicalState["PuckY"]);
            Point puckV = new Point(physicalState["PuckVx"], physicalState["PuckVy"]);
            try
            {
                // (y, T)
                crossParamsGoal = AHEntities.EstimateLineCrossing.Estimate(puckP, puckV, 32,
                                  AHEntities.EstimateLineCrossing.goalLine, (int)global["Tablewidth"], (int)global["Tableheight"]);

                crossParamsDefense = AHEntities.EstimateLineCrossing.Estimate(puckP, puckV, 32,
                                      AHEntities.EstimateLineCrossing.defenseAttackLine, (int)global["Tablewidth"], (int)global["Tableheight"]);

                crossParamsAttack = AHEntities.EstimateLineCrossing.Estimate(puckP, puckV, 32,
                                    AHEntities.EstimateLineCrossing.attackLine, (int)global["Tablewidth"], (int)global["Tableheight"]);
            }
            catch (Exception)
            {
                action.Action = AHEntities.Action.LEAVE;
                action.Duration = TimeSpan.FromSeconds(0);
                return action;
            }
            #endregion Crossing calculations

            // not directed to the goal
            if (crossParamsGoal == null)
            {
                action.Action = AHEntities.Action.PREPARE;
                action.Duration = TimeSpan.FromSeconds(1);
                return action;
            }
            
            // is directed to the goal
            if ((crossParamsGoal[0] < 220) && (crossParamsGoal[0] > -220))
                isThreat = true;
            
            // time to reach the goal and attack lines
            time2goal = crossParamsGoal[1];
            time2defense = crossParamsDefense[1];
            time2attack = crossParamsAttack[1];

            #region emergency Block/Leave
            // puck is too fast, immidiate BLOCK or LEAVE
            if (time2goal < 0.2)
            {
                if (isThreat)
                {
                    action.Action = AHEntities.Action.BLOCK;
                    action.Duration = TimeSpan.FromSeconds(time2goal);
                    return action;
                }
                else
                {
                    action.Action = AHEntities.Action.LEAVE;
                    action.Duration = TimeSpan.FromSeconds(0);
                    return action;
                }
            }
            #endregion emergency Block/Leave

            if (time2attack < 0.5)      // defense attack (no time to calcualte elaborated attack
            {
                action.Action = AHEntities.Action.DEFENSE_ATTACK;
                action.Duration = TimeSpan.FromSeconds(time2defense);
                return action;
            }
            else // Directed attack
            {
                int height = (int)global["Tableheight"]/2;
                double rim = height * 0.95;
                action.Duration = TimeSpan.FromSeconds(time2attack);
                
                // puck is crossing on the rim of the table
                if (Math.Abs(crossParamsAttack[0]) > rim)
                {
                    action.Action = AHEntities.Action.DEFENSE_ATTACK;
                    action.Duration = TimeSpan.FromSeconds(time2defense);
                    return action;
                }
                else // crossing on the inside of the table
                {
                    Random random = new Random();
                    AHEntities.Action[] attacks;
                    double outerTable = height * 0.65;
                    if (Math.Abs(crossParamsAttack[0]) > outerTable) // outer part
                    {
                        if (crossParamsAttack[2] > height - outerTable) // Agent's left part
                        {
                            attacks = new AHEntities.Action[] { AHEntities.Action.ATTACK_RIGHT,
                                                                AHEntities.Action.ATTACK_MIDDLE };
                        }
                        else // Agent's right part
                        {
                            attacks = new AHEntities.Action[] { AHEntities.Action.ATTACK_LEFT,
                                                                AHEntities.Action.ATTACK_MIDDLE };
                        }
                    }
                    else // inner part
                    {
                        attacks = new AHEntities.Action[] { AHEntities.Action.ATTACK_LEFT,
                                                            AHEntities.Action.ATTACK_MIDDLE,
                                                            AHEntities.Action.ATTACK_RIGHT };
                    }
                    action.Action = attacks[random.Next(attacks.Length)];
                    return action;
                }
            }
        }

        private double[] EstimateLinesParameters(double puckX, double PuckY, double PuckVx, double PuckVy,
                                                 double xLine, int tableW, int tableH)
        {
            // The puck is moving away from the agent
            if (PuckVx >= 0)
                return null;

            double puckRadius = 32.5;
            Point xTemp = new Point(puckX, PuckY);
            Point puckV = new Point(PuckVx, PuckVy);
            double EffectiveTableY = tableH / 2 - puckRadius;
            double time = 0.0;
            double tempTime;
            Point temp;

            while (xTemp.X > xLine)
            {
                temp = new Point();
                if (puckV.Y > 0)
                {
                    #region Y>0
                    tempTime = (EffectiveTableY - xTemp.Y) / puckV.Y;
                    if (xTemp.X + tempTime * puckV.X > xLine)
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
                    #endregion Y>0
                }
                else if (puckV.Y < 0)
                {
                    #region Y<0
                    tempTime = (-EffectiveTableY - xTemp.Y) / puckV.Y;
                    if (xTemp.X + tempTime * puckV.X > xLine)
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
                    #endregion Y<0
                }
                else
                {
                    time = (xLine - xTemp.X) / puckV.X;
                    temp.X = xLine;
                    temp.Y = xTemp.Y;
                }
                puckV.Y = -puckV.Y;
                xTemp = temp;
            }

            double[] crossing = { xTemp.Y, time };
            return crossing;
        }
    }
}
