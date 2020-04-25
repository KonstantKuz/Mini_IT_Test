using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Match
{
    public List<Crystal> line;

    public Match()
    {
        line = new List<Crystal>();
    }
}

public class Field : MonoBehaviour
{
    private Crystal[,] crystalGrid = null;
    private Crystal firstSelected = null;
    private Crystal secondSelected = null;

    private List<Crystal> removed = null;

    public static Action<Crystal> OnCrystalSelected;
    public static Action<(Crystal dragged, Vector2 direction)> OnCrystalDragged;
    public static Action OnMovingDone;

    public void Initialize(Crystal[,] crystalGrid)
    {

        this.crystalGrid = crystalGrid;

        removed = new List<Crystal>();

        OnCrystalSelected += FillClickSelections;
        OnMovingDone += RemoveMatches;
    }

    private void FillClickSelections(Crystal selectedCrystal)
    {
        if(!firstSelected)
        {
            firstSelected = selectedCrystal;
        }
        else if(!secondSelected)
        {
            secondSelected = selectedCrystal;
            SwipeCrystals();
            firstSelected = null;
            secondSelected = null;
        }
    }

    private void SwipeCrystals()
    {
        Vector3 firstPos = firstSelected.transform.position;
        Vector3 secondPos = secondSelected.transform.position;
        crystalGrid[firstSelected.x, firstSelected.y].MoveTo(secondPos, true);
        crystalGrid[secondSelected.x, secondSelected.y].MoveTo(firstPos,false);

        crystalGrid[firstSelected.x, firstSelected.y] = secondSelected;
        crystalGrid[secondSelected.x, secondSelected.y] = firstSelected;

        int firstX = firstSelected.x;
        int firstY = firstSelected.y;
        firstSelected.x = secondSelected.x;
        firstSelected.y = secondSelected.y;
        secondSelected.x = firstSelected.x;
        secondSelected.y = firstSelected.y;
    }

    private void RemoveMatches()
    {
        List<Match> matches = MatchListOn(crystalGrid);

        for (int i = 0; i < matches.Count; i++)
        {
            for (int j = 0; j < matches[i].line.Count; j++)
            {
                int rowIndexRemoveFrom = matches[i].line[j].x;
                int columnIndexRemoveFrom = matches[i].line[j].y;
                RemoveCrystal(rowIndexRemoveFrom, columnIndexRemoveFrom);
            }
        }
    }

    private void RemoveCrystal(int rowIndex, int columnIndex)
    {
        Crystal removedCrystal = crystalGrid[rowIndex, columnIndex];
        removedCrystal.gameObject.SetActive(false);
        removed.Add(removedCrystal);
        AffectCrystalsOver(removedCrystal);
    }

    public void AffectCrystalsOver(Crystal removedCrystal)
    {
        int rows = crystalGrid.GetLength(0);
        
        for (int i = removedCrystal.x + 1; i < rows; i++)
        {
            if (crystalGrid[i, removedCrystal.y] != null)
            {
                int nextCrystalIndex = i;
                nextCrystalIndex--;
                crystalGrid[i, removedCrystal.y].MoveTo(crystalGrid[nextCrystalIndex, removedCrystal.y].transform.position, false);
                crystalGrid[i, removedCrystal.y].x = nextCrystalIndex;
                crystalGrid[nextCrystalIndex, removedCrystal.y] = crystalGrid[i, removedCrystal.y];

                crystalGrid[i, removedCrystal.y].SetDebugText();
                crystalGrid[nextCrystalIndex, removedCrystal.y].SetDebugText();

                //grid[piece.col][row + 1] = grid[piece.col][row];
                //grid[piece.col][row] = null;
            }
        }
    }

    public List<Match> MatchListOn(Crystal[,] grid)
    {
        List<Match> matchList = new List<Match>();

        int rows = grid.GetLength(0);
        int columns = grid.GetLength(1);

        Debug.Log($"Rows == {rows}. Columns == {columns}.");

        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            for (int colIndex = 0; colIndex < columns; colIndex++)
            {
                Match horizontalMatch = GetHorizontalMatchOn(rowIndex, colIndex, grid);
                if (horizontalMatch.line.Count > 2)
                {
                    matchList.Add(horizontalMatch);
                    colIndex += horizontalMatch.line.Count;
                }
            }
        }

        for (int colIndex = 0; colIndex < columns; colIndex++)
        {
            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                Match verticalMatch = GetVerticalMatchOn(rowIndex, colIndex, grid);
                if (verticalMatch.line.Count > 2)
                {
                    matchList.Add(verticalMatch);
                    rowIndex += verticalMatch.line.Count;
                }
            }
        }

        Debug.Log($"MatchList count == {matchList.Count}");

        return matchList;
    }

    private Match GetHorizontalMatchOn(int rowIndex, int columnIndex, Crystal[,] grid)
    {
        Match match = new Match();

        int columns = grid.GetLength(1);
        
        match.line.Add(grid[rowIndex, columnIndex]);
        for (int i = 1; columnIndex + i < columns; i++)
        {
            if (grid[rowIndex, columnIndex].crystalType == grid[rowIndex, columnIndex + i].crystalType)
            {
                match.line.Add(grid[rowIndex, columnIndex + i]);
            }
            else
            {
                return match;
            }
        }
        return match;
    }

    private Match GetVerticalMatchOn(int rowIndex, int columnIndex, Crystal[,] grid)
    {
        Match match = new Match();

        int rows = grid.GetLength(0);
        match.line.Add(grid[rowIndex, columnIndex]);
        for (int i = 1; rowIndex + i < rows; i++)
        {
            if (grid[rowIndex, columnIndex].crystalType == grid[rowIndex + i, columnIndex].crystalType)
            {
                match.line.Add(grid[rowIndex + i, columnIndex]);
            }
            else
            {
                return match;
            }
        }
        return match;
    }

    public bool HasPossibleMovesOn(Crystal[,] grid)
    {
        return true;
    }
}
