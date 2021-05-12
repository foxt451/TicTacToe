using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public struct GameOptions
{
    public GameMode mode;
    public bool AI;

    public float timeLeft;
    public (int player1, int player2) initialScore;

    public GameOptions(GameMode mode, bool AI, float timeLeft, (int player1, int player2) initialScore)
    {
        this.mode = mode;
        this.AI = AI;
        this.timeLeft = timeLeft;
        this.initialScore = initialScore;
    }
}

