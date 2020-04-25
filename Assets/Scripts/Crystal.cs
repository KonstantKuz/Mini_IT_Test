using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public enum CrystalType
{
    White = 0,
    Black = 1,
    Red = 2,
    Green = 3,
    Blue = 4,
    Yellow = 5,
}

public class Crystal : MonoBehaviour
{
    public int x = 0;
    public int y = 0;
    public CrystalType crystalType = 0;

    [SerializeField] private Button thisButton = null;

    private void Start()
    {
        thisButton.onClick.AddListener(() => InvokeSelection());
    }

    private void InvokeSelection()
    {
        Field.OnCrystalSelected(this);
    }

    public void MoveTo(Vector3 position, bool invoke)
    {
        transform.DOMove(position, 0.75f);
        if(invoke)
        {
            StartCoroutine(OnMovingDoneInvoke());
        }
    }

    private IEnumerator OnMovingDoneInvoke()
    {
        yield return new WaitForSeconds(1f);
        Field.OnMovingDone();
        SetDebugText();
    }

    public void SetDebugText()
    {
        GetComponentInChildren<Text>().text = $"({x},{y})";
    }
}