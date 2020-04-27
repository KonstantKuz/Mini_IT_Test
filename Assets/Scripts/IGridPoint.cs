using UnityEngine;

public interface IGridPoint
{
    int rowIndex { get; set; }
    int colIndex { get; set; }
    void SetDebugText();
    void Initialize();
}