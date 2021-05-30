using System.Collections.Generic;


// analyzer for timed mode
public class TimedGameAnalyzer : GameAnalyzer
{
    public TimedGameAnalyzer(Field field, int lineLength) : base(field, lineLength)
    {
    }

    // the cells we already have added points for
    private HashSet<(int x, int y)> accountedCells = new HashSet<(int x, int y)>();

    // serializable info
    public TimedGameAnalyzerInfo GetSerializableInfo()
    {
        TimedGameAnalyzerInfo info = new TimedGameAnalyzerInfo();
        info.accountedCells = new HashSet<(int x, int y)>(accountedCells);
        return info;
    }

    // reconstruct from serializable info
    public void Reconstruct(TimedGameAnalyzerInfo info)
    {
        accountedCells = new HashSet<(int x, int y)>(info.accountedCells);
    }

    // game score, calculated for the last moves stored in the field (+1 for every unaccounted cell)
    public (int player1Score, int player2Score) GetGameScore()
    {
        int player1Score = 0, player2Score = 0;
        foreach (var lastCell in field.lastMoves)
        {
            foreach (var direction in directions)
            {
                Line line = GetLineInFullDirection(direction, lastCell, false, 0);
                if (line.length >= lineLength)
                {
                    foreach ((int x, int y) cell in line.GetLineCells())
                    {
                        if (!accountedCells.Contains(cell))
                        {
                            if (field.GetPlayerAtCell(cell.x, cell.y) == PlayerMark.Player1)
                            {
                                player1Score++;
                            }
                            else if (field.GetPlayerAtCell(cell.x, cell.y) == PlayerMark.Player2)
                            {
                                player2Score++;
                            }
                            accountedCells.Add(cell);
                        }
                    }
                }
            }
        }
        return (player1Score, player2Score);
    }


}