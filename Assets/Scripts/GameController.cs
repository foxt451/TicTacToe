using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private PlayerMark movingPlayer = PlayerMark.Player1;

    public static GameState gameState;
    private GameMode mode;
    private bool AI;
    

    [SerializeField]
    private Field field;

    [SerializeField]
    private int fullLineLength;

    private DifficultyGameAnalyzer difficultyGameAnalyzer;
    private TimedGameAnalyzer timedGameAnalyzer;

    private void Awake()
    {
        difficultyGameAnalyzer = new DifficultyGameAnalyzer(field, fullLineLength);
        timedGameAnalyzer = new TimedGameAnalyzer(field, fullLineLength);

        Messenger<GameOptions>.AddListener(GameEvents.NEW_GAME, StartNewGame);
    }

    private void OnDestroy()
    {
        Messenger<GameOptions>.RemoveListener(GameEvents.NEW_GAME, StartNewGame);
    }

    private void StartNewGame(GameOptions options)
    {
        gameState = GameState.INGAME;

        mode = options.mode;
        AI = options.AI;

        field.Clear();
    }

    private void Start()
    {
        gameState = GameState.PREGAME;
    }

    public void Move(Vector2Int stableMatrixPos)
    {
        field.PutPlayer(stableMatrixPos, movingPlayer);
        movingPlayer = movingPlayer == PlayerMark.Player1 ? PlayerMark.Player2 : PlayerMark.Player1;

        // analyze field
        if (mode == GameMode.Difficulty)
        {
            if (difficultyGameAnalyzer.GetGameStatus() == DifficultyGameStatus.Defeated)
            {
                Debug.Log("The game has ended!");
            }
        } 
        else
        {
            Debug.Log("Score: " + timedGameAnalyzer.GetGameScore());
        }
    }
}
