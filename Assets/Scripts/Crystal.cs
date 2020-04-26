using System.Collections;
using System.Collections.Generic;
using System;
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

public interface IGridPoint
{
    int rowIndex { get; set; }
    int columnIndex { get; set; }
    void SetDebugText();
    void Initialize();
}

[System.Serializable]
public class CrystalData
{
    [SerializeField] private CrystalType crystalType = 0;
    [SerializeField] private float moveTime = 0.75f;
    [SerializeField] private Color color;

    public CrystalType CrystalType { get { return crystalType; } }
    public float MoveTime { get { return moveTime; } }
    public Color Color { get { return color; } }
}

public class Crystal : MonoBehaviour, IGridPoint
{
    [Header("Components to cache")]
    [SerializeField] private Button thisButton = null;
    [SerializeField] private Image image = null;
    [SerializeField] private Text indexesDebugText = null;

    public int rowIndex { get; set; }
    public int columnIndex { get; set; }
    public Transform Transform { get { return transform; } }
    public CrystalData data;
    
    public float MoveTime { get { return data.MoveTime; } }

    public void Initialize()
    {
        image.color = data.Color;

        thisButton.onClick.AddListener(() => InvokeSelection());
    }

    private void InvokeSelection()
    {
        Match3Field.OnCrystalSelected(this as Crystal);
    }

    public void MoveTo(Vector3 position)
    {
        SetDebugText();
        Transform.DOMove(position, data.MoveTime);
    }

    public void SetDebugText()
    {
        if(indexesDebugText)
        {
            indexesDebugText.text = $"({rowIndex},{columnIndex})";
        }
    }
}