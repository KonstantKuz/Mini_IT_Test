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
    public static Action<(Crystal from, Crystal to)> OnCrystalDragged;

    public void Initialize(Crystal[,] crystalGrid)
    {
        this.crystalGrid = crystalGrid;

        OnCrystalSelected += FillSelections;
    }

    private void FillSelections(Crystal selectedCrystal)
    {
        if(!firstSelected)
        {
            firstSelected = selectedCrystal;
        }
        else if(!secondSelected)
        {
            secondSelected = selectedCrystal;
            ChangeCrystals();
            firstSelected = null;
            secondSelected = null;
        }
    }

    private void ChangeCrystals()
    {
        Vector3 firstPos = firstSelected.transform.position;
        Vector3 secondPos = secondSelected.transform.position;

        crystalGrid[firstSelected.x, firstSelected.y].transform.position = secondPos;
        crystalGrid[secondSelected.x, secondSelected.y].transform.position = firstPos;

        crystalGrid[firstSelected.x, firstSelected.y] = secondSelected;
        crystalGrid[secondSelected.x, secondSelected.y] = firstSelected;

        int firstX = firstSelected.x;
        int firstY = firstSelected.y;
        int secondX = secondSelected.x;
        int secondY = secondSelected.y;

        firstSelected.x = secondX;
        firstSelected.y = secondY;
        secondSelected.x = firstX;
        secondSelected.y = firstY;

        RemoveMatches();

        //crystalGrid[i, j].GetComponentInChildren<UnityEngine.UI.Text>().text = $"({i},{j})";
        //crystalGrid[randomRow, randomCol].GetComponentInChildren<UnityEngine.UI.Text>().text = $"({randomRow},{randomCol})";
    }

    [ContextMenu("RemoveMatches")]
    public void RemoveMatches()
    {
        List<Match> matches = MatchListOn(crystalGrid);

        for (int i = 0; i < matches.Count; i++)
        {
            for (int j = 0; j < matches[i].line.Count; j++)
            {
                matches[i].line[j].gameObject.SetActive(false);
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
