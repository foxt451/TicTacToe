using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DifficultyGameAnalyzer: GameAnalyzer
{
    public DifficultyGameAnalyzer(Field field, int lineLength) : base(field, lineLength)
    {
    }


    // returns Defeated if we have a row of 'lineLength'
    public DifficultyGameStatus GetGameStatus()
    {
        foreach (var direction in directions)
        {
            Line line = GetLineInFullDirection(direction, field.lastMove, true, lineLength);
            if (line.length >= lineLength)
            {
                return DifficultyGameStatus.Defeated;
            }
        }
        return DifficultyGameStatus.Continuing;
    }


}

