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
    private int maxRangeFromLastMove;

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

    private int[] GetBothPositiveAndNegative(int x)
    {
        return new int[] { -x, x };
    }

    private List<(int x, int y)> GetRectangularPoses((int x, int y) centralCell,
        (int xLeft, int xRight, int yBot, int yTop) bounds)
    {
        List<(int x, int y)> result = new List<(int x, int y)>();
        int delta = 0;
        while ((centralCell.x + delta <= bounds.xRight ||
            centralCell.x - delta >= bounds.xLeft ||
            centralCell.y + delta <= bounds.yTop ||
            centralCell.y - delta >= bounds.yBot) && delta <= maxRangeFromLastMove)
        {
            var negAndPos = GetBothPositiveAndNegative(delta);
            foreach (int i in negAndPos)
            {
                for (int j = negAndPos[0]; j <= negAndPos[1]; j++)
                {
                    result.Add((centralCell.x + i, centralCell.y + j));
                    if (j != negAndPos[0] && j != negAndPos[1])
                    {
                        result.Add((centralCell.x + j, centralCell.y + i));
                    }
                }
            }
            delta++;
        }

        return result;
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
        IsGameOver isGameOver, int alpha = int.MinValue, int beta = int.MaxValue)
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

        // traverse all possible positions
        (int x, int y) centralCell = field.stableLastMove;

        var bounds = field.GetStableBounds();
        foreach ((int i, int j) in GetRectangularPoses(centralCell, bounds))
        {
            // skip cells out of bounds
            if (!field.HasCell(i, j))
            {
                continue;
            }

            // only try empty cells
            if (field.GetPlayerAtCell(i, j) == PlayerMark.Empty)
            {
                field.PutPlayer(new Vector2Int(i, j), maximizing);
                var branchBestResult = Minimax(depth - 1,
                    maximizing == PlayerMark.Player1 ? PlayerMark.Player2 : PlayerMark.Player1,
                    getScore, isGameOver, alpha, beta);

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
                field.CopyField(fieldInfo);

                if (beta <= alpha)
                {
                    break;
                }
            }
        }
        return (bestScore, bestPos);
    }
}
