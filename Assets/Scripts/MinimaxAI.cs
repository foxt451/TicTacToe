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

    [SerializeField]
    private int winLine;

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
    private delegate double StaticAnalysis();

    private delegate bool IsGameOver();

    private bool IsGameOverTimed()
    {
        return false;
    }

    private bool IsGameOverDifficulty(DifficultyGameAnalyzer analyzer)
    {
        return analyzer.GetGameStatus() == DifficultyGameStatus.Defeated;
    }

    private double TimedStaticAnalysis(TimedGameAnalyzer analyzer)
    {
        // save state before analyzing
        TimedGameAnalyzerInfo info = analyzer.GetSerializableInfo();

        var deltaScores = analyzer.GetGameScore();

        // restore
        analyzer.Reconstruct(info);

        return deltaScores.player1Score - deltaScores.player2Score;
    }

    private double DifficultyStaticAnalysis(DifficultyGameAnalyzer analyzer)
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

    private List<(int x, int y)> GetAvailableMoves((int x, int y) centralCell,
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
                    if (field.HasCell(centralCell.x + i, centralCell.y + j) &&
                        field.GetPlayerAtCell(centralCell.x + i, centralCell.y + j) == PlayerMark.Empty)
                    {
                        result.Add((centralCell.x + i, centralCell.y + j));
                    }
                    if (j != negAndPos[0] && j != negAndPos[1])
                    {
                        if (field.HasCell(centralCell.x + j, centralCell.y + i) &&
                            field.GetPlayerAtCell(centralCell.x + j, centralCell.y + i) == PlayerMark.Empty)
                        {
                            result.Add((centralCell.x + j, centralCell.y + i));
                        }
                    }
                }
            }
            delta++;
        }

        return result;
    }



    private (int deltaX, int deltaY)[] directions = new (int deltaX, int deltaY)[]
    {
        (0, 1), // vertical
        (1, 0), // horizontal
        (1, 1), // right-diagonal
        (-1, 1) // left-diagonal
    };

    private (double p1H, double p2H) GetHeuristicsForField((int xLeft, int xRight, int yBot, int yTop) boundsForAnalysis)
    {
        (int totalP1H, int totalP2H) = (0, 0);
        // horizontal
        for (int y = boundsForAnalysis.yBot; y <= boundsForAnalysis.yTop; y++)
        {
            (int x, int y) curCell = (boundsForAnalysis.xLeft, y);
            // scan right
            int player1Value = 0;
            int player2Value = 0;
            PlayerMark curPlayer = PlayerMark.Empty;
            while (field.HasCell(curCell.x, curCell.y))
            {
                PlayerMark player = field.GetPlayerAtCell(curCell.x, curCell.y);
                if (curPlayer == PlayerMark.Empty)
                {
                    curPlayer = player;
                    player1Value++;
                    player2Value++;
                }
                
                
                if (curPlayer == player && player != PlayerMark.Empty)
                {
                    if (curPlayer == PlayerMark.Player1)
                    {
                        player1Value *= 2;
                    }
                    else
                    {
                        player2Value *= 2;
                    }
                }
                else if (curPlayer == player && player == PlayerMark.Empty)
                {
                    player1Value++;
                    player2Value++;
                }
                else
                {
                    curPlayer = player;
                }
                
                curCell.x++;
            }
            totalP1H += player1Value;
            totalP2H += player2Value;
        }

        // vertical
        for (int x = boundsForAnalysis.xLeft; x <= boundsForAnalysis.xRight; x++)
        {
            (int x, int y) curCell = (x, boundsForAnalysis.yBot);
            // scan up
            int player1Value = 0;
            int player2Value = 0;
            PlayerMark curPlayer = PlayerMark.Empty;
            while (field.HasCell(curCell.x, curCell.y))
            {
                PlayerMark player = field.GetPlayerAtCell(curCell.x, curCell.y);
                if (curPlayer == PlayerMark.Empty)
                {
                    curPlayer = player;
                    player1Value++;
                    player2Value++;
                }


                if (curPlayer == player && player != PlayerMark.Empty)
                {
                    if (curPlayer == PlayerMark.Player1)
                    {
                        player1Value *= 2;
                    }
                    else
                    {
                        player2Value *= 2;
                    }
                }
                else if (curPlayer == player && player == PlayerMark.Empty)
                {
                    player1Value++;
                    player2Value++;
                }
                else
                {
                    curPlayer = player;
                }

                curCell.y++;
            }
            totalP1H += player1Value;
            totalP2H += player2Value;
        }

        // diagonal1
        for (int x = boundsForAnalysis.xLeft, y = boundsForAnalysis.yBot;
            x < boundsForAnalysis.xRight || y < boundsForAnalysis.yTop;)
        {
            (int x, int y) curCell = (x, boundsForAnalysis.yBot);
            // scan up
            int player1Value = 0;
            int player2Value = 0;
            PlayerMark curPlayer = PlayerMark.Empty;
            while (field.HasCell(curCell.x, curCell.y))
            {
                PlayerMark player = field.GetPlayerAtCell(curCell.x, curCell.y);
                if (curPlayer == PlayerMark.Empty)
                {
                    curPlayer = player;
                    player1Value++;
                    player2Value++;
                }


                if (curPlayer == player && player != PlayerMark.Empty)
                {
                    if (curPlayer == PlayerMark.Player1)
                    {
                        player1Value *= 2;
                    }
                    else
                    {
                        player2Value *= 2;
                    }
                }
                else if (curPlayer == player && player == PlayerMark.Empty)
                {
                    player1Value++;
                    player2Value++;
                }
                else
                {
                    curPlayer = player;
                }

                curCell.y++;
                curCell.x--;
            }
            totalP1H += player1Value;
            totalP2H += player2Value;

            if (x < boundsForAnalysis.xRight)
            {
                x++;
            }
            if (y < boundsForAnalysis.yTop)
            {
                y++;
            }
        }

        // d2
        for (int x = boundsForAnalysis.xRight, y = boundsForAnalysis.yBot;
            x > boundsForAnalysis.xLeft || y < boundsForAnalysis.yTop;)
        {
            (int x, int y) curCell = (x, boundsForAnalysis.yBot);
            // scan up
            int player1Value = 0;
            int player2Value = 0;
            PlayerMark curPlayer = PlayerMark.Empty;
            while (field.HasCell(curCell.x, curCell.y))
            {
                PlayerMark player = field.GetPlayerAtCell(curCell.x, curCell.y);
                if (curPlayer == PlayerMark.Empty)
                {
                    curPlayer = player;
                    player1Value++;
                    player2Value++;
                }


                if (curPlayer == player && player != PlayerMark.Empty)
                {
                    if (curPlayer == PlayerMark.Player1)
                    {
                        player1Value *= 2;
                    }
                    else
                    {
                        player2Value *= 2;
                    }
                }
                else if (curPlayer == player && player == PlayerMark.Empty)
                {
                    player1Value++;
                    player2Value++;
                }
                else
                {
                    curPlayer = player;
                }

                curCell.y++;
                curCell.x++;
            }
            totalP1H += player1Value;
            totalP2H += player2Value;

            if (x > boundsForAnalysis.xLeft)
            {
                x--;
            }
            if (y < boundsForAnalysis.yTop) 
            {
                y++;
            }
        }

        return (totalP1H, totalP2H);
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
    private (double score, (int x, int y) posToMove) Minimax(int depth, PlayerMark maximizing, StaticAnalysis getScore,
        IsGameOver isGameOver, double alpha = double.NegativeInfinity, double beta = double.PositiveInfinity)
    {
        if (isGameOver())
        {
            return (getScore() * 10000 * (depth + 1), field.stableLastMove);
        }
        else if (depth == 0)
        {
            var heuristics = GetHeuristicsForField((field.stableLastMove.x - maxRangeFromLastMove,
                field.stableLastMove.x + maxRangeFromLastMove, field.stableLastMove.y - maxRangeFromLastMove,
                field.stableLastMove.y + maxRangeFromLastMove));
            Debug.Log(heuristics);
            return ((heuristics.p1H - heuristics.p2H) * (depth + 1), field.stableLastMove);
        }

        // save field state
        FieldOptions fieldInfo = field.GetFieldData();

        // find best scores
        double bestScore = maximizing == PlayerMark.Player1 ? double.NegativeInfinity : double.PositiveInfinity;
        (int x, int y) bestPos = (0, 0);

        // traverse all possible positions
        (int x, int y) centralCell = field.stableLastMove;

        var bounds = field.GetStableBounds();
        foreach ((int i, int j) in GetAvailableMoves(centralCell, bounds))
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
        return (bestScore, bestPos);
    }
}
