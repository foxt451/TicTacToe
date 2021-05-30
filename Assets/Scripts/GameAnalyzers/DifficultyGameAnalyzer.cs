// analyzer for difficulty mode
public class DifficultyGameAnalyzer: GameAnalyzer
{
    public DifficultyGameAnalyzer(Field field, int lineLength) : base(field, lineLength)
    {
    }


    // returns Defeated if we have a row of 'lineLength'
    public DifficultyGameStatus GetGameStatus()
    {
        // when field is initially empty
        // the function returns Defeat
        // but it's useful
        // because this way AI moves in (0, 0)

        foreach (var direction in directions)
        {
            Line line = GetLineInFullDirection(direction, field.stableLastMove, true, lineLength);
            if (line.length >= lineLength && field.GetPlayerAtCell(line.stableEnd1.x, line.stableEnd1.y) != PlayerMark.Empty)
            {
                return DifficultyGameStatus.Defeated;
            }
        }
        return DifficultyGameStatus.Continuing;
    }


}

