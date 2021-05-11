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

    // it's more convenient to work indirectly through variables rather than returning List dimensions
    // because we may want to modify the list, at the same time relying on the old width and height
    // and only then update sizes
    public int Height { get => height; private set => height = value; }

    [SerializeField]
    private int width;
    public int Width { get => width; private set => width = value; }

    // when we draw grid it's centered by default
    // but we want it to remain fixed in position even after resizing
    // one way to achieve this is to remember by how much and in what direction we have resized the grid
    // the grid drawer will take these offsets into consideration
    public (int xRight, int xLeft,
        int yTop, int yBot) totalIncrease = (0, 0, 0, 0);

    public (int width, int height) initialSize;
    public (int x, int y) lastMove;

    private List<List<PlayerMark>> matrix; // 2d field with moves, 1 - one player, 2 - another, 0 - empty cell
    // (0, 0) - lower left
    // (N, N) - upper right

    private void Awake()
    {
        initialSize = (width, height);
    }

    

    private void Start()
    {
        // initial field matrix
        matrix = new List<List<PlayerMark>>();
        for (int i = 0; i < Height; i++)
        {
            matrix.Add(new List<PlayerMark>());
            for (int j = 0; j < Width; j++) 
            {
                matrix[i].Add(PlayerMark.Empty);
            }
        }

        Messenger.Broadcast(GameEvents.FIELD_UPDATED);
    }

    public void Clear()
    {
        foreach(var row in matrix)
        {
            for (int i = 0; i < row.Count; i++)
            {
                row[i] = PlayerMark.Empty;
            }
        }
    }

    public bool HasCell(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public PlayerMark GetPlayerAtCell(int x, int y)
    {
        return matrix[y][x];
    }

    private bool FallsWithinGrid(Vector3Int matrixPos)
    {
        return matrixPos.x < Width && matrixPos.x >= 0 && matrixPos.y < Height & matrixPos.y >= 0;
    }

    private bool IsCellEmpty(Vector3Int matrixPos)
    {
        if (matrix[matrixPos.y][matrixPos.x] != 0)
        {
            return false;
        }
        return true;
    }

    // returns distance from the point to every border
    private (int distance, FieldBorders border)[] DistanceToBorders(Vector3Int pos)
    {
        return new (int distance, FieldBorders border)[4]
        {
            (Height - pos.y - 1, FieldBorders.Top),
            (pos.y, FieldBorders.Bottom),
            (Width - pos.x - 1, FieldBorders.Right),
            (pos.x, FieldBorders.Left)
        };
    }

    // pushes 'borderToMove' by 'increase'
    private void Expand(int increase, FieldBorders borderToMove)
    {
        // add empty row where we need
        if (borderToMove == FieldBorders.Bottom || borderToMove == FieldBorders.Top)
        {
            for (int i = 0; i < increase; i++)
            {
                List<PlayerMark> newRow = new List<PlayerMark>();

                // init the row with zeroes
                for (int j = 0; j < Width; j++)
                {
                    newRow.Add(PlayerMark.Empty);
                }
                matrix.Insert(borderToMove == FieldBorders.Top ? Height : 0, newRow);
            }
            Height += increase;
        }

        // add empty column where we need
        if (borderToMove == FieldBorders.Left || borderToMove == FieldBorders.Right)
        {

            // add a cell to every row
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < increase; j++)
                {
                    matrix[i].Insert(borderToMove == FieldBorders.Right ? Width : 0, PlayerMark.Empty);
                }
            }
            Width += increase;
        }

        switch (borderToMove)
        {
            case FieldBorders.Top:
                totalIncrease.yTop += increase;
                break;
            case FieldBorders.Bottom:
                lastMove.y += increase;
                totalIncrease.yBot += increase;
                break;
            case FieldBorders.Left:
                lastMove.x += increase;
                totalIncrease.xLeft += increase;
                break;
            case FieldBorders.Right:
                totalIncrease.xRight += increase;
                break;
        }

    }

    private void UpdateSize(Vector3Int pos)
    {
        foreach (var distanceBorderPair in DistanceToBorders(pos))
        {
            if (distanceBorderPair.distance <= expandingDistance)
            {
                Expand(expandingDistance - distanceBorderPair.distance + 1, distanceBorderPair.border);
            }
        }
    } 

    public void PutPlayer(Vector3Int matrixPos, PlayerMark player)
    {
        // check if the pos falls within the current field
        if (!FallsWithinGrid(matrixPos))
        {
            return;
        }

        if (!IsCellEmpty(matrixPos))
        {
            return;
        }

        matrix[matrixPos.y][matrixPos.x] = player;

        lastMove = (matrixPos.x, matrixPos.y);

        // if we approach borders, resize the field
        UpdateSize(matrixPos);

        Messenger.Broadcast(GameEvents.FIELD_UPDATED);
    }
}
