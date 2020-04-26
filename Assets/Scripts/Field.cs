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
    [SerializeField] private GridGenerator generator = null;
    [SerializeField] private int minCrystalsCountInMatch = 2;
    private Crystal[][] crystalGrid = null;
    private Vector3[][] positionsGrid = null;

    private Crystal firstSelected = null;
    private Crystal secondSelected = null;

    private List<Crystal> removed = null;

    public static Action<Crystal> OnCrystalSelected;
    public static Action<(Crystal dragged, Vector2 direction)> OnCrystalDragged;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        crystalGrid = generator.GenerateGrid(this);

        SetUpPositionsGrid();

        removed = new List<Crystal>();

        OnCrystalSelected += FillClickSelections;
    }

    private void SetUpPositionsGrid()
    {
        positionsGrid = new Vector3[crystalGrid.Length][];
        for (int i = 0; i < positionsGrid.Length; i++)
        {
            positionsGrid[i] = new Vector3[crystalGrid[i].Length];
            for (int j = 0; j < positionsGrid[i].Length; j++)
            {
                positionsGrid[i][j] = crystalGrid[i][j].transform.position;
            }
        }
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

            if (SelectedAreNeighbors())
            {
                RemoveMatchesAfterDelay(secondSelected.MoveTime);
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
        crystalGrid[firstSelected.rowIndex][firstSelected.columnIndex].MoveTo(positionsGrid[secondSelected.rowIndex][secondSelected.columnIndex]);
        crystalGrid[secondSelected.rowIndex][secondSelected.columnIndex].MoveTo(positionsGrid[firstSelected.rowIndex][firstSelected.columnIndex]);
    }

    private void SwipeCrystalsInGrid()
    {
        crystalGrid[firstSelected.rowIndex][firstSelected.columnIndex] = secondSelected;
        crystalGrid[secondSelected.rowIndex][secondSelected.columnIndex] = firstSelected;
    }

    private void SwipeCrystalsIndexes()
    {
        int firstRowIndex = firstSelected.rowIndex;
        int firstColumnIndex = firstSelected.columnIndex;
        int secondRowIndex = secondSelected.rowIndex;
        int secondColumnIndex = secondSelected.columnIndex;

        firstSelected.rowIndex = secondRowIndex;
        firstSelected.columnIndex = secondColumnIndex;
        secondSelected.rowIndex = firstRowIndex;
        secondSelected.columnIndex = firstColumnIndex;

        firstSelected.SetDebugText();
        secondSelected.SetDebugText();
    }

    private bool SelectedAreNeighbors()
    {
        return !(Mathf.Abs(firstSelected.rowIndex - secondSelected.rowIndex) > 1 || Mathf.Abs(firstSelected.columnIndex - secondSelected.columnIndex) > 1);
    }

    private void RemoveMatchesAfterDelay(float delay)
    {
        StartCoroutine(RemoveMatches(delay));
    }

    private IEnumerator RemoveMatches(float delay)
    {
        yield return new WaitForSeconds(delay);

        List<Match> matches = MatchListOn(crystalGrid);

        for (int i = 0; i < matches.Count; i++)
        {
            for (int j = 0; j < matches[i].line.Count; j++)
            {
                int rowIndexRemoveFrom = matches[i].line[j].rowIndex;
                int columnIndexRemoveFrom = matches[i].line[j].columnIndex;
                RemoveCrystal(crystalGrid[rowIndexRemoveFrom][columnIndexRemoveFrom]);
            }
        }
    }

    private void RemoveCrystal(Crystal removedCrystal)
    {
        removedCrystal.gameObject.SetActive(false);
        removed.Add(removedCrystal);
        crystalGrid[removedCrystal.rowIndex][removedCrystal.columnIndex] = null;
        AffectToAllCrystalsOver(removedCrystal);
    }

    public void AffectToAllCrystalsOver(Crystal removedCrystal)
    {
        int rows = crystalGrid.Length;

        for (int upperCrystalIndex = removedCrystal.rowIndex + 1; upperCrystalIndex < rows; upperCrystalIndex++)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            str.AppendLine("Move crystal from to :");
            str.AppendLine($"From : ({crystalGrid[upperCrystalIndex][removedCrystal.columnIndex].rowIndex},{crystalGrid[upperCrystalIndex][removedCrystal.columnIndex].columnIndex})");
            if(crystalGrid[upperCrystalIndex][removedCrystal.columnIndex].rowIndex==upperCrystalIndex)
            {
                crystalGrid[upperCrystalIndex][removedCrystal.columnIndex].rowIndex = upperCrystalIndex - 1;
                crystalGrid[upperCrystalIndex - 1][removedCrystal.columnIndex] = crystalGrid[upperCrystalIndex][removedCrystal.columnIndex];
                crystalGrid[upperCrystalIndex][removedCrystal.columnIndex].MoveTo(positionsGrid[upperCrystalIndex - 1][removedCrystal.columnIndex]);
            }
            else
            {
                crystalGrid[upperCrystalIndex - 1][removedCrystal.columnIndex] = new Crystal();
            }

            str.AppendLine($"To : ({crystalGrid[upperCrystalIndex][removedCrystal.columnIndex].rowIndex},{crystalGrid[upperCrystalIndex][removedCrystal.columnIndex].columnIndex})");
            Debug.Log(str.ToString());

            crystalGrid[upperCrystalIndex][removedCrystal.columnIndex].SetDebugText();
        }

        //RemoveMatchesAfterDelay(1f);
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
            if (grid[rowIndex][columnIndex].data.CrystalType == grid[rowIndex][columnIndex + i].data.CrystalType)
            {
                match.line.Add(grid[rowIndex][columnIndex + i]);
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
            if (grid[rowIndex][columnIndex].data.CrystalType == grid[rowIndex + i][columnIndex].data.CrystalType)
            {
                match.line.Add(grid[rowIndex + i][columnIndex]);
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
