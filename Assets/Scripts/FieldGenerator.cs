using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GenerationType
{
    SimpleGrid,
    PatternBased,
}

public class FieldGenerator : MonoBehaviour
{
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;
    [SerializeField] private float distanceBtwnCrystals = 65f;
    [SerializeField] private Transform gridCenter;
    [SerializeField] private Transform crystalsParent;

    [SerializeField] private GenerationType generationType;
    [SerializeField] private Field field;
    [SerializeField] private GameObject[] crystalPrefabs;

    private Crystal[,] initialGrid = null;

    #region DebugOptions
    private int GenerationAttemptCount = 1;
    #endregion

    [ContextMenu("Generate")]
    public void Generate()
    {
        GenerationAttemptCount = 1;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        InitializeField();
        sw.Stop();
        Debug.Log($"Generation time == {sw.ElapsedMilliseconds} ms");
    }

    private void InitializeField()
    {
        switch (generationType)
        {
            case GenerationType.SimpleGrid:
                initialGrid = RandomSimpleGrid();
                break;
            case GenerationType.PatternBased:
                break;
        }

        field.Initialize(initialGrid);
    }

    private Crystal[,] RandomSimpleGrid()
    {
        if(initialGrid == null)
        {
            initialGrid = new Crystal[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    initialGrid[i, j] = InstantiateRandomCrystal();
                    initialGrid[i, j].SetDebugText();
                }
            }
        }
        else
        {
            RandomizeGrid();
        }

        PlaceCrystals();
        SetUpCrystals();

        if (field.MatchListOn(initialGrid).Count>0)
        {
            GenerationAttemptCount++;
            Debug.Log($"Generation attempt count == {GenerationAttemptCount}");
            return RandomSimpleGrid();
        }

        if(field.HasPossibleMovesOn(initialGrid))
        {
            return initialGrid;
        }
        else
        {
            return RandomSimpleGrid();
        }
    }

    private Crystal InstantiateRandomCrystal()
    {
        int rndPrefabIndex = Random.Range(0, crystalPrefabs.Length);
        Crystal rndCrystal = Instantiate(crystalPrefabs[rndPrefabIndex], crystalsParent).GetComponent<Crystal>();
        return rndCrystal;
    }

    private void RandomizeGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Crystal temp = initialGrid[i, j];
                int randomRow = Random.Range(0, rows);
                int randomCol = Random.Range(0, columns);

                initialGrid[i,j] = initialGrid[randomRow, randomCol];
                initialGrid[randomRow, randomCol] = temp;
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
                initialGrid[i, j].transform.position = topLeft + horizontalOffset + verticalOffset;
            }
        }
    }

    private void SetUpCrystals()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                initialGrid[i, j].x = i;
                initialGrid[i, j].y = j;
                initialGrid[i, j].SetDebugText();
            }
        }
    }
}
