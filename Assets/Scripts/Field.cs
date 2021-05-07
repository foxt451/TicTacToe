using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Field : MonoBehaviour
{
    // when we move in a cell closer than 5 cells from the boundaries, we expand the matrix
    [SerializeField]
    private int expandingDistance = 5;

    [SerializeField]
    private int height;
    public int Height { get => height; private set => height = value; }

    [SerializeField]
    private int width;
    public int Width { get => width; private set => width = value; }

    public int[,] Matrix { get; private set; } // 2d field with moves, 1 - one player, 2 - another, 0 - empty cell
    // (0, 0) - lower left
    // (N, N) - upper right

    private int movingPlayer = 1;

    private void Awake()
    {
        
    }

    

    private void Start()
    {
        // initial field matrix
        Matrix = new int[Height, Width];
        Messenger.Broadcast(GameEvents.FIELD_UPDATED);
    }

    private bool FallsWithinGrid(Vector3Int matrixPos)
    {
        return matrixPos.x < Width && matrixPos.x >= 0 && matrixPos.y < Height & matrixPos.y >= 0;
    }

    private bool MoveCompliesWithRules(Vector3Int matrixPos)
    {
        if (Matrix[matrixPos.y, matrixPos.x] != 0)
        {
            return false;
        }
        return true;
    }

    public void MakeMove(Vector3Int matrixPos)
    {
        // check if the pos falls within the current field
        if (!FallsWithinGrid(matrixPos))
        {
            return;
        }

        if (!MoveCompliesWithRules(matrixPos))
        {
            return;
        }

        Matrix[matrixPos.y, matrixPos.x] = movingPlayer;
        movingPlayer = movingPlayer == 1 ? 2 : 1;

        // check if we need to resize

        Messenger.Broadcast(GameEvents.FIELD_UPDATED);
    }
}
