using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameProgressController : MonoBehaviour
{
    [SerializeField] private int mergeCountToWin = 10;
    private int currentMergedShipsCount = 0;

    InitialData<LevelProgress> initialData;
    UpdateData<LevelProgress> updateData;

    private void OnEnable()
    {
        LevelProgress.OnProgressFinished += delegate { FinishGame(); } ;
        MergeField.OnShipsMerged += UpdateProgress;
    }

    private void OnDisable()
    {
        LevelProgress.OnProgressFinished -= delegate { FinishGame(); };
        MergeField.OnShipsMerged -= UpdateProgress;
    }

    private void Start()
    {
        InitializeLevelProgress();
    }

    private void InitializeLevelProgress()
    {
        currentMergedShipsCount = 0;

        initialData.MinValue = 0;
        initialData.MaxValue = mergeCountToWin;
        initialData.CurrentValue = currentMergedShipsCount;
        LevelProgress.InitializeProgress(initialData);
    }

    private void UpdateProgress()
    {
        currentMergedShipsCount++;
        updateData.CurrentValue = currentMergedShipsCount;
        LevelProgress.UpdateProgress(updateData);
    }

    private void FinishGame()
    {
        Debug.Log("Win!");
    }
}
