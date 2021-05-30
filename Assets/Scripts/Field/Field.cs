using System;
using System.Collections.Generic;
using UnityEngine;


// the game field representation and related methods
public class Field : MonoBehaviour
{
    // when we move in a cell closer than 5 cells from the boundaries, we expand the matrix
    [SerializeField]
    private int expandingDistance = 5;

    [SerializeField]
    private int height;

    [SerializeField]
    private int lastMovesToStore = 5;

    public List<(int x, int y)> lastMoves = new List<(int x, int y)>();

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

    public (int x, int y) stableLastMove;

    private List<List<PlayerMark>> matrix; // 2d field with moves, 1 - one player, 2 - another, 0 - empty cell
    // (0, 0) - lower left
    // (N, N) - upper right

    private void Awake()
    {
        initialSize = (width, height);
    }


    // returns the last player to move in the field
    public PlayerMark GetLastPlayerToMove()
    {
        return GetPlayerAtCell(stableLastMove.x, stableLastMove.y);
    }

    // whether the specified position is not further than "distance" to the positions from "lastMoves"
    // if it is close enough to at least 1 of them, it's true
    public bool IsCloseToLastMoves((int x, int y) pos, int distance)
    {
        if (lastMoves.Count == 0) return true; 
        foreach((int x, int y) move in lastMoves)
        {
            if (Math.Max(Math.Abs(move.x - pos.x), Math.Abs(move.y - pos.y)) <= distance)
            {
                return true;
            }
        }
        return false;
    }


    // serializable info
    public FieldOptions GetFieldData()
    {
        FieldOptions options = new FieldOptions();
        options.matrix = HelperFunctions.DeepMatrixCopy(matrix);
        options.lastMoves = new List<(int x, int y)>(lastMoves);
        options.lastMovesToStore = lastMovesToStore;
        options.stableLastMove = stableLastMove;
        options.initialSize = initialSize;
        options.totalIncrease = totalIncrease;
        options.expandingDistance = expandingDistance;
        options.height = height;
        options.width = width;
        return options;
    }

    // restore field from serializable info
    public void CopyField(FieldOptions field, bool sendMsg = true)
    {
        matrix = HelperFunctions.DeepMatrixCopy(field.matrix);
        lastMoves = new List<(int x, int y)>(field.lastMoves);
        lastMovesToStore = field.lastMovesToStore;
        height = field.height;
        width = field.width;
        expandingDistance = field.expandingDistance;
        totalIncrease = field.totalIncrease;
        initialSize = field.initialSize;
        stableLastMove = field.stableLastMove;
        if (sendMsg)
        {
            Messenger.Broadcast(GameEvents.FIELD_UPDATED);
        }
    }
    

    private void Start()
    {
        // initial field matrix
        Reset();

        Messenger.Broadcast(GameEvents.FIELD_UPDATED);
    }


    // restore the field to its initial state
    public void Reset(bool sendMsg = true)
    {
        Width = initialSize.width;
        Height = initialSize.height;

        stableLastMove = (0, 0);
        lastMoves = new List<(int x, int y)>();

        totalIncrease = (0, 0, 0, 0);
        matrix = new List<List<PlayerMark>>();
        for (int i = 0; i < Height; i++)
        {
            matrix.Add(new List<PlayerMark>());
            for (int j = 0; j < Width; j++)
            {
                matrix[i].Add(PlayerMark.Empty);
            }
        }
        if (sendMsg)
        {
            Messenger.Broadcast(GameEvents.FIELD_UPDATED);
        }
    }

    // whether the field contains a cell with such coordinates
    public bool HasCell(int stableX, int stableY)
    {
        Vector2Int realMatrixPos = StablePosToMatrixPos(new Vector2Int(stableX, stableY));
        return realMatrixPos.x >= 0 && realMatrixPos.y >= 0 && realMatrixPos.x < Width && realMatrixPos.y < Height;
    }


    // returns the player at the specified cell
    public PlayerMark GetPlayerAtCell(int stableX, int stableY)
    {
        Vector2Int realMatrixPos = StablePosToMatrixPos(new Vector2Int(stableX, stableY));
        return matrix[realMatrixPos.y][realMatrixPos.x];
    }


    // whether there are no players in the cell
    private bool IsCellEmpty(Vector2Int stableMatrixPos)
    {
        Vector2Int realMatrixPos = StablePosToMatrixPos(stableMatrixPos);
        if (matrix[realMatrixPos.y][realMatrixPos.x] != 0)
        {
            return false;
        }
        return true;
    }

    // if nothing prevents us from moving into the cell (basically, if it exists and is empty)
    public bool CellCompliesWithRules(Vector2Int stableMatrixPos)
    {
        return HasCell(stableMatrixPos.x, stableMatrixPos.y) && IsCellEmpty(stableMatrixPos);
    }

    // returns distance from the point to every border
    private (int distance, FieldBorders border)[] DistanceToBorders(Vector2Int stableMatrixPos)
    {
        Vector2Int realMatrixPos = StablePosToMatrixPos(stableMatrixPos);
        return new (int distance, FieldBorders border)[4]
        {
            (Height - realMatrixPos.y - 1, FieldBorders.Top),
            (realMatrixPos.y, FieldBorders.Bottom),
            (Width - realMatrixPos.x - 1, FieldBorders.Right),
            (realMatrixPos.x, FieldBorders.Left)
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
                totalIncrease.yBot += increase;
                break;
            case FieldBorders.Left:
                totalIncrease.xLeft += increase;
                break;
            case FieldBorders.Right:
                totalIncrease.xRight += increase;
                break;
        }

    }


    // updates the size of the matrix according to the move
    private void UpdateSize(Vector2Int stableMatrixPos)
    {
        foreach (var distanceBorderPair in DistanceToBorders(stableMatrixPos))
        {
            if (distanceBorderPair.distance <= expandingDistance)
            {
                Expand(expandingDistance - distanceBorderPair.distance + 1, distanceBorderPair.border);
            }
        }
    } 

    // converts stable pos to matrix pos
    private Vector2Int StablePosToMatrixPos(Vector2Int stableMatrixPos)
    {
        return stableMatrixPos + new Vector2Int(
            totalIncrease.xLeft + initialSize.width / 2,
            totalIncrease.yBot + initialSize.height / 2);
    }


    // converts matrix pos to stable pos
    private Vector2Int MatrixPosToStablePos(Vector2Int realMatrixPos)
    {
        return realMatrixPos - new Vector2Int(
            totalIncrease.xLeft + initialSize.width / 2,
            totalIncrease.yBot + initialSize.height / 2);
    }


    // returns the bounds of the field (in stable representation)
    public (int xLeft, int xRight, int yBot, int yTop) GetStableBounds()
    {
        Vector2Int stableBotLeft = MatrixPosToStablePos(new Vector2Int(0, 0));
        Vector2Int stableTopRight = MatrixPosToStablePos(new Vector2Int(Width - 1, Height - 1));

        return (stableBotLeft.x, stableTopRight.x, stableBotLeft.y, stableTopRight.y);
    }

    // puts the player into the field, expanding it afterwards
    public void PutPlayer(Vector2Int stableMatrixPos, PlayerMark player, bool sendMsg = true)
    {
        Vector2Int realMatrixPos = StablePosToMatrixPos(stableMatrixPos);

        // check if the pos falls within the current field
        if (!HasCell(stableMatrixPos.x, stableMatrixPos.y))
        {
            return;
        }

        if (!IsCellEmpty(stableMatrixPos))
        {
            return;
        }

        matrix[realMatrixPos.y][realMatrixPos.x] = player;

        stableLastMove = (stableMatrixPos.x, stableMatrixPos.y);
            lastMoves.Add(stableLastMove);
            if (lastMoves.Count > lastMovesToStore)
            {
                lastMoves.RemoveAt(0);
            }

        // if we approach borders, resize the field
        UpdateSize(stableMatrixPos);

        if (sendMsg)
        {
            Messenger.Broadcast(GameEvents.FIELD_UPDATED);
        }
    }
}
