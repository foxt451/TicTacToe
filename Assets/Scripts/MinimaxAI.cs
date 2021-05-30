using System.Collections.Generic;
using UnityEngine;
using System;
using C5;

// class for AI (minimax algorithm)
public class MinimaxAI : MonoBehaviour
{
    // field to calculate the best move on
    [SerializeField]
    private Field field;

    // how far we are ready to calculate the moves (recurstion max depth)
    [SerializeField]
    private int movesToCalculate;

    // we don't want to see beyond some distance from last x moves, otherwise after the field gets too big, it'll be overhead
    [SerializeField]
    private int maxRangeFromLastMoves;

    // line length to win or start get points for
    [SerializeField]
    private int winLine;

    // how much moves we even look at (based on heuristics)
    [SerializeField]
    private int maxPosNumberEachLevel = 100;

    // positive score for player1
    // negative score for player2
    private delegate int StaticAnalysis();

    // whether the game is over with current field
    private delegate bool IsGameOver();

    // the game in timed mode (for AI at least) is never over
    private bool IsGameOverTimed()
    {
        return false;
    }

    // will return true when someone hits line of 5
    private bool IsGameOverDifficulty(DifficultyGameAnalyzer analyzer)
    {
        return analyzer.GetGameStatus() == DifficultyGameStatus.Defeated;
    }

    // difference between scores
    private int TimedStaticAnalysis(TimedGameAnalyzer analyzer)
    {
        // save state before analyzing
        TimedGameAnalyzerInfo info = analyzer.GetSerializableInfo();

        var deltaScores = analyzer.GetGameScore();

        // restore
        analyzer.Reconstruct(info);

        return deltaScores.player1Score - deltaScores.player2Score;
    }

    // 1 if player 1 builds a line, -1 - player 2, 0 - otherwise
    private int DifficultyStaticAnalysis(DifficultyGameAnalyzer analyzer)
    {
        (int player1, int player2) scores = (0, 0);
        if (analyzer.GetGameStatus() == DifficultyGameStatus.Defeated)
        {
            if (field.GetLastPlayerToMove() == PlayerMark.Player1)
            {
                scores.player1++;
            }
            else if (field.GetLastPlayerToMove() == PlayerMark.Player2)
            {
                scores.player2++;
            }
        }
        return scores.player1 - scores.player2;
    }

    // stable pos
    // returns best position our AI has found
    public Vector2Int GetBestPosition(PlayerMark player, GameMode mode, GameAnalyzer analyzer)
    {
        float bestPosStart = Time.realtimeSinceStartup;

        StaticAnalysis getScore;
        IsGameOver isGameOver;
        if (mode == GameMode.Difficulty)
        {
            getScore = () => DifficultyStaticAnalysis((DifficultyGameAnalyzer)analyzer);
            isGameOver = () => IsGameOverDifficulty((DifficultyGameAnalyzer)analyzer);
        }
        else
        {
            getScore = () => TimedStaticAnalysis((TimedGameAnalyzer)analyzer);
            isGameOver = IsGameOverTimed;
        }
        var bestResult = Minimax(movesToCalculate, player, getScore, isGameOver, new GameAnalyzer(field, winLine));
        float bestPosEnd = Time.realtimeSinceStartup;
        return new Vector2Int(bestResult.posToMove.x, bestResult.posToMove.y);
    }

    // positive score for player1
    // negative score for player2
    // returns best score and score for current maximizing player (recursive function, see minimax)
    private (int score, (int x, int y) posToMove) Minimax(int depth, PlayerMark maximizing, StaticAnalysis getScore,
        IsGameOver isGameOver, GameAnalyzer analyzer, int alpha = int.MinValue, int beta = int.MaxValue)
    {
        if (depth == 0 || isGameOver())
        {
            return (getScore() * (depth + 1), field.stableLastMove);
        }

        // save field state
        FieldOptions fieldInfo = field.GetFieldData();

        // find best scores
        int bestScore = maximizing == PlayerMark.Player1 ? int.MinValue : int.MaxValue;
        (int x, int y) bestPos = (0, 0);

        IntervalHeap<((int x, int y) pos, double h)> moves = GetPosesSortedByHeuristics(analyzer);
        for (int k = 0; k < maxPosNumberEachLevel && !moves.IsEmpty; k++)
        {
            ((int x, int y) pos, double h) = moves.DeleteMax();
            (int i, int j) = pos;
            
            field.PutPlayer(new Vector2Int(i, j), maximizing, false);
            (int score, (int x, int y) posToMove) branchBestResult = Minimax(depth - 1,
                    maximizing == PlayerMark.Player1 ? PlayerMark.Player2 : PlayerMark.Player1,
                    getScore, isGameOver, analyzer, alpha, beta);
            

            if (maximizing == PlayerMark.Player1)
            {
                if (bestScore < branchBestResult.score)
                {
                    bestScore = branchBestResult.score;
                    bestPos = (i, j);
                }
                alpha = Math.Max(alpha, branchBestResult.score);
            }
            else
            {
                if (bestScore > branchBestResult.score)
                {
                    bestScore = branchBestResult.score;
                    bestPos = (i, j);
                }
                beta = Math.Min(beta, branchBestResult.score);
            }

            // restore field after every branch
            field.CopyField(fieldInfo, false);

            // alpha-beta pruning
            if (beta <= alpha)
            {
                break;
            }
            
        }

        return (bestScore, bestPos);
    }

    // sorts positions by their heuristics
    private IntervalHeap<((int x, int y) pos, double h)> GetPosesSortedByHeuristics(GameAnalyzer analyzer)
    {
        IntervalHeap<((int x, int y) pos, double h)> moves = new IntervalHeap<((int x, int y) pos, double h)>(
            Comparer<((int x, int y) pos, double h)>.Create(
                (((int x, int y) pos, double h) v1, ((int x, int y) pos, double h) v2) =>
                v1.h > v2.h ? 1 : (v1.h == v2.h ? 0 : -1)));
        (int xLeft, int xRight, int yBot, int yTop) bounds = field.GetStableBounds();
        for (int x = bounds.xLeft; x <= bounds.xRight; x++)
        {
            for (int y = bounds.yBot; y <= bounds.yTop; y++)
            {
                if (!field.HasCell(x, y))
                {
                    continue;
                }
                if (field.GetPlayerAtCell(x, y) != PlayerMark.Empty)
                {
                    continue;
                }
                if (!field.IsCloseToLastMoves((x, y), maxRangeFromLastMoves))
                {
                    
                    continue;
                }
                double posH = Heuristics2Players1Pos((x, y), analyzer);
                moves.Add(((x, y), posH));
            }
        }
        return moves;
    }

    // calculates heuristics for a pos by adding its advantage for player1 and 2
    private double Heuristics2Players1Pos((int i, int j) pos, GameAnalyzer analyzer)
    {
        double h1 = Heuristics1Player1Pos(pos, analyzer, PlayerMark.Player1);
        double h2 = Heuristics1Player1Pos(pos, analyzer, PlayerMark.Player2);
        return h1 + h2;
    }
    // calculates avdantage of the pos for one player
    private double Heuristics1Player1Pos((int i, int j) pos, GameAnalyzer analyzer, PlayerMark imaginablePlayer)
    {
        List<(int totalSpace, List<(int combo, bool isEmpty, bool isPosInSeries)> series)> advantage = analyzer.GetPosAdvantage(pos, imaginablePlayer,
            maxRangeFromLastMoves);
        double h = 0;
        foreach ((int totalSpace, List<(int combo, bool isEmpty, bool isPosInSeries)> series) in advantage)
        {
            if (totalSpace < winLine)
            {
                continue;
            }

            double dirH = 0;
            int emptyBetween = 0;
            int freeEdges = 0;
            for (int i = 0; i < series.Count; i++)
            {
                (int combo, bool isEmpty, bool isPosInSeries) value = series[i];
                if (value.isEmpty && (i == 0 || i == series.Count - 1))
                {
                    freeEdges++;
                }
                else if (value.isEmpty)
                {
                    emptyBetween+=value.combo;
                }
                else
                {
                    int combo = value.combo % (value.isPosInSeries ? (winLine + 1) : winLine);
                    dirH += Math.Pow(combo * 2, 3) * 10;
                    dirH += (value.combo - combo) * 1000;
                }
            }

            dirH *= freeEdges + 1;
            dirH /= Math.Log(emptyBetween + 2, 2);
            h += dirH;
        }
        return h;
    }
}
