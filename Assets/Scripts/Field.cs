using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Field : MonoBehaviour
{
    private Crystal[,] crystalGrid = null;
    private Crystal firstSelected = null;
    private Crystal secondSelected = null;
    
    public static Action<Crystal> OnCrystalSelected;

    public void Initialize(Crystal[,] crystalGrid)
    {
        this.crystalGrid = crystalGrid;
    }

    public bool HasMatchOn(Crystal[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {

            }
        }

        return false;
    }

    public bool HasPossibleMovesOn(Crystal[,] grid)
    {
        return false;
    }
}
