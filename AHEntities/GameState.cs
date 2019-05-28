using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public enum GameState
    {
        // Disconnected
        Disconnected,
        // Idle - connected to server but no game in motion
        Idle,
        // Game Idle - during a game but not playing currently
        GameIdle,
        // Game Playing
        Playing
    }
}
