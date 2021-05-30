using System.Collections.Generic;


// general analyzer, fits into both modes
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


    // what is the last cell we get moving into the specified direction? also, optional parameters to specify whether we want to include empty
    // cells and if we want to imagine that in the initial cell we have some player
    private (int x, int y, int length) GetLastCellOfLineInSemiDirection((int deltaX, int deltaY) semiDirection,
        (int x, int y) startCell, bool earlyExit, int earlyExitDistance, bool ignoreEmpty=false,
        bool imaginePlayer = false, PlayerMark imaginable = PlayerMark.Empty)
    {
        PlayerMark player = field.GetPlayerAtCell(startCell.x, startCell.y);
        if (imaginePlayer)
        {
            player = imaginable;
        }
        int length = 1;

        (int x, int y) nextCell = (startCell.x + semiDirection.deltaX, startCell.y + semiDirection.deltaY);
        if (field.HasCell(nextCell.x, nextCell.y))
        {
            PlayerMark nextPlayer = field.GetPlayerAtCell(nextCell.x, nextCell.y);
            while (field.HasCell(nextCell.x, nextCell.y) &&
                (nextPlayer == player || (ignoreEmpty && nextPlayer == PlayerMark.Empty)))
            {
                if (earlyExit && length >= earlyExitDistance)
                {
                    break;
                }
                startCell = (nextCell.x, nextCell.y);
                nextCell = (startCell.x + semiDirection.deltaX, startCell.y + semiDirection.deltaY);
                if (field.HasCell(nextCell.x, nextCell.y))
                {
                    if (field.GetPlayerAtCell(nextCell.x, nextCell.y) != PlayerMark.Empty || !ignoreEmpty)
                    {
                        nextPlayer = field.GetPlayerAtCell(nextCell.x, nextCell.y);
                    }
                }
                length++;
            }
        }
        return (startCell.x, startCell.y, length);
    }


    // the line we get moving into some direction (and its reverse) (also, parameters if we want to include empty cells)
    protected Line GetLineInFullDirection((int deltaX, int deltaY) direction, (int x, int y) lastMove, bool earlyExit,
        int earlyExitLength, bool ignoreEmpty=false, bool imaginePlayer = false, PlayerMark imaginable = PlayerMark.Empty)
    {
        // straight direction
        (int x, int y, int length) end1 = GetLastCellOfLineInSemiDirection(direction, lastMove, earlyExit, earlyExitLength,
            ignoreEmpty, imaginePlayer, imaginable);
        // reverse direction
        (int x, int y, int length) end2 = GetLastCellOfLineInSemiDirection((-direction.deltaX, -direction.deltaY),
            lastMove, earlyExit, earlyExitLength, ignoreEmpty, imaginePlayer, imaginable);
        return new Line(end1.length + end2.length - 1, (end2.x, end2.y), 
            (end1.x, end1.y));
    }


    // combos of cells we get moving into some direction, needed for heuristics
    public List<(int totalSpace, List<(int combo, bool isEmpty, bool isPosInSeries)> series)> GetPosAdvantage((int x, int y) pos, PlayerMark imaginablePlayer, int maxRange)
    {
        var result = new List<(int totalSpace, List<(int combo, bool isEmpty, bool isPosInSeries)> series)>();
        foreach((int deltaX, int deltaY) dir in directions)
        {
            int totalSpace = 0;
            List<(int combo, bool isEmpty, bool isPosInSeries)> series = new List<(int combo, bool isEmpty, bool isPosInSeries)>();

            Line line = GetLineInFullDirection(dir, pos, true, maxRange, true, true, imaginablePlayer);
            totalSpace = line.length;
            (int x, int y)[] lineCells = line.GetLineCells();
            PlayerMark previous = PlayerMark.Empty;
            int previousSeries = 0;
            bool containsPos = false;
            for (int i = 0; i < lineCells.Length; i++)
            {
                PlayerMark current = field.GetPlayerAtCell(lineCells[i].x, lineCells[i].y);
                // to avoid changing field, we imagine the player is at the initial pos
                if (lineCells[i].x == pos.x && lineCells[i].y == pos.y)
                {
                    containsPos = true;
                    current = imaginablePlayer;
                }
                if (i == 0)
                {
                    previous = current;
                    previousSeries++;
                }
                else
                {
                    if (previous == current)
                    {
                        previousSeries++;
                    }
                    else
                    {
                        //flush the series
                        series.Add((previousSeries, previous == PlayerMark.Empty,
                            containsPos));
                        containsPos = false;
                        previousSeries = 1;
                        previous = current;
                    }
                }
            }
            // flush remaining
            if (previousSeries > 0)
            {
                series.Add((previousSeries, previous == PlayerMark.Empty, containsPos));
            }
            result.Add((totalSpace, series));
        }
        return result;
    }

    public GameAnalyzer(Field field, int lineLength)
    {
        this.field = field;
        this.lineLength = lineLength;
    }
}
