using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGridGenerator : MonoBehaviour
{
    [SerializeField] private int rows = 6;
    [SerializeField] private int columns = 6;
    [SerializeField] private float distanceBtwnPoints = 65f;
    [SerializeField] private Transform gridCenter = null;
    [SerializeField] private Transform gridParent = null;

    [SerializeField] private GameObject[] gridPointPrefabs = null;

    public T[][] GetNewRandomGrid<T>() where T : MonoBehaviour, IGridPoint
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
            }
        }
        generatedGrid = PlacePoints(generatedGrid);
        generatedGrid = SetUpPointsIndexes(generatedGrid);

        return generatedGrid as T[][];
    }

    public T[][] GetNewEmptyGrid<T>() where T : MonoBehaviour, IGridPoint
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
                generatedGrid[i][j] = null;
            }
        }

        return generatedGrid as T[][];
    }

    public Vector3[][] GetPositionsGrid()
    {
        Vector3[][] positionsGrid;

        positionsGrid = new Vector3[rows][];
        for (int i = 0; i < positionsGrid.Length; i++)
        {
            positionsGrid[i] = new Vector3[columns];
        }

        Vector3 horizontalOffset = Vector3.up * distanceBtwnPoints * rows / 2;
        Vector3 verticalOffset = Vector3.right * distanceBtwnPoints * columns / 2;
        Vector3 topLeft = gridCenter.transform.position - horizontalOffset - verticalOffset;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                horizontalOffset = Vector3.up * distanceBtwnPoints * (i + 0.5f);
                verticalOffset = Vector3.right * distanceBtwnPoints * (j + 0.5f);
                positionsGrid[i][j] = topLeft + horizontalOffset + verticalOffset;
            }
        }
        return positionsGrid;
    }

    public T InstantiateRandomPoint<T>() where T : MonoBehaviour, IGridPoint
    {
        int rndPointPrefab = Random.Range(0, gridPointPrefabs.Length);

        T newRndPoint = Instantiate(gridPointPrefabs[rndPointPrefab], gridParent).GetComponent<T>();
        newRndPoint.Initialize();

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
        Vector3 horizontalOffset = Vector3.up * distanceBtwnPoints * rows / 2;
        Vector3 verticalOffset = Vector3.right * distanceBtwnPoints * columns / 2;
        Vector3 topLeft = gridCenter.transform.position - horizontalOffset - verticalOffset;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                horizontalOffset = Vector3.up * distanceBtwnPoints * (i + 0.5f);
                verticalOffset = Vector3.right * distanceBtwnPoints * (j + 0.5f);
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

    private void OnDrawGizmos()
    {
        Vector3 horizontalOffset = Vector3.up * distanceBtwnPoints * rows / 2;
        Vector3 verticalOffset = Vector3.right * distanceBtwnPoints * columns / 2;
        Vector3 topLeft = gridCenter.transform.position - horizontalOffset - verticalOffset;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                horizontalOffset = Vector3.up * distanceBtwnPoints * (i + 0.5f);
                verticalOffset = Vector3.right * distanceBtwnPoints * (j + 0.5f);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(topLeft + horizontalOffset + verticalOffset, 5f);
            }
        }
    }
}
