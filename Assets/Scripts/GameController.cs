using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private PlayerMark movingPlayer = PlayerMark.Player1;

    public static GameState gameState;

    [SerializeField]
    private Field field;

    [SerializeField]
    private int fullLineLength;

    private DifficultyGameAnalyzer difficultyGameAnalyzer;

    private void Awake()
    {
        difficultyGameAnalyzer = new DifficultyGameAnalyzer(field, fullLineLength);

        Messenger.AddListener(GameEvents.NEW_GAME, StartNewGame);
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener(GameEvents.NEW_GAME, StartNewGame);
    }

    private void StartNewGame()
    {
        gameState = GameState.INGAME;
        field.Clear();
    }

    private void Start()
    {
        gameState = GameState.PREGAME;
    }

    public void Move(Vector3Int matrixPos)
    {
        field.PutPlayer(matrixPos, movingPlayer);
        movingPlayer = movingPlayer == PlayerMark.Player1 ? PlayerMark.Player2 : PlayerMark.Player1;

        // analyze field
        if (difficultyGameAnalyzer.GetGameStatus() == DifficultyGameStatus.Defeated)
        {
            Debug.Log("The game has ended!");
        }
    }
}
