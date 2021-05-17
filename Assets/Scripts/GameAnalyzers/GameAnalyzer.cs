using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAnalyzer
{
    protected Field field;
    protected int lineLength;

    // one direction consists of the straight and backwards directions
    // e.g. if you want a vertical line, you need deltaX = 0 and deltaY = 1
    // backwards direction can be achieved by inverting deltas
    protected (int deltaX, int deltaY)[] directions = new (int deltaX, int deltaY)[]
    {
        (0, 1), // vertical
        (1, 0), // horizontal
        (1, 1), // right-diagonal
        (-1, 1) // left-diagonal
    };

    private (int x, int y, int length) GetLastCellOfLineInSemiDirection((int deltaX, int deltaY) semiDirection,
        (int x, int y) startCell, bool earlyExit, int earlyExitDistance)
    {
        PlayerMark player = field.GetPlayerAtCell(startCell.x, startCell.y);
        int length = 1;

        (int x, int y) nextCell = (startCell.x + semiDirection.deltaX, startCell.y + semiDirection.deltaY);
        while (field.HasCell(nextCell.x, nextCell.y) &&
            field.GetPlayerAtCell(nextCell.x, nextCell.y) == player)
        {
            if (earlyExit && length >= earlyExitDistance)
            {
                break;
            }
            startCell = (nextCell.x, nextCell.y);
            nextCell = (startCell.x + semiDirection.deltaX, startCell.y + semiDirection.deltaY);
            length++;
        }
        return (startCell.x, startCell.y, length);
    }

    protected Line GetLineInFullDirection((int deltaX, int deltaY) direction, (int x, int y) lastMove, bool earlyExit, int earlyExitLength)
    {
        // straight direction
        (int x, int y, int length) end1 = GetLastCellOfLineInSemiDirection(direction, lastMove, earlyExit, earlyExitLength);
        // reverse direction
        (int x, int y, int length) end2 = GetLastCellOfLineInSemiDirection((-direction.deltaX, -direction.deltaY),
            lastMove, earlyExit, earlyExitLength - end1.length + 1);
        return new Line(end1.length + end2.length - 1, (end2.x, end2.y), 
            (end1.x, end1.y));
    }


    private List<Line> GetLinesInFullDirection((int deltaX, int deltaY) direction, (int x, int y) lastMove, bool scanPartially, int maxScanL)
    {
        List<Line> lines = new List<Line>();
        // go down the field following the reverse direction
        (int x, int y) curCell = lastMove;
        (int x, int y) nextCell = (curCell.x - direction.deltaX, curCell.y - direction.deltaY);    
        while (field.HasCell(nextCell.x, nextCell.y))
        {
            if (scanPartially && (Math.Abs(nextCell.x - lastMove.x) > maxScanL || Math.Abs(nextCell.y - lastMove.y) > maxScanL))
            {
                break;
            }
            curCell = nextCell;
            nextCell = (curCell.x - direction.deltaX, curCell.y - direction.deltaY);
        }

        // scan sublines in straight direction
        while(field.HasCell(curCell.x, curCell.y) && 
            (Math.Abs(curCell.x - lastMove.x) <= maxScanL && Math.Abs(nextCell.y - lastMove.y) <= maxScanL))
        {
            (int x, int y, int length) subLineEnd = GetLastCellOfLineInSemiDirection(direction,
                curCell, false, 0);
            lines.Add(new Line(subLineEnd.length, (subLineEnd.x, subLineEnd.y), curCell));
            curCell = (subLineEnd.x + direction.deltaX, subLineEnd.y + direction.deltaY);
        }

        return lines;
    }

    public List<List<Line>> ScanField((int x, int y) lastMove, int maxScanL)
    {

        List<List<Line>> lines = new List<List<Line>>();

        var bounds = field.GetStableBounds();
        foreach (var dir in directions)
        {
            (int x, int y) cur;
            cur.x = dir.deltaY < 0 ? Math.Max(lastMove.x - maxScanL, bounds.xLeft) :
                Math.Min(lastMove.x + maxScanL, bounds.xRight);
            cur.y = dir.deltaX > 0 ? Math.Max(lastMove.y - maxScanL, bounds.yBot) :
                Math.Min(lastMove.y + maxScanL, bounds.yTop);
            if (dir.deltaY != 0)
            {
                for (; cur.x <= Math.Min(lastMove.x + maxScanL, bounds.xRight) &&
                    cur.x >= Math.Max(lastMove.x - maxScanL, bounds.xLeft);
                    cur.x += (dir.deltaY < 0 ? 1 : -1))
                {
                    lines.Add(GetLinesInFullDirection(dir, cur, true, maxScanL));
                }
            }

            cur.x -= (dir.deltaY < 0 ? 1 : -1);
            cur.y += (dir.deltaX > 0 ? 1 : -1);

            if (dir.deltaX != 0)
            {
                for (; cur.y <= Math.Min(lastMove.y + maxScanL, bounds.yTop) &&
                    cur.y >= Math.Max(lastMove.y - maxScanL, bounds.yBot);
                    cur.y += (dir.deltaX > 0 ? 1 : -1))
                {
                    lines.Add(GetLinesInFullDirection(dir, cur, true, maxScanL));
                }
            }
        }
        return lines;
    }

    public GameAnalyzer(Field field, int lineLength)
    {
        this.field = field;
        this.lineLength = lineLength;
    }
}
