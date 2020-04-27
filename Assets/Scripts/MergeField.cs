using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MergeField : MonoBehaviour
{
    [SerializeField] private SimpleGridGenerator generator = null;
    [SerializeField] private List<GameObject> shipPrefabs = null;
    [SerializeField] private Vector2 newShipSpawnPeriod;

    private Ship[][] shipGrid = null;
    private Vector3[][] positionsGrid = null;

    private Ship firstSelected = null;
    private Ship secondSelected = null;

    public static Action<Ship> OnShipSelected;

    private Dictionary<ShipType, GameObject> shipPrefabsDict = null;

    private void Start()
    {
        Initialize();
        StartCoroutine(StartSpawnShips());
    }
    
    public void Initialize()
    {
        SetUpPrefabsDictionary();
        GetPositionsGrid();
        GetEmptyShipGrid();

        OnShipSelected += FillSelections;
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

    private void FillSelections(Ship selectedShip)
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

    private void TryMergeShips()
    {
        if (SelectedAreNeighbors() && SelectedAreIdentical() && SelectedAreNotTheSame())
        {
            StartCoroutine(MergeShips());
        }
        else
        {
            ResetSelections();
        }
    }

    private bool SelectedAreNeighbors()
    {
        return !(Mathf.Abs(firstSelected.rowIndex - secondSelected.rowIndex) > 1 || Mathf.Abs(firstSelected.columnIndex - secondSelected.columnIndex) > 1);
    }

    private bool SelectedAreIdentical()
    {
        return firstSelected.data.ShipType == secondSelected.data.ShipType;
    }

    private bool SelectedAreNotTheSame()
    {
        return firstSelected != secondSelected;
    }

    private void ResetSelections()
    {
        firstSelected = null;
        secondSelected = null;
    }

    private IEnumerator MergeShips()
    {
        firstSelected.MoveTo(secondSelected.transform.position);
        yield return new WaitForSeconds(firstSelected.MoveTime);

        ShipType mergeResult = (ShipType)((int)firstSelected.data.ShipType + 1);
        Ship mergedShip = Instantiate(shipPrefabsDict[mergeResult], transform).GetComponent<Ship>();

        AffectMergeResult(mergedShip);

        Destroy(firstSelected.gameObject);
        Destroy(secondSelected.gameObject);

        ResetSelections();
    }

    private void AffectMergeResult(Ship mergedShip)
    {
        mergedShip.transform.position = positionsGrid[secondSelected.rowIndex][secondSelected.columnIndex];

        shipGrid[firstSelected.rowIndex][firstSelected.columnIndex] = null;
        shipGrid[secondSelected.rowIndex][secondSelected.columnIndex] = mergedShip;
        shipGrid[secondSelected.rowIndex][secondSelected.columnIndex].rowIndex = secondSelected.rowIndex;
        shipGrid[secondSelected.rowIndex][secondSelected.columnIndex].columnIndex = secondSelected.columnIndex;

        mergedShip.Initialize();
        mergedShip.SetDebugText();
    }

    private IEnumerator StartSpawnShips()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(newShipSpawnPeriod.x, newShipSpawnPeriod.y));

        StartCoroutine(SpawnNewShipForcibly());

        yield return StartCoroutine(StartSpawnShips());
    }

    private IEnumerator SpawnNewShipForcibly()
    {
        yield return new WaitForEndOfFrame();

        int rndRowIndex = UnityEngine.Random.Range(0, shipGrid.Length);
        int rndColIndex = UnityEngine.Random.Range(0, shipGrid[0].Length);

        if (shipGrid[rndRowIndex][rndColIndex] == null)
        {
            SpawnNewShipAndSetUpOnGrid(rndRowIndex, rndColIndex);
            yield return null;
        }
        else
        {
            yield return StartCoroutine(SpawnNewShipForcibly());
        }
    }

    private void SpawnNewShipAndSetUpOnGrid(int rndRowIndex, int rndColIndex)
    {
        Ship newShip = Instantiate(shipPrefabsDict[ShipType.White], transform).GetComponent<Ship>();
        shipGrid[rndRowIndex][rndColIndex] = newShip;
        shipGrid[rndRowIndex][rndColIndex].transform.position = positionsGrid[rndRowIndex][rndColIndex];
        shipGrid[rndRowIndex][rndColIndex].rowIndex = rndRowIndex;
        shipGrid[rndRowIndex][rndColIndex].columnIndex = rndColIndex;
        shipGrid[rndRowIndex][rndColIndex].Initialize();
        shipGrid[rndRowIndex][rndColIndex].SetDebugText();
    }
}
