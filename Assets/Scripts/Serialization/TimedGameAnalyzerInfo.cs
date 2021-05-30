using System;
using System.Collections.Generic;

// serializable info for TimedGameAnalyzer
[Serializable]
public class TimedGameAnalyzerInfo
{
    public HashSet<(int x, int y)> accountedCells;
}

