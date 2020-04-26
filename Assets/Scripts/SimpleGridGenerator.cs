using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGridGenerator : MonoBehaviour
{
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;
    [SerializeField] private float distanceBtwnCrystals = 65f;
    [SerializeField] private Transform gridCenter = null;
    [SerializeField] private Transform gridParent = null;

    [SerializeField] private GameObject[] gridPointPrefabs = null;


    public T[][] RandomSimpleGrid<T>() where T : MonoBehaviour, IGridPoint
    {
        T[][] generatedGrid;

        generatedGrid = new T[rows][];
        for (int i = 0; i < generatedGrid.Length; i++)
        {
            generatedGrid[i] = new T[columns];
        }
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                generatedGrid[i][j] = InstantiateRandomPoint<T>();
                generatedGrid[i][j].Initialize();
                generatedGrid[i][j].SetDebugText();
            }
        }
        generatedGrid = PlacePoints(generatedGrid);
        generatedGrid = SetUpPointsIndexes(generatedGrid);

        return generatedGrid as T[][];
    }

    private T InstantiateRandomPoint<T>() where T : MonoBehaviour, IGridPoint
    {
        int rndPointPrefab = Random.Range(0, gridPointPrefabs.Length);

        T newRndPoint = Instantiate(gridPointPrefabs[rndPointPrefab], gridParent).GetComponent<T>();

        return newRndPoint;
    }

    public T[][] RandomizePoints<T>(T[][] generatedGrid) where T : IGridPoint
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                T temp = generatedGrid[i][j];
                int randomRow = Random.Range(0, rows);
                int randomCol = Random.Range(0, columns);

                generatedGrid[i][j] = generatedGrid[randomRow][randomCol];
                generatedGrid[randomRow][randomCol] = temp;
            }
        }

        return generatedGrid;
    }

    public T[][] PlacePoints<T>(T[][] generatedGrid) where T : MonoBehaviour, IGridPoint
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

        return generatedGrid;
    }

    public T[][] SetUpPointsIndexes<T>(T[][] generatedGrid) where T : IGridPoint
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

        return generatedGrid;
    }
}
