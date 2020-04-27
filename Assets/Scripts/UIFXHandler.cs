using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[System.Serializable]
public class ProgressBarAnimProperties
{
    public RectTransform progressBarRect = null;
    public int sizeDeltaOnAnim;

    [HideInInspector] public Vector2 sizeDelta;
}

[System.Serializable]
public class WinTextAnimProperties
{
    public RectTransform winTextRect = null;
    public int sizeDeltaOnAnim;

    [HideInInspector] public Vector2 sizeDelta;
}

public class UIFXHandler : MonoBehaviour
{
    [SerializeField] private WinTextAnimProperties winTextProperties;
    [SerializeField] private ProgressBarAnimProperties progressBarProperties;


    private void OnEnable()
    {
        LevelProgress.OnProgressFinished += delegate { AnimateWinText(); };
        MergeField.OnShipsMerged += delegate { StartCoroutine(AnimateProgressBar()); };
    }

    private void OnDisable()
    {
        LevelProgress.OnProgressFinished -= delegate { AnimateWinText(); };
        MergeField.OnShipsMerged -= delegate { StartCoroutine(AnimateProgressBar()); };
    }

    private void Start()
    {
        InitSizeDeltas();
    }

    private void InitSizeDeltas()
    {
        winTextProperties.sizeDelta = new Vector2(winTextProperties.sizeDeltaOnAnim, winTextProperties.sizeDeltaOnAnim);
        progressBarProperties.sizeDelta = new Vector2(progressBarProperties.sizeDeltaOnAnim, progressBarProperties.sizeDeltaOnAnim);
    }

    private IEnumerator AnimateProgressBar()
    {
        progressBarProperties.progressBarRect.DOSizeDelta(progressBarProperties.progressBarRect.sizeDelta + progressBarProperties.sizeDelta, 0.5f);
        yield return new WaitForSeconds(0.5f);
        progressBarProperties.progressBarRect.DOSizeDelta(progressBarProperties.progressBarRect.sizeDelta - progressBarProperties.sizeDelta, 0.5f);
    }

    private void AnimateWinText()
    {
        winTextProperties.winTextRect.DOSizeDelta(winTextProperties.winTextRect.sizeDelta + winTextProperties.sizeDelta, 0.5f);
    }

}
