﻿using System.Collections.Generic;

public static class HelperFunctions
{
    // deep-copies a matrix
    public static List<List<T>> DeepMatrixCopy<T>(List<List<T>> matrix)
    {
        List<List<T>> newMatrix = new List<List<T>>();
        for (int i = 0; i < matrix.Count; i++)
        {
            newMatrix.Add(new List<T>());
            for (int j = 0; j < matrix[0].Count; j++)
            {
                newMatrix[i].Add(matrix[i][j]);
            }
        }
        return newMatrix;
    }
}

