﻿using System.Collections;
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

public class Match3Field : MonoBehaviour
{
    [SerializeField] private SimpleGridGenerator generator = null;
    [SerializeField] private int minCrystalsCountInMatch = 2;
    private Crystal[][] crystalGrid = null;
    private Vector3[][] positionsGrid = null;

    private Crystal firstSelected = null;
    private Crystal secondSelected = null;

    private List<Crystal> removed = null;

    public static Action<Crystal> OnCrystalSelected;

    #region DebugOptions
    private int GenerationAttemptCount = 1;
    #endregion

    private void OnEnable()
    {
        OnCrystalSelected += FillSelections;
    }

    private void OnDisable()
    {
        OnCrystalSelected -= FillSelections;
    }

    private void Start()
    {
        GenerationAttemptCount = 1;

        Initialize();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        GenerationAttemptCount = 1;

        Initialize();
    }

    public void Initialize()
    {

        if(crystalGrid == null)
        {
            crystalGrid = generator.GetNewRandomizedGrid<Crystal>();
        }
        else
        {
            crystalGrid = generator.RandomizePoints(crystalGrid);
            crystalGrid = generator.PlacePoints(crystalGrid);
            crystalGrid = generator.SetUpPointsIndexes(crystalGrid);
        }

        if (HasMatches())
        {
            GenerationAttemptCount++;
            Debug.Log($"Generated grid that already exists matches. Regenerate. Generation attempt == {GenerationAttemptCount}");
            Initialize();
            return;
        }

        if (!HasPossibleMovesOn(crystalGrid))
        {
            Initialize();
            return;
        }

        SetUpPositionsGrid();

        removed = new List<Crystal>();
    }

    private void SetUpPositionsGrid()
    {
        positionsGrid = generator.GetNewPositionsGrid();
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

            SwipeCrystals();

            if (SelectedAreNeighbors())
            {
                RemoveAllMatchesAfterDelay(secondSelected.MoveTime);
            }

            firstSelected = null;
            secondSelected = null;
        }
    }

    private void SwipeCrystals()
    {
        if (!SelectedAreNeighbors())
        {
            return;
        }

        SwipeCrystalsPositions();
        SwipeCrystalsInGrid();
        SwipeCrystalsIndexes();
    }

    private void SwipeCrystalsPositions()
    {
        crystalGrid[firstSelected.rowIndex][firstSelected.colIndex].MoveTo(positionsGrid[secondSelected.rowIndex][secondSelected.colIndex]);
        crystalGrid[secondSelected.rowIndex][secondSelected.colIndex].MoveTo(positionsGrid[firstSelected.rowIndex][firstSelected.colIndex]);
    }

    private void SwipeCrystalsInGrid()
    {
        crystalGrid[firstSelected.rowIndex][firstSelected.colIndex] = secondSelected;
        crystalGrid[secondSelected.rowIndex][secondSelected.colIndex] = firstSelected;
    }

    private void SwipeCrystalsIndexes()
    {
        int firstRowIndex = firstSelected.rowIndex;
        int firstColumnIndex = firstSelected.colIndex;
        int secondRowIndex = secondSelected.rowIndex;
        int secondColumnIndex = secondSelected.colIndex;

        firstSelected.rowIndex = secondRowIndex;
        firstSelected.colIndex = secondColumnIndex;
        secondSelected.rowIndex = firstRowIndex;
        secondSelected.colIndex = firstColumnIndex;

        firstSelected.SetDebugText();
        secondSelected.SetDebugText();
    }

    private bool SelectedAreNeighbors()
    {
        return !(Mathf.Abs(firstSelected.rowIndex - secondSelected.rowIndex) > 1 || Mathf.Abs(firstSelected.colIndex - secondSelected.colIndex) > 1);
    }

    private void RemoveMatchesAfterDelay(float delay, int onRowIndex, int onColIndex)
    {
        StartCoroutine(RemoveMatches(delay, onRowIndex, onColIndex));
    }

    private IEnumerator RemoveMatches(float delay, int onRowIndex, int onColIndex)
    {
        yield return new WaitForSeconds(delay);

        List<Match> matchList = new List<Match>();

        for (int i = -1; i < 1; i++)
        {
            Match horizontalMatch = GetHorizontalMatchOn(onRowIndex, onColIndex + i, crystalGrid);
            if (horizontalMatch.line.Count > minCrystalsCountInMatch)
            {
                matchList.Add(horizontalMatch);
            }
        }

        for (int i = -1; i < 1; i++)
        {
            Match verticalMatch = GetVerticalMatch(onRowIndex + i, onColIndex, crystalGrid);
            if (verticalMatch.line.Count > minCrystalsCountInMatch)
            {
                matchList.Add(verticalMatch);
            }
        }

        for (int i = 0; i < matchList.Count; i++)
        {
            for (int j = 0; j < matchList[i].line.Count; j++)
            {
                int rowIndexRemoveFrom = matchList[i].line[j].rowIndex;
                int columnIndexRemoveFrom = matchList[i].line[j].colIndex;
                RemoveCrystal(crystalGrid[rowIndexRemoveFrom][columnIndexRemoveFrom]);
            }
        }
    }

    private void RemoveAllMatchesAfterDelay(float delay)
    {
        StartCoroutine(RemoveAllMatches(delay));
    }

    private IEnumerator RemoveAllMatches(float delay)
    {
        yield return new WaitForSeconds(delay);

        List<Match> matchList = MatchListOn(crystalGrid);

        for (int i = 0; i < matchList.Count; i++)
        {
            for (int j = 0; j < matchList[i].line.Count; j++)
            {
                int rowIndexRemoveFrom = matchList[i].line[j].rowIndex;
                int columnIndexRemoveFrom = matchList[i].line[j].colIndex;
                RemoveCrystal(crystalGrid[rowIndexRemoveFrom][columnIndexRemoveFrom]);
            }
        }
    }

    private void RemoveCrystal(Crystal removedCrystal)
    {
        if(removedCrystal)
        {
            removedCrystal.gameObject.SetActive(false);
            removed.Add(removedCrystal);
            crystalGrid[removedCrystal.rowIndex][removedCrystal.colIndex] = null;
            AffectToAllCrystalsOver(removedCrystal);
        }
    }

    public void AffectToAllCrystalsOver(IGridPoint removedCrystal)
    {
        int rows = crystalGrid.Length;

        for (int upperCrystalIndex = removedCrystal.rowIndex + 1; upperCrystalIndex < rows; upperCrystalIndex++)
        {
            if(!crystalGrid[upperCrystalIndex][removedCrystal.colIndex])
            {
                continue;
            }
            if(crystalGrid[upperCrystalIndex][removedCrystal.colIndex].rowIndex==upperCrystalIndex)
            {
                crystalGrid[upperCrystalIndex][removedCrystal.colIndex].rowIndex = upperCrystalIndex - 1;
                crystalGrid[upperCrystalIndex - 1][removedCrystal.colIndex] = crystalGrid[upperCrystalIndex][removedCrystal.colIndex];
                crystalGrid[upperCrystalIndex][removedCrystal.colIndex].MoveTo(positionsGrid[upperCrystalIndex - 1][removedCrystal.colIndex]);
            }

            crystalGrid[upperCrystalIndex][removedCrystal.colIndex].SetDebugText();
        }
    }
    
    public List<Match> MatchListOn(Crystal[][] grid)
    {
        List<Match> matchList = new List<Match>();

        int rows = grid.Length;

        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            int columns = grid[rowIndex].Length;

            for (int colIndex = 0; colIndex < columns; colIndex++)
            {
                Match horizontalMatch = GetHorizontalMatchOn(rowIndex, colIndex, grid);
                if (horizontalMatch.line.Count > minCrystalsCountInMatch)
                {
                    matchList.Add(horizontalMatch);
                    colIndex += horizontalMatch.line.Count;
                }
            }
        }

        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            int columns = grid[rowIndex].Length;

            for (int colIndex = 0; colIndex < columns; colIndex++)
            {
                Match verticalMatch = GetVerticalMatch(rowIndex, colIndex, grid);
                if (verticalMatch.line.Count > minCrystalsCountInMatch)
                {
                    matchList.Add(verticalMatch);
                    colIndex += verticalMatch.line.Count;
                }
            }
        }

        Debug.Log($"MatchList count == {matchList.Count}");

        return matchList;
    }

    public bool HasMatches()
    {
        List<Match> matchList = new List<Match>();

        int rows = crystalGrid.Length;

        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            int columns = crystalGrid[rowIndex].Length;

            for (int colIndex = 0; colIndex < columns; colIndex++)
            {
                Match horizontalMatch = GetHorizontalMatchOn(rowIndex, colIndex, crystalGrid);
                if (horizontalMatch.line.Count > minCrystalsCountInMatch)
                {
                    matchList.Add(horizontalMatch);
                    colIndex += horizontalMatch.line.Count;

                    return true;
                }
            }
        }

        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            int columns = crystalGrid[rowIndex].Length;

            for (int colIndex = 0; colIndex < columns; colIndex++)
            {
                Match verticalMatch = GetVerticalMatch(rowIndex, colIndex, crystalGrid);
                if (verticalMatch.line.Count > minCrystalsCountInMatch)
                {
                    matchList.Add(verticalMatch);
                    colIndex += verticalMatch.line.Count;

                    return true;
                }
            }
        }

        return false;
    }

    private Match GetHorizontalMatchOn(int rowIndex, int columnIndex, Crystal[][] grid)
    {
        Match match = new Match();

        int columns = grid[rowIndex].Length;

        match.line.Add(grid[rowIndex][columnIndex]);

        for (int i = 1; columnIndex + i < columns; i++)
        {
            if (grid[rowIndex][columnIndex] == null || grid[rowIndex][columnIndex + i] == null)
            {
                return match;
            }

            Crystal current = grid[rowIndex][columnIndex];
            Crystal nextRight = grid[rowIndex][columnIndex + i];

            if (current.data.CrystalType == nextRight.data.CrystalType)
            {
                match.line.Add(nextRight);
            }
            else
            {
                return match;
            }
        }
        return match;
    }

    private Match GetVerticalMatch(int rowIndex, int columnIndex, Crystal[][] grid)
    {
        Match match = new Match();

        int rows = grid.Length;

        match.line.Add(grid[rowIndex][columnIndex]);

        for (int i = 1; rowIndex + i < rows; i++)
        {
            if (grid[rowIndex][columnIndex] == null || grid[rowIndex + i][columnIndex ] == null)
            {
                return match;
            }
            Crystal current = grid[rowIndex][columnIndex];
            Crystal nextUpper = grid[rowIndex + i][columnIndex];

            if (current.data.CrystalType == nextUpper.data.CrystalType)
            {
                match.line.Add(nextUpper);
            }
            else
            {
                return match;
            }
        }
        return match;
    }

    public bool HasPossibleMovesOn(Crystal[][] grid)
    {
        return true;
    }
}
