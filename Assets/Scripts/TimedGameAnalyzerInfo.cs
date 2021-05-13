using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class TimedGameAnalyzerInfo
{
    public HashSet<(int x, int y)> accountedCells;
}

