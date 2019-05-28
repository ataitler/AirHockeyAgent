using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public enum SenseEventType
    {
        // puck hit one of the back walls - changing direction
        xWall,
        // puck hit one of the side walls -  continue in same direction
        yWall,
        // opponent hit the puck
        OpponentCollision,
        // agent hit the puck
        AgentCollision,
        // puck is stuck on the table - either not moving, or Vx=0 on the agent's side
        StuckAgent,
        // puck is stuck on the table - either not moving, or Vx=0 on the player's side
        StuckPlayer,
        // Unknown cause to the puck change of motion
        Disturbance,
        // nothing happend
        NoEvent
    }
}
