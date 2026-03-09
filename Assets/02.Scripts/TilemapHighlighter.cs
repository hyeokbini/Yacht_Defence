using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapHighlighter : MonoBehaviour
{
    [SerializeField] private Tilemap buildableTilemap;
    [SerializeField] private Tilemap fillHighlightTilemap;
    [SerializeField] private Tilemap borderHighlightTilemap;
    [SerializeField] private TowerPlacementManager placementManager;

    [SerializeField] private TileBase fillTile;
    [SerializeField] private TileBase borderTile;

    [SerializeField] private Color validFillColor = new Color(0.25f, 0.95f, 0.35f, 0.22f);
    [SerializeField] private Color invalidFillColor = new Color(1.00f, 0.35f, 0.35f, 0.22f);
    [SerializeField] private Color validBorderColor = new Color(0.25f, 1.00f, 0.35f, 0.95f);
    [SerializeField] private Color invalidBorderColor = new Color(1.00f, 0.30f, 0.30f, 0.95f);

    private Camera mainCamera;

    private Vector3Int? currentCell;
    private bool? lastCanPlace;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3Int cellPos = buildableTilemap.WorldToCell(mouseWorldPos);
        bool canPlace = placementManager.CanPlaceAt(cellPos);

        bool isSameCell = currentCell.HasValue && currentCell.Value == cellPos;
        bool isSameState = lastCanPlace.HasValue && lastCanPlace.Value == canPlace;

        if (isSameCell && isSameState)
        {
            return;
        }

        ClearCurrentHighlight();

        currentCell = cellPos;
        lastCanPlace = canPlace;

        Color fillColor = canPlace ? validFillColor : invalidFillColor;
        Color borderColor = canPlace ? validBorderColor : invalidBorderColor;

        fillHighlightTilemap.SetTile(cellPos, fillTile);
        borderHighlightTilemap.SetTile(cellPos, borderTile);

        fillHighlightTilemap.SetTileFlags(cellPos, TileFlags.None);
        borderHighlightTilemap.SetTileFlags(cellPos, TileFlags.None);

        fillHighlightTilemap.SetColor(cellPos, fillColor);
        borderHighlightTilemap.SetColor(cellPos, borderColor);
    }

    private void ClearCurrentHighlight()
    {
        if (currentCell.HasValue == false)
        {
            return;
        }

        Vector3Int cellPos = currentCell.Value;

        fillHighlightTilemap.SetTile(cellPos, null);
        borderHighlightTilemap.SetTile(cellPos, null);

        currentCell = null;
        lastCanPlace = null;
    }
}