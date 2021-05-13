using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private PlayerMark movingPlayer = PlayerMark.Player1;

    private GameState gameState;
    public GameState GameState
    {
        get => gameState;
        set
        {
            gameState = value;
            Messenger<GameState>.Broadcast(GameEvents.GAMESTATE_CHANGED, gameState);
        }
    }

    public static GameController controller;
    private GameMode mode;
    private bool AI;


    private float totalSeconsTime;
    private (int player1, int player2) score;
    

    [SerializeField]
    private Field field;

    [SerializeField]
    private int fullLineLength;

    private DifficultyGameAnalyzer difficultyGameAnalyzer;
    private TimedGameAnalyzer timedGameAnalyzer;

    private void Awake()
    {
        controller = this;

        difficultyGameAnalyzer = new DifficultyGameAnalyzer(field, fullLineLength);
        timedGameAnalyzer = new TimedGameAnalyzer(field, fullLineLength);
    }

    private void Update()
    {
        if (GameState == GameState.INGAME && mode == GameMode.Timed)
        {
            totalSeconsTime -= Time.deltaTime;
        }
    }

    public (GameOptions options, Field field) GetGameData() => (new GameOptions(mode, AI, totalSeconsTime, score), field);

    public void StartNewGame(GameOptions options, FieldOptions field = null)
    {
        GameState = GameState.INGAME;

        mode = options.mode;
        AI = options.AI;

        score = options.initialScore;
        totalSeconsTime = options.timeLeft;

        if (field == null)
        {
            this.field.Clear();
        }
        else
        {
            this.field.CopyField(field);
        }
    }

    private void Start()
    {
        GameState = GameState.PREGAME;
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
            var (player1Score, player2Score) = timedGameAnalyzer.GetGameScore();
            score = (score.player1 + player1Score, score.player2 + player2Score);
        }
    }
}
