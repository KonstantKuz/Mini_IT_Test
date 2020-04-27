using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum ShipType
{
    White = 0,
    Black = 1,
    Red = 2,
    Green = 3,
    Blue = 4,
    Yellow = 5,
}

[System.Serializable]
public class ShipData
{
    [SerializeField] private ShipType shipType = 0;
    [SerializeField] private float moveTime = 0.75f;
    [SerializeField] private Color color;

    public ShipType ShipType { get { return shipType; } set { shipType = value; } }
    public float MoveTime { get { return moveTime; } }
    public Color Color { get { return color; } }
}


public class Ship : MonoBehaviour, IGridPoint
{
    [Header("Components to cache")]
    [SerializeField] private Button thisButton = null;
    [SerializeField] private Image image = null;
    [SerializeField] private Text indexesDebugText = null;
    public ShipData data = null;

    public int rowIndex { get; set; }
    public int columnIndex { get; set; }

    public float MoveTime { get { return data.MoveTime; } }

    public void Initialize()
    {
        image.color = data.Color;

        thisButton.onClick.AddListener(() => InvokeSelection());
    }

    private void InvokeSelection()
    {
        MergeField.OnShipSelected(this);
    }

    public void MoveTo(Vector3 position)
    {
        SetDebugText();
        transform.DOMove(position, data.MoveTime);
    }

    public void SetDebugText()
    {
        if (indexesDebugText)
        {
            indexesDebugText.text = $"({rowIndex},{columnIndex})";
        }
    }
}