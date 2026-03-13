using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerPlacementManager : PhaseListener
{
    private Camera mainCamera;

    [SerializeField] private Tilemap buildableTilemap;
    [SerializeField] private GameObject towerPrefab;

    private readonly Dictionary<Vector3Int, Tower> placedTowers = new();

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
            MouseInputHandler.Instance.OnMouseClick += HandleMouseClick;
        }
    }

    protected override void HandlePhaseExited(InGamePhase phase)
    {
        if (phase == InGamePhase.BuildSelect)
        {
            isBuildPhase = false;
            MouseInputHandler.Instance.OnMouseClick -= HandleMouseClick;
        }
    }

    private void HandleMouseClick(Vector3 mouseScreenPos)
    {
        if (!isBuildPhase) return;

        // ← 여기서 드래그 중이면 클릭 무시
        //if (MouseInputHandler.Instance.IsDragging)
        //    return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3Int cellPos = buildableTilemap.WorldToCell(mouseWorldPos);

        if (!CanPlaceAt(cellPos))
            return;

        PlaceTower(cellPos);
    }

    public bool CanPlaceAt(Vector3Int cellPos)
    {
        if (!buildableTilemap.HasTile(cellPos))
        {
            return false;
        }

        if (placedTowers.ContainsKey(cellPos))
        {
            return false;
        }

        return true;
    }

    public bool PlaceTower(Vector3Int cellPos)
    {
        Vector3 spawnWorldPos = buildableTilemap.GetCellCenterWorld(cellPos);
        GameObject towerObject = Instantiate(towerPrefab, spawnWorldPos, Quaternion.identity);

        Tower tower = towerObject.GetComponent<Tower>();

        if (tower == null)
        {
            Destroy(towerObject);
            return false;
        }

        tower.Initialize(cellPos);
        placedTowers.Add(cellPos, tower);

        return true;
    }

    public bool RemoveTower(Vector3Int cellPos)
    {
        if (!placedTowers.TryGetValue(cellPos, out Tower tower))
        {
            return false;
        }

        placedTowers.Remove(cellPos);
        Destroy(tower.gameObject);
        return true;
    }

    public bool HasTowerAt(Vector3Int cellPos)
    {
        return placedTowers.ContainsKey(cellPos);
    }

    public Tower GetTowerAt(Vector3Int cellPos)
    {
        if (placedTowers.TryGetValue(cellPos, out Tower tower))
            return tower;

        return null;
    }
}