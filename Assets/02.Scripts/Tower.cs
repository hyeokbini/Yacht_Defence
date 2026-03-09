using UnityEngine;

public class Tower : MonoBehaviour
{
    public Vector3Int PlacedCellPosition { get; private set; }

    public void Initialize(Vector3Int cellPosition)
    {
        PlacedCellPosition = cellPosition;
    }
}