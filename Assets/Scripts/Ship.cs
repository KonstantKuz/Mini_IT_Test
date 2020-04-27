using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

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


public class Ship : MonoBehaviour, IGridPoint, IPointerClickHandler, IDragHandler
{
    [SerializeField] private bool debugIndexes = true;
    [Header("Components to cache")]
    [SerializeField] private Button thisButton = null;
    [SerializeField] private Image image = null;
    [SerializeField] private Text indexesDebugText = null;
    public ShipData data = null;

    public int rowIndex { get; set; }
    public int colIndex { get; set; }

    public float MoveTime { get { return data.MoveTime; } }

    private bool canDrag = true;

    public void Initialize()
    {
        image.color = data.Color;
        if(debugIndexes)
        {
            indexesDebugText.gameObject.SetActive(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InvokeSelection();
    }

    public void OnDrag(PointerEventData eventData)
    {
        InvokeDragging(eventData.delta);
    }

    private void InvokeSelection()
    {
        MergeField.OnShipSelected?.Invoke(this);
    }

    private void InvokeDragging(Vector2 dragDirection)
    {
        if(canDrag)
        {
            MergeField.OnShipDragged?.Invoke((this, dragDirection));
            StartCoroutine(FreezeOn(0.5f));
        }
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
            indexesDebugText.text = $"({rowIndex},{colIndex})";
        }
    }

    private IEnumerator FreezeOn(float delay)
    {
        canDrag = false;
        yield return new WaitForSeconds(delay);
        canDrag = true;
    }
}