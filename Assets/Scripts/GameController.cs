using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private PlayerMark movingPlayer = PlayerMark.Player1;

    [SerializeField]
    private Field field;

    [SerializeField]
    private int fullLineLength;

    private DifficultyGameAnalyzer difficultyGameAnalyzer;

    private void Awake()
    {
        difficultyGameAnalyzer = new DifficultyGameAnalyzer(field, fullLineLength);
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
