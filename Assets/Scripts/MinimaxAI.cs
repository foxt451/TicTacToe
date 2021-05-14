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

    // stable pos
    public Vector2Int GetBestPosition()
    {
        return GetRandomAvailablePos();
    }
}
