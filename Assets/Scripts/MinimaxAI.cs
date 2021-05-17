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

    private Vector2Int GetRandomAvailablePos()
    {
        List<Vector2Int> available = new List<Vector2Int>();
        var bounds = field.GetStableBounds();
        for (int i = bounds.xLeft; i <= bounds.xRight; i++)
        {
            for (int j = bounds.yBot; j <= bounds.yTop; j++)
            {
                if (field.GetPlayerAtCell(i, j) == PlayerMark.Empty)
                {
                    available.Add(new Vector2Int(i, j));
                }
            }
        }

        int ind = new System.Random().Next(available.Count);
        return available[ind];
    }

    // positive score for player1
    // negative score for player2

    private delegate bool IsGameOver();

    private bool IsGameOverTimed()
    {
        return false;
    }

    private bool IsGameOverDifficulty(DifficultyGameAnalyzer analyzer)
    {
        return analyzer.GetGameStatus() == DifficultyGameStatus.Defeated;
    }

    

    private List<(int x, int y)> GetAvailableMovesInRectOrder((int x, int y) centralCell,
        (int xLeft, int xRight, int yBot, int yTop) bounds)
    {

        int[] GetBothPositiveAndNegative(int x)
        {
            return new int[] { -x, x };
        }

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


    private double GetLinePrice(List<Line> onePlayerSublines)
    {
        string log = "";
        int totalSpace = 0;
        int emptyBetween = 0;
        int emptyEdgesCount = 0;
        double playerSeries = 0;
        for (int i = 0; i < onePlayerSublines.Count; i++)
        {
            Line subLine = onePlayerSublines[i];
            log += $"{field.GetPlayerAtCell(subLine.stableEnd1.x, subLine.stableEnd1.y)}x{subLine.length} ";
            if (field.GetPlayerAtCell(subLine.stableEnd1.x, subLine.stableEnd1.y) == PlayerMark.Empty)
            {
                if (i == 0 || i == onePlayerSublines.Count - 1)
                {
                    emptyEdgesCount++;
                }
                else
                {
                    emptyBetween += subLine.length;
                }
            }
            else
            {
                if (subLine.length >= winLine)
                {
                    log += " 1000000";
                    Debug.Log(log);
                    return 1000000;
                }
                playerSeries += Math.Pow(subLine.length, 3) * 10;
            }
            totalSpace += subLine.length;
        }

        if (totalSpace < winLine)
        {
            return 0;
        }

        // improve playerSeries based on other data
        playerSeries *= emptyEdgesCount + 1;
        playerSeries /= emptyBetween + 1;

        if (playerSeries > 30)
        {
            log += playerSeries;
            Debug.Log(log);
        }
        return playerSeries;
    }

    private (double p1H, double p2H) GetHeuristicsForField(GameAnalyzer analyzer)
    {
        (double totalP1H, double totalP2H) = (0, 0);

        List<List<Line>> lines = analyzer.ScanField(field.stableLastMove, maxRangeFromLastMove);

        foreach(List<Line> line in lines)
        {
            List<Line> onePlayerSublines = new List<Line>();
            PlayerMark previousPlayer = PlayerMark.Empty;

            for (int i = 0; i < line.Count; i++)
            {
                PlayerMark player = field.GetPlayerAtCell(line[i].stableEnd1.x, line[i].stableEnd1.y);
                // no player discovered yet
                if (previousPlayer == PlayerMark.Empty)
                {
                    previousPlayer = player;
                }
                
                if (player != PlayerMark.Empty)
                {
                    // player line abrupted by another player, so flush the line
                    if (player != previousPlayer || i == line.Count - 1)
                    {
                        double collected = GetLinePrice(onePlayerSublines);
                        if (previousPlayer == PlayerMark.Player1)
                        {
                            totalP1H += collected;
                        }
                        else if (previousPlayer == PlayerMark.Player2)
                        {
                            totalP2H += collected;
                        }


                        if (onePlayerSublines.Count > 0)
                        {
                            Line lastSubline = new Line();
                            try
                            {

                                lastSubline = onePlayerSublines[onePlayerSublines.Count - 1];
                            }
                            catch
                            {
                                Debug.Log("ERROR");
                            }
                            onePlayerSublines = new List<Line>();
                            if (field.GetPlayerAtCell(lastSubline.stableEnd1.x, lastSubline.stableEnd1.y) == PlayerMark.Empty)
                            {
                                onePlayerSublines.Add(lastSubline);
                            }
                        }
                        onePlayerSublines.Add(line[i]);
                        previousPlayer = player;
                    }
                    else
                    {
                        // continue
                        onePlayerSublines.Add(line[i]);
                    }
                }
                else
                {
                    onePlayerSublines.Add(line[i]);
                }
                
            }
            
            //PlayerMark player = field.GetPlayerAtCell(line.stableEnd1.x, line.stableEnd1.y);
            //int additionalH = line.length * line.length * 10;
            //if (player == PlayerMark.Player1)
            //{
            //    totalP1H += additionalH;
            //}
            //else if (player == PlayerMark.Player2)
            //{
            //    totalP2H += additionalH;
            //}
        }
        
        return (totalP1H, totalP2H);
    }



    // stable pos
    public Vector2Int GetBestPosition(PlayerMark player, GameMode mode)
    {
        IsGameOver isGameOver;
        if (mode == GameMode.Difficulty)
        {
            isGameOver = () => IsGameOverDifficulty(new DifficultyGameAnalyzer(field, winLine));
        }
        else
        {
            isGameOver = IsGameOverTimed;
        }
        var bestResult = Minimax(movesToCalculate, player, isGameOver, new GameAnalyzer(field, winLine));
        return new Vector2Int(bestResult.posToMove.x, bestResult.posToMove.y);
    }

    // positive score for player1
    // negative score for player2
    private (double score, (int x, int y) posToMove) Minimax(int depth, PlayerMark maximizing,
        IsGameOver isGameOver, GameAnalyzer analyzer, double alpha = double.NegativeInfinity, double beta = double.PositiveInfinity)
    {
        if (depth == 0 || isGameOver())
        {
            var heuristics = GetHeuristicsForField(analyzer);
            if (heuristics.p1H == 160)
            {
                Debug.Log(1);
            }
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
        foreach ((int i, int j) in GetAvailableMovesInRectOrder(centralCell, bounds))
        {
            field.PutPlayer(new Vector2Int(i, j), maximizing);
            var branchBestResult = Minimax(depth - 1,
                maximizing == PlayerMark.Player1 ? PlayerMark.Player2 : PlayerMark.Player1,
                isGameOver, analyzer, alpha, beta);

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
