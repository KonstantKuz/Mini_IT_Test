using UnityEngine;

public interface IGridPoint
{
    int rowIndex { get; set; }
    int columnIndex { get; set; }
    void SetDebugText();
    void Initialize();
}