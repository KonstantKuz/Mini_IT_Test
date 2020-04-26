using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGridGenerator : GridGenerator
{
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;
    [SerializeField] private float distanceBtwnCrystals = 65f;
    [SerializeField] private Transform gridCenter;
    [SerializeField] private Transform crystalsParent;

    [SerializeField] private GameObject[] crystalPrefabs;

    private Crystal[][] generatedGrid = null;

    #region DebugOptions
    private int GenerationAttemptCount = 1;
    #endregion

    #region GridGenerator

    public override Crystal[][] GenerateGrid(Field field)
    {
        GenerationAttemptCount = 1;

        generatedGrid = RandomSimpleGrid(field);

        return generatedGrid;
    }
    #endregion

    private Crystal[][] RandomSimpleGrid(Field field)
    {
        if(generatedGrid == null)
        {
            generatedGrid = new Crystal[rows][];
            for (int i = 0; i < generatedGrid.Length; i++)
            {
                generatedGrid[i] = new Crystal[columns];
            }
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    generatedGrid[i][j] = InstantiateRandomCrystal();
                    generatedGrid[i][j].Initialize();
                    generatedGrid[i][j].SetDebugText();
                }
            }
        }
        else
        {
            RandomizeCrystals();
        }

        PlaceCrystals();
        SetUpCrystals();

        if (field.MatchListOn(generatedGrid).Count>0)
        {
            GenerationAttemptCount++;
            Debug.Log($"Generated grid that already exists matches. Regenerate. Generation attempt == {GenerationAttemptCount}");
            return RandomSimpleGrid(field);
        }

        if(field.HasPossibleMovesOn(generatedGrid))
        {
            return generatedGrid;
        }
        else
        {
            return RandomSimpleGrid(field);
        }
    }

    private Crystal InstantiateRandomCrystal()
    {
        int rndCrystalPrefabIndex = Random.Range(0, crystalPrefabs.Length);

        Crystal newCrystal = Instantiate(crystalPrefabs[rndCrystalPrefabIndex], crystalsParent).GetComponent<Crystal>();

        return newCrystal;
    }

    private void RandomizeCrystals()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Crystal temp = generatedGrid[i][j];
                int randomRow = Random.Range(0, rows);
                int randomCol = Random.Range(0, columns);

                generatedGrid[i][j] = generatedGrid[randomRow][randomCol];
                generatedGrid[randomRow][randomCol] = temp;
            }
        }
    }

    private void PlaceCrystals()
    {
        Vector3 horizontalOffset = Vector3.up * distanceBtwnCrystals * rows / 2;
        Vector3 verticalOffset = Vector3.right * distanceBtwnCrystals * columns / 2;
        Vector3 topLeft = gridCenter.transform.position - horizontalOffset - verticalOffset;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                horizontalOffset = Vector3.up * distanceBtwnCrystals * (i + 0.5f);
                verticalOffset = Vector3.right * distanceBtwnCrystals * (j + 0.5f);
                generatedGrid[i][j].transform.position = topLeft + horizontalOffset + verticalOffset;
            }
        }
    }

    private void SetUpCrystals()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                generatedGrid[i][j].rowIndex = i;
                generatedGrid[i][j].columnIndex = j;
                generatedGrid[i][j].SetDebugText();
            }
        }
    }
}
