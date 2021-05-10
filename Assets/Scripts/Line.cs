using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Line
{
    public int length;
    public (int x, int y) botLeftCell; // (0, 0)
    public (int x, int y) topRightCell; // (N, N)

    public Line (int length, (int x, int y) botLeftCell, (int x, int y) topRightCell)
    {
        this.length = length;
        this.botLeftCell = botLeftCell;
        this.topRightCell = topRightCell;
    }
}
