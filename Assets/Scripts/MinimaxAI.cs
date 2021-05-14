using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MinimaxAI : MonoBehaviour
{
    [SerializeField]
    private Field field;

    [SerializeField]
    private int movesToCalculate;

    [SerializeField]
    private int maxRangeFromExistingCells;

    //private Vector2Int GetRandomAvailablePos()
    //{
    //    List<Vector2Int> available = new List<Vector2Int>();
    //    var bounds = field.GetStableBounds();
    //    for (int i = bounds.xLeft; i <= bounds.xRight; i++)
    //    {
    //        for (int j = bounds.yBot; j <= bounds.yTop; j++)
    //        {
    //            if (field.GetPlayerAtCell(i, j) == PlayerMark.Empty)
    //            {
    //                available.Add(new Vector2Int(i, j));
    //            }
    //        }
    //    }

    //    int ind = new System.Random().Next(available.Count);
    //    return available[ind];
    //}

    // positive score for player1
    // negative score for player2
    private delegate int StaticAnalysis();

    private delegate bool IsGameOver();

    private bool IsGameOverTimed()
    {
        return false;
    }

    private bool IsGameOverDifficulty(DifficultyGameAnalyzer analyzer)
    {
        return analyzer.GetGameStatus() == DifficultyGameStatus.Defeated;
    }

    private int TimedStaticAnalysis(TimedGameAnalyzer analyzer)
    {
        // save state before analyzing
        TimedGameAnalyzerInfo info = analyzer.GetSerializableInfo();

        var deltaScores = analyzer.GetGameScore();

        // restore
        analyzer.Reconstruct(info);

        return deltaScores.player1Score - deltaScores.player2Score;
    }

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
    public Vector2Int GetBestPosition(PlayerMark player, GameMode mode, GameAnalyzer analyzer)
    {
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
        var bestResult = Minimax(movesToCalculate, player, getScore, isGameOver);
        return new Vector2Int(bestResult.posToMove.x, bestResult.posToMove.y);
    }

    // positive score for player1
    // negative score for player2
    private (int score, (int x, int y) posToMove) Minimax(int depth, PlayerMark maximizing, StaticAnalysis getScore,
        IsGameOver isGameOver)
    {
        if (depth == 0 || isGameOver())
        {
            return (getScore(), field.stableLastMove);
        }

        // save field state
        FieldOptions fieldInfo = field.GetFieldData();

        // find best scores
        int bestScore = maximizing == PlayerMark.Player1 ? int.MinValue : int.MaxValue;
        (int x, int y) bestPos = (0, 0);

        // traverse all possible positions
        var bounds = field.GetStableBounds();
        for (int i = bounds.xLeft; i <= bounds.xRight; i++)
        {
            for (int j = bounds.yBot; j <= bounds.yTop; j++)
            {
                // if the cell is >(x, x) away from other filled cells, prune it
                if (!field.IsCloserThanDistanceToOthers((i, j), maxRangeFromExistingCells))
                {
                    continue;
                }

                // only try empty cells
                if (field.GetPlayerAtCell(i, j) == PlayerMark.Empty)
                {
                    field.PutPlayer(new Vector2Int(i, j), maximizing);
                    var branchBestResult = Minimax(depth - 1,
                        maximizing == PlayerMark.Player1 ? PlayerMark.Player2 : PlayerMark.Player1,
                        getScore, isGameOver);
                    if (maximizing == PlayerMark.Player1)
                    {
                        if (bestScore < branchBestResult.score)
                        {
                            bestScore = branchBestResult.score;
                            bestPos = (i, j);
                        }
                    }
                    else
                    {
                        if (bestScore > branchBestResult.score)
                        {
                            bestScore = branchBestResult.score;
                            bestPos = (i, j);
                        }
                    }

                    // restore field after every branch
                    field.CopyField(fieldInfo);
                }
            }
        }

        return (bestScore, bestPos);
    }
}
