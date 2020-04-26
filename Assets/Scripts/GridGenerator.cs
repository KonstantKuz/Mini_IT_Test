using UnityEngine;

public abstract class GridGenerator : MonoBehaviour 
{
    public abstract Crystal[][] GenerateGrid(Field field);
}