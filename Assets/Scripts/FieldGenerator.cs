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
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float distanceBtwnCrystals = 1f;
    [SerializeField] private Transform gridCenter;
    [SerializeField] private Transform crystalsParent;

    [SerializeField] private GenerationType generationType;
    [SerializeField] private Field field;
    [SerializeField] private GameObject[] crystalPrefabs;

    private void Start()
    {
        InitializeField();
    }

    private void InitializeField()
    {
        Crystal[,] initialGrid = null;

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
        Crystal[,] newRandomizedGrid = new Crystal[gridWidth, gridHeight];
        
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                newRandomizedGrid[i, j] = InstantiateRandomCrystal();
            }
        }

        newRandomizedGrid = GetPlacedCrystalsFrom(newRandomizedGrid);

        if(field.HasMatchOn(newRandomizedGrid))
        {
            return RandomSimpleGrid();
        }

        if(field.HasPossibleMovesOn(newRandomizedGrid))
        {
            return newRandomizedGrid;
        }
        else
        {
            return RandomSimpleGrid();
        }
    }

    private Crystal[,] GetPlacedCrystalsFrom(Crystal [,] grid)
    {
        Vector3 horizontalOffset = Vector3.right * distanceBtwnCrystals * gridWidth / 2;
        Vector3 verticalOffset = Vector3.up * distanceBtwnCrystals * gridHeight / 2;
        Vector3 topLeft = gridCenter.transform.position - horizontalOffset - verticalOffset;

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                grid[i, j] = InstantiateRandomCrystal();
                horizontalOffset = Vector3.right * distanceBtwnCrystals * (i + 0.5f);
                verticalOffset = Vector3.up * distanceBtwnCrystals * (j + 0.5f);
                grid[i, j].transform.position = topLeft + horizontalOffset + verticalOffset;
            }
        }

        return grid;
    }

    private Crystal InstantiateRandomCrystal()
    {
        int rndPrefabIndex = Random.Range(0, crystalPrefabs.Length);
        Crystal rndCrystal = Instantiate(crystalPrefabs[rndPrefabIndex], crystalsParent).GetComponent<Crystal>();
        return rndCrystal;
    }
}
