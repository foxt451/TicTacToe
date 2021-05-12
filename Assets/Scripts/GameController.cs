using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private PlayerMark movingPlayer = PlayerMark.Player1;

    public GameState gameState;

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
        if (gameState == GameState.INGAME && mode == GameMode.Timed)
        {
            totalSeconsTime -= Time.deltaTime;
        }
    }


    private const string fieldDataName = "field";
    private const string optionsDataName = "options";
    private const string timeDataName = "time";
    private const string scoreDataName = "score";

    public (GameOptions options, Field field) GetGameData() => (new GameOptions(mode, AI, totalSeconsTime, score), field);

    public void StartNewGame(GameOptions options, FieldOptions field = null)
    {
        gameState = GameState.INGAME;

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
            var deltaScore = timedGameAnalyzer.GetGameScore();
            score = (score.player1 + deltaScore.player1Score, score.player2 + deltaScore.player2Score);
        }
    }
}
