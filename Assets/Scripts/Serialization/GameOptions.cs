using System;
using System.Collections.Generic;
using System.Linq;
// serialization info for GameController
[Serializable]
public class GameOptions
{
    public GameMode mode;
    public bool AI;

    public float timeLeft;
    public (int player1, int player2) initialScore;

    public PlayerMark movingPlayer;
    public bool isGameOver;

    public GameOptions(GameMode mode, bool AI, float timeLeft, (int player1, int player2) initialScore, 
        PlayerMark movingPlayer, bool isGameOver)
    {
        this.mode = mode;
        this.AI = AI;
        this.timeLeft = timeLeft;
        this.initialScore = initialScore;
        this.movingPlayer = movingPlayer;
        this.isGameOver = isGameOver;
    }
}

