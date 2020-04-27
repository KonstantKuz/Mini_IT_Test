using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MergeField : MonoBehaviour
{
    [SerializeField] private SimpleGridGenerator generator = null;
    [SerializeField] private Vector2 newShipSpawnPeriod;
    [SerializeField] private Transform shipsParent;
    [SerializeField] private List<GameObject> shipPrefabs = null;

    private Ship[][] shipGrid = null;
    private Vector3[][] positionsGrid = null;

    private Ship firstSelected = null;
    private Ship secondSelected = null;

    public static Action<Ship> OnShipSelected;
    public static Action<(Ship selected, Vector2 direction)> OnShipDragged;

    public static Action OnShipsMerged;

    private Dictionary<ShipType, GameObject> shipPrefabsDict = null;

    private void OnEnable()
    {
        OnShipSelected += FillSelectionsAndTryMerge;
        OnShipDragged += TryFillSelectionsAndTryMerge;
        LevelProgress.OnProgressFinished += delegate { StopAllCoroutines(); };
    }

    private void OnDisable()
    {
        OnShipSelected -= FillSelectionsAndTryMerge;
        OnShipDragged -= TryFillSelectionsAndTryMerge;
        LevelProgress.OnProgressFinished -= delegate { StopAllCoroutines(); };
    }

    private void Start()
    {
        Initialize();
        StartCoroutine(StartSpawnShips());
    }
    
    public void Initialize()
    {
        CheckShipsParent();
        SetUpPrefabsDictionary();
        GetPositionsGrid();
        GetEmptyShipGrid();
    }

    private void CheckShipsParent()
    {
        if(shipsParent)
        {
            return;
        }

        shipsParent = transform;
    }

    private void SetUpPrefabsDictionary()
    {
        shipPrefabsDict = new Dictionary<ShipType, GameObject>();
        for (int i = 0; i < shipPrefabs.Count; i++)
        {
            shipPrefabsDict.Add(shipPrefabs[i].GetComponent<Ship>().data.ShipType, shipPrefabs[i]);
        }
    }

    private void GetPositionsGrid()
    {
        positionsGrid = generator.GetPositionsGrid();
    }

    private void GetEmptyShipGrid()
    {
        shipGrid = generator.GetNewEmptyGrid<Ship>();
    }

    private void FillSelectionsAndTryMerge(Ship selectedShip)
    {
        if (ReferenceEquals(firstSelected, null))
        {
            firstSelected = selectedShip;
        }
        else if (ReferenceEquals(secondSelected, null))
        {
            secondSelected = selectedShip;

            TryMergeShips();
        }
    }

    private void TryFillSelectionsAndTryMerge((Ship selected, Vector2 direction) data)
    {
        firstSelected = data.selected;

        int rowIndexToMerge = firstSelected.rowIndex + Mathf.Clamp((int)data.direction.y, -1, 1);
        int colIndexToMerge = firstSelected.colIndex + Mathf.Clamp((int)data.direction.x, -1, 1);

        TryGetSecondSelected(rowIndexToMerge, colIndexToMerge);

        if(secondSelected)
        {
            TryMergeShips();
            Debug.Log($"Seleted = ({firstSelected.rowIndex},{firstSelected.colIndex}). ToMerge = ({rowIndexToMerge},{colIndexToMerge}). Direction == ({data.direction.x},{data.direction.y})");
        }
    }

    private void TryGetSecondSelected(int rowIndexToMerge, int colIndexToMerge)
    {
        int rows = shipGrid.Length;
        rowIndexToMerge = Mathf.Clamp(rowIndexToMerge, 0, rows);

        if (rowIndexToMerge >= 0 && rowIndexToMerge < rows)
        {
            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                int columns = shipGrid[rowIndex].Length;
                colIndexToMerge = Mathf.Clamp(colIndexToMerge, 0, columns);

                if (colIndexToMerge >= 0 && colIndexToMerge < columns)
                {
                    secondSelected = shipGrid[rowIndexToMerge][colIndexToMerge];
                }
            }
        }
    }

    private void TryMergeShips()
    {
        if (SelectedAreNeighbors() && SelectedAreIdentical() && SelectedAreNotTheSame())
        {
            StartCoroutine(MergeShips());
        }
        else
        {
            CleanSelections();
        }
    }

    private bool SelectedAreNeighbors()
    {
        return !(Mathf.Abs(firstSelected.rowIndex - secondSelected.rowIndex) > 1 || Mathf.Abs(firstSelected.colIndex - secondSelected.colIndex) > 1);
    }

    private bool SelectedAreIdentical()
    {
        return firstSelected.data.ShipType == secondSelected.data.ShipType;
    }

    private bool SelectedAreNotTheSame()
    {
        return firstSelected != secondSelected;
    }

    private void CleanSelections()
    {
        firstSelected = null;
        secondSelected = null;
    }

    private IEnumerator MergeShips()
    {
        firstSelected.MoveTo(secondSelected.transform.position);
        yield return new WaitForSeconds(firstSelected.MoveTime);

        ShipType mergeResult = (ShipType)((int)firstSelected.data.ShipType + 1);
        Ship resultShip = Instantiate(shipPrefabsDict[mergeResult], shipsParent).GetComponent<Ship>();

        AffectMergeResult(resultShip);

        Destroy(firstSelected.gameObject);
        Destroy(secondSelected.gameObject);

        CleanSelections();

        OnShipsMerged?.Invoke();
    }

    private void AffectMergeResult(Ship mergedShip)
    {
        mergedShip.transform.position = positionsGrid[secondSelected.rowIndex][secondSelected.colIndex];

        shipGrid[firstSelected.rowIndex][firstSelected.colIndex] = null;
        shipGrid[secondSelected.rowIndex][secondSelected.colIndex] = mergedShip;
        shipGrid[secondSelected.rowIndex][secondSelected.colIndex].rowIndex = secondSelected.rowIndex;
        shipGrid[secondSelected.rowIndex][secondSelected.colIndex].colIndex = secondSelected.colIndex;

        mergedShip.Initialize();
        mergedShip.SetDebugText();
    }

    private IEnumerator StartSpawnShips()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(newShipSpawnPeriod.x, newShipSpawnPeriod.y));

        StartCoroutine(ForceRandomSpawn());

        yield return StartCoroutine(StartSpawnShips());
    }

    private IEnumerator ForceRandomSpawn()
    {
        yield return new WaitForEndOfFrame();

        int rndRowIndex = UnityEngine.Random.Range(0, shipGrid.Length);
        int rndColIndex = UnityEngine.Random.Range(0, shipGrid[rndRowIndex].Length);

        if (shipGrid[rndRowIndex][rndColIndex] == null)
        {
            SpawnNewShipAndSetUpOnGrid(rndRowIndex, rndColIndex);
            yield return null;
        }
        else
        {
            StartCoroutine(ForceRandomSpawn());
        }
    }

    private void SpawnNewShipAndSetUpOnGrid(int rndRowIndex, int rndColIndex)
    {
        Ship newShip = Instantiate(shipPrefabsDict[ShipType.White], shipsParent).GetComponent<Ship>();
        shipGrid[rndRowIndex][rndColIndex] = newShip;
        shipGrid[rndRowIndex][rndColIndex].transform.position = positionsGrid[rndRowIndex][rndColIndex];
        shipGrid[rndRowIndex][rndColIndex].rowIndex = rndRowIndex;
        shipGrid[rndRowIndex][rndColIndex].colIndex = rndColIndex;

        shipGrid[rndRowIndex][rndColIndex].Initialize();
        shipGrid[rndRowIndex][rndColIndex].SetDebugText();
    }
}
