
public interface IGridPoint
{
    int rowIndex { get; set; }
    int colIndex { get; set; }
    void Initialize();
    void SetDebugText();
}