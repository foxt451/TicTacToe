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
    [NonSerialized]
    public GameMode mode;
    private bool isAIenabled;
    private float aiWorkSecs = 0;

    [NonSerialized]
    public float totalSecondsTime;
    [NonSerialized]
    public (int player1, int player2) score;
    

    [SerializeField]
    private Field field;

    [SerializeField]
    private int fullLineLength;

    private DifficultyGameAnalyzer difficultyGameAnalyzer;
    private TimedGameAnalyzer timedGameAnalyzer;

    private bool isGameOver = false;

    [SerializeField]
    private MinimaxAI ai;
    [SerializeField]
    private PlayerMark aiPlayer;

    [SerializeField]
    private MousePanner panner;

    private void Awake()
    {
        controller = this;
    }

    private void Update()
    {
        if (GameState == GameState.INGAME && mode == GameMode.Timed)
        {
            if ((movingPlayer == PlayerMark.Player1 && score.player1 <= score.player2) ||
                (movingPlayer == PlayerMark.Player2 && score.player2 <= score.player1))
            {
                totalSecondsTime -= Time.deltaTime;
                if (totalSecondsTime <= 0)
                {
                    FinishGame();
                }
            }
        }
    }

    private void UpdateTimeAI()
    {
        if (mode == GameMode.Timed)
        {
            if ((aiPlayer == PlayerMark.Player1 && score.player1 <= score.player2) ||
                (aiPlayer == PlayerMark.Player2 && score.player2 <= score.player1))
            {
                aiWorkSecs = 0;
                totalSecondsTime -= aiWorkSecs;
            }
            if (totalSecondsTime <= 0)
            {
                FinishGame();
            }
        }
    }

    void FinishGame()
    {
        isGameOver = true;
        GameState = GameState.PAUSED;
        if (mode == GameMode.Difficulty)
        {
            Messenger<PlayerMark>.Broadcast(GameEvents.GAME_FINISHED, movingPlayer);
        }
        else
        {
            Messenger<PlayerMark>.Broadcast(GameEvents.GAME_FINISHED,
                score.player1 > score.player2 ? PlayerMark.Player1 : (score.player1 == score.player2 ? PlayerMark.Empty : PlayerMark.Player2));
        }
    }

    public (GameOptions options, Field field, TimedGameAnalyzer timedAnalyzer) GetGameData() => (new GameOptions(mode,
        isAIenabled, totalSecondsTime, score, movingPlayer, isGameOver), field, timedGameAnalyzer);

    public void StartNewGame((float x, float y, float z) cameraPos, GameOptions options, FieldOptions field = null, TimedGameAnalyzerInfo timedAnalyzerInfo = null)
    {
        panner.SetCurPos(cameraPos);

        mode = options.mode;
        isAIenabled = options.AI;

        score = options.initialScore;
        totalSecondsTime = options.timeLeft;

        movingPlayer = options.movingPlayer;

        if (field == null)
        {
            this.field.Reset();
        }
        else
        {
            this.field.CopyField(field);
        }

        difficultyGameAnalyzer = new DifficultyGameAnalyzer(this.field, fullLineLength);
        timedGameAnalyzer = new TimedGameAnalyzer(this.field, fullLineLength);

        if (timedAnalyzerInfo != null)
        {
            timedGameAnalyzer.Reconstruct(timedAnalyzerInfo);
        }

        GameState = GameState.INGAME;
        isGameOver = options.isGameOver;
        if (options.isGameOver)
        {
            FinishGame();
        }

        if (isAIenabled && movingPlayer == aiPlayer)
        {
            StartCoroutine(MoveWithAI(true));
        }
    }

    private void Start()
    {
        GameState = GameState.PREGAME;
    }

    private void NextPlayer()
    {
        movingPlayer = movingPlayer == PlayerMark.Player1 ? PlayerMark.Player2 : PlayerMark.Player1;
        if (isAIenabled && movingPlayer == aiPlayer)
        {
            StartCoroutine(MoveWithAI());
        }
    }

    private void Move(Vector2Int stableMatrixPos)
    {
        field.PutPlayer(stableMatrixPos, movingPlayer);
        AnalyzeField();
        if (!isGameOver)
        {
            NextPlayer();
        }
    }

    public void MoveWithPlayer(Vector2Int stableMatrixPos)
    {
        if (field.CellCompliesWithRules(stableMatrixPos))
        {
            if (!isAIenabled || movingPlayer != aiPlayer)
            {
                Move(stableMatrixPos);
            }
        }
    }

    // TODO
    // turn into coroutine, so that the game doesn't freeze
    private IEnumerator MoveWithAI(bool initialMove=false)
    {
        GameAnalyzer analyzerToUse;
        if (mode == GameMode.Difficulty)
        {
            analyzerToUse = difficultyGameAnalyzer;
        }
        else
        {
            analyzerToUse = timedGameAnalyzer;
        }
        Messenger.Broadcast(GameEvents.AI_START);
        yield return null;
        float bestPosStart = Time.realtimeSinceStartup;
        Vector2Int pos;
        if (initialMove)
        {
            pos = new Vector2Int(0, 0);
        }
        else
        {
            pos = ai.GetBestPosition(movingPlayer, mode, analyzerToUse);
        }
        float bestPosEnd = Time.realtimeSinceStartup;
        aiWorkSecs = bestPosEnd - bestPosStart;
        UpdateTimeAI();
        Messenger.Broadcast(GameEvents.AI_FINISH);
        Move(pos);
    }

    void AnalyzeField()
    {
        if (mode == GameMode.Difficulty)
        {
            if (difficultyGameAnalyzer.GetGameStatus() == DifficultyGameStatus.Defeated)
            {
                FinishGame();
            }
        }
        else
        {

            var (player1Score, player2Score) = timedGameAnalyzer.GetGameScore();
            Debug.Log(player1Score + " " + player2Score);
            score = (score.player1 + player1Score, score.player2 + player2Score);
        }
    }
}
