using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TimedGameAnalyzer : GameAnalyzer
{

    // TODO
    // add game analyzer serialization
    public TimedGameAnalyzer(Field field, int lineLength) : base(field, lineLength)
    {
    }

    private HashSet<(int x, int y)> accountedCells = new HashSet<(int x, int y)>();

    public TimedGameAnalyzerInfo GetSerializableInfo()
    {
        TimedGameAnalyzerInfo info = new TimedGameAnalyzerInfo();
        info.accountedCells = new HashSet<(int x, int y)>(accountedCells);
        return info;
    }

    public void Reconstruct(TimedGameAnalyzerInfo info)
    {
        accountedCells = new HashSet<(int x, int y)>(info.accountedCells);
    }


    // returns Defeated if we have a row of 'lineLength'
    public (int player1Score, int player2Score) GetGameScore()
    {
        int player1Score = 0, player2Score = 0;
        foreach (var direction in directions)
        {
            Line line = GetLineInFullDirection(direction, field.stableLastMove, false, 0);
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
        return (player1Score, player2Score);
    }


}