using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct GameOptions
{
    public GameMode mode;
    public bool AI;

    public GameOptions(GameMode mode, bool AI)
    {
        this.mode = mode;
        this.AI = AI;
    }
}

