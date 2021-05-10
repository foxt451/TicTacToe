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
        PlayerMark player = field.GetPlayerAtCell(lastMove.x, lastMove.y);
        // straight direction
        (int x, int y, int length) topRightCell = GetLastCellOfLineInSemiDirection(direction, lastMove, earlyExit, earlyExitLength);
        // reverse direction
        (int x, int y, int length) botLeftCell = GetLastCellOfLineInSemiDirection((-direction.deltaX, -direction.deltaY),
            lastMove, earlyExit, earlyExitLength - topRightCell.length + 1);
        return new Line(topRightCell.length + botLeftCell.length - 1, (botLeftCell.x, botLeftCell.y), 
            (topRightCell.x, topRightCell.y));
    }

    public GameAnalyzer(Field field, int lineLength)
    {
        this.field = field;
        this.lineLength = lineLength;
    }
}
