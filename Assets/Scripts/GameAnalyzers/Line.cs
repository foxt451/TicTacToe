using System;

// line representation
public struct Line
{
    public int length;
    public (int x, int y) stableEnd1;
    public (int x, int y) stableEnd2;

    public Line (int length, (int x, int y) stableEnd1, (int x, int y) stableEnd2)
    {
        this.length = length;
        this.stableEnd1 = stableEnd1;
        this.stableEnd2 = stableEnd2;
    }


    // the array of line cells
    public (int stableX, int stableY)[] GetLineCells()
    {
        var result = new (int stableX, int stableY)[length];

        (int xDelta, int yDelta) = (Math.Sign(stableEnd2.x - stableEnd1.x),
            Math.Sign(stableEnd2.y - stableEnd1.y));
        (int x, int y) current = stableEnd1;

        for (int i = 0; i < length; i++)
        {
            result[i] = current;
            current = (current.x + xDelta, current.y + yDelta);
        }

        return result;
    }
}
