using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerPlacementManager : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private Tilemap buildableTilemap;
    [SerializeField] private GameObject towerPrefab;

    private readonly Dictionary<Vector3Int, Tower> placedTowers = new Dictionary<Vector3Int, Tower>();

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 추후 new input으로 교체하기.
            TryPlaceTower();
        }
    }

    private void TryPlaceTower()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3Int cellPos = buildableTilemap.WorldToCell(mouseWorldPos);

        if (CanPlaceAt(cellPos) == false)
        {
            return;
        }

        PlaceTower(cellPos);
    }

    public bool CanPlaceAt(Vector3Int cellPos)
    {
        if (buildableTilemap.HasTile(cellPos) == false)
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
        if (placedTowers.TryGetValue(cellPos, out Tower tower) == false)
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
        {
            return tower;
        }
        return null;
    }
}