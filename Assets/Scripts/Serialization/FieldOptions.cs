using System;
using System.Collections.Generic;

[Serializable]
public class FieldOptions
{
    public List<List<PlayerMark>> matrix;
    public (int xRight, int xLeft,
        int yTop, int yBot) totalIncrease;

    public (int width, int height) initialSize;
    public (int x, int y) stableLastMove;
    public int width;
    public int height;
    public int expandingDistance;
}