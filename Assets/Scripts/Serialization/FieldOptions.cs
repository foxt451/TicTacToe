using System;
using System.Collections.Generic;

// serialization info for Field
[Serializable]
public class FieldOptions
{
    public List<List<PlayerMark>> matrix;
    public List<(int x, int y)> lastMoves;
    public int lastMovesToStore;

    public (int xRight, int xLeft,
        int yTop, int yBot) totalIncrease;

    public (int width, int height) initialSize;
    public (int x, int y) stableLastMove;
    public int width;
    public int height;
    public int expandingDistance;
}