using System;
using System.Collections;
using UnityEngine;

// controller of the general gaming process
public class GameController : MonoBehaviour
{
    // player currently moving
    private PlayerMark movingPlayer = PlayerMark.Player1;

    private GameState gameState;
    public GameState GameState
    {
        get => gameState;
        set
        {
            gameState = value;
            // broadcast the message for other classes (UIController)
            Messenger<GameState>.Broadcast(GameEvents.GAMESTATE_CHANGED, gameState);
        }
    }

    // reference to itself, so that other classes have easy access to the controller
    public static GameController controller;
    [NonSerialized]
    // current game mode
    public GameMode mode;

    // whether the ai is enabled for this game
    private bool isAIenabled;
    // time AI has worked, will we subtracted from the game time in timed mode
    private float aiWorkSecs = 0;

    // time left for timed mode
    [NonSerialized]
    public float totalSecondsTime;
    [NonSerialized]
    public (int player1, int player2) score;
    

    [SerializeField]
    private Field field;

    [SerializeField]
    // line length we win for in difficulty mode and start to get points for in timed mode
    private int fullLineLength;

    // analyzers for both game modes
    private DifficultyGameAnalyzer difficultyGameAnalyzer;
    private TimedGameAnalyzer timedGameAnalyzer;

    private bool isGameOver = false;

    [SerializeField]
    private MinimaxAI ai;
    [SerializeField]
    // the player AI responds for
    private PlayerMark aiPlayer;

    [SerializeField]
    private MousePanner panner;

    private void Awake()
    {
        controller = this;
    }

    // checks whether the time for timed mode is over (every frame)
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

    // subtracts time from timed game time that ai has taken
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

    // sends messages about game finish
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

    // data for serialization (retrieved by DataManager)
    public (GameOptions options, Field field, TimedGameAnalyzer timedAnalyzer) GetGameData() => (new GameOptions(mode,
        isAIenabled, totalSecondsTime, score, movingPlayer, isGameOver), field, timedGameAnalyzer);

    // starts a new game based on the serializable data (called by DataManager on load or by replay popup with default initial parameters)
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

    // swaps the player to move and calls AI if it's its turn
    private void NextPlayer()
    {
        movingPlayer = movingPlayer == PlayerMark.Player1 ? PlayerMark.Player2 : PlayerMark.Player1;
        if (isAIenabled && movingPlayer == aiPlayer)
        {
            StartCoroutine(MoveWithAI());
        }
    }

    // moves in the specified cell
    private void Move(Vector2Int stableMatrixPos)
    {
        field.PutPlayer(stableMatrixPos, movingPlayer);
        AnalyzeField();
        if (!isGameOver)
        {
            NextPlayer();
        }
    }

    // called by FieldGrid to move with player
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

    // method that moves with AI (calculates the best move and calls Move based on it)
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

    // after each move, we have to analyze the field in case the game is over
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
            score = (score.player1 + player1Score, score.player2 + player2Score);
        }
    }
}
