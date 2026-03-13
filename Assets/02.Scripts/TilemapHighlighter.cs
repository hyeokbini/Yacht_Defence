using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapHighlighter : PhaseListener
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
    private bool isBuildPhase = false;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    protected override void HandlePhaseEntered(InGamePhase phase)
    {
        if (phase == InGamePhase.BuildSelect)
        {
            isBuildPhase = true;
            MouseInputHandler.Instance.OnMouseMove += HandleMouseMove;
        }
    }

    protected override void HandlePhaseExited(InGamePhase phase)
    {
        if (phase == InGamePhase.BuildSelect)
        {
            isBuildPhase = false;
            MouseInputHandler.Instance.OnMouseMove -= HandleMouseMove;
            ClearCurrentHighlight();
        }
    }

    /*protected override void OnEnable()
    {
        base.OnEnable();
        MouseInputHandler.Instance.OnMouseMove += HandleMouseMove;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        MouseInputHandler.Instance.OnMouseMove -= HandleMouseMove;
    }*/



    private void HandleMouseMove(Vector3 mouseScreenPos)
    {
        if (!isBuildPhase)
            return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3Int cellPos = buildableTilemap.WorldToCell(mouseWorldPos);
        UpdateHighlight(cellPos);
    }

    private void UpdateHighlight(Vector3Int cellPos)
    {

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
        if (!currentCell.HasValue)
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